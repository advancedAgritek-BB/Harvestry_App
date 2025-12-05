using Harvestry.Irrigation.Domain.Entities;
using Harvestry.Irrigation.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Harvestry.Irrigation.Application.Services;

/// <summary>
/// Service for managing irrigation event queuing based on flow rate constraints
/// </summary>
public interface IIrrigationQueueService
{
    /// <summary>
    /// Evaluate if an irrigation event should be queued or executed immediately
    /// </summary>
    Task<QueueEvaluationResult> EvaluateEventAsync(
        Guid siteId,
        Guid programId,
        IEnumerable<Guid> targetZoneIds,
        DateTime scheduledTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queue an irrigation event for later execution
    /// </summary>
    Task<QueuedIrrigationEvent> QueueEventAsync(
        Guid siteId,
        Guid programId,
        Guid? scheduleId,
        IEnumerable<Guid> targetZoneIds,
        DateTime originalScheduledTime,
        DateTime expectedExecutionTime,
        string queueReason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all queued events for a site
    /// </summary>
    Task<IReadOnlyList<QueuedIrrigationEvent>> GetQueuedEventsAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Process the queue and start any events that can now run
    /// </summary>
    Task<int> ProcessQueueAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get queue statistics for analyzing patterns
    /// </summary>
    Task<QueueStatistics> GetQueueStatisticsAsync(
        Guid siteId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);
}

public sealed class IrrigationQueueService : IIrrigationQueueService
{
    private readonly IFlowRateCalculationService _flowRateService;
    private readonly IIrrigationSettingsRepository _settingsRepository;
    private readonly IQueuedIrrigationEventRepository _queueRepository;
    private readonly IIrrigationOrchestratorService _orchestratorService;
    private readonly ILogger<IrrigationQueueService> _logger;

    public IrrigationQueueService(
        IFlowRateCalculationService flowRateService,
        IIrrigationSettingsRepository settingsRepository,
        IQueuedIrrigationEventRepository queueRepository,
        IIrrigationOrchestratorService orchestratorService,
        ILogger<IrrigationQueueService> logger)
    {
        _flowRateService = flowRateService;
        _settingsRepository = settingsRepository;
        _queueRepository = queueRepository;
        _orchestratorService = orchestratorService;
        _logger = logger;
    }

    public async Task<QueueEvaluationResult> EvaluateEventAsync(
        Guid siteId,
        Guid programId,
        IEnumerable<Guid> targetZoneIds,
        DateTime scheduledTime,
        CancellationToken cancellationToken = default)
    {
        var settings = await _settingsRepository.GetBySiteIdAsync(siteId, cancellationToken);
        
        // If queuing is disabled, always allow immediate execution
        if (settings == null || !settings.EnableFlowRateQueuing)
        {
            return QueueEvaluationResult.ExecuteImmediately();
        }

        var zoneIdList = targetZoneIds.ToList();
        var flowRateCheck = await _flowRateService.CheckFlowRateAsync(siteId, zoneIdList, cancellationToken);

        if (flowRateCheck.IsAllowed)
        {
            return QueueEvaluationResult.ExecuteImmediately();
        }

        // Calculate expected execution time based on queue state
        var queuedEvents = await _queueRepository.GetPendingEventsAsync(siteId, cancellationToken);
        var expectedExecutionTime = CalculateExpectedExecutionTime(
            scheduledTime,
            flowRateCheck.EstimatedWaitTime ?? TimeSpan.FromMinutes(5),
            queuedEvents);

        var delay = expectedExecutionTime - scheduledTime;
        var queuePosition = queuedEvents.Count + 1;

        return QueueEvaluationResult.ShouldQueue(
            expectedExecutionTime,
            delay,
            queuePosition,
            $"Flow rate limit exceeded: {flowRateCheck.ProjectedFlowRateLitersPerMinute:F1} L/min would exceed {flowRateCheck.EffectiveMaxFlowRateLitersPerMinute:F1} L/min limit");
    }

    public async Task<QueuedIrrigationEvent> QueueEventAsync(
        Guid siteId,
        Guid programId,
        Guid? scheduleId,
        IEnumerable<Guid> targetZoneIds,
        DateTime originalScheduledTime,
        DateTime expectedExecutionTime,
        string queueReason,
        CancellationToken cancellationToken = default)
    {
        var queuedEvent = QueuedIrrigationEvent.Create(
            siteId,
            programId,
            scheduleId,
            targetZoneIds.ToArray(),
            originalScheduledTime,
            expectedExecutionTime,
            queueReason);

        await _queueRepository.AddAsync(queuedEvent, cancellationToken);

        _logger.LogInformation(
            "Queued irrigation event for program {ProgramId}. Original time: {Original}, Expected: {Expected}, Delay: {Delay}",
            programId,
            originalScheduledTime,
            expectedExecutionTime,
            queuedEvent.DelayDuration.FormattedDelay);

        return queuedEvent;
    }

    public async Task<IReadOnlyList<QueuedIrrigationEvent>> GetQueuedEventsAsync(
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        return await _queueRepository.GetPendingEventsAsync(siteId, cancellationToken);
    }

    public async Task<int> ProcessQueueAsync(
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var queuedEvents = await _queueRepository.GetPendingEventsAsync(siteId, cancellationToken);
        var processedCount = 0;

        foreach (var queuedEvent in queuedEvents.OrderBy(e => e.ExpectedExecutionTime))
        {
            // Re-check flow rate for this event
            var flowCheck = await _flowRateService.CheckFlowRateAsync(
                siteId,
                queuedEvent.TargetZoneIds,
                cancellationToken);

            if (flowCheck.IsAllowed)
            {
                try
                {
                    // Start the irrigation run
                    await _orchestratorService.StartRunAsync(
                        queuedEvent.ProgramId,
                        queuedEvent.ScheduleId,
                        null, // system triggered
                        cancellationToken);

                    // Mark as executed
                    queuedEvent.MarkExecuted();
                    await _queueRepository.UpdateAsync(queuedEvent, cancellationToken);
                    processedCount++;

                    _logger.LogInformation(
                        "Processed queued event for program {ProgramId} after {Delay} delay",
                        queuedEvent.ProgramId,
                        queuedEvent.DelayDuration.FormattedDelay);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process queued event {EventId}", queuedEvent.Id);
                    queuedEvent.MarkFailed(ex.Message);
                    await _queueRepository.UpdateAsync(queuedEvent, cancellationToken);
                }
            }
            else
            {
                // Update expected execution time
                var newExpectedTime = DateTime.UtcNow.Add(flowCheck.EstimatedWaitTime ?? TimeSpan.FromMinutes(5));
                queuedEvent.UpdateExpectedExecutionTime(newExpectedTime);
                await _queueRepository.UpdateAsync(queuedEvent, cancellationToken);
            }
        }

        return processedCount;
    }

    public async Task<QueueStatistics> GetQueueStatisticsAsync(
        Guid siteId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        var events = await _queueRepository.GetEventsInRangeAsync(siteId, fromDate, toDate, cancellationToken);

        var totalQueued = events.Count;
        var totalDelayMinutes = events.Sum(e => e.DelayDuration.DelayDuration.TotalMinutes);
        var averageDelayMinutes = totalQueued > 0 ? totalDelayMinutes / totalQueued : 0;
        var maxDelayMinutes = events.Any() ? events.Max(e => e.DelayDuration.DelayDuration.TotalMinutes) : 0;

        // Group by hour of day to find patterns
        var queuesByHour = events
            .GroupBy(e => e.OriginalScheduledTime.Hour)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => new HourlyQueuePattern(g.Key, g.Count()))
            .ToList();

        return new QueueStatistics(
            totalQueued,
            TimeSpan.FromMinutes(averageDelayMinutes),
            TimeSpan.FromMinutes(maxDelayMinutes),
            queuesByHour);
    }

    private DateTime CalculateExpectedExecutionTime(
        DateTime scheduledTime,
        TimeSpan estimatedWait,
        IReadOnlyList<QueuedIrrigationEvent> existingQueue)
    {
        var baseTime = DateTime.UtcNow > scheduledTime ? DateTime.UtcNow : scheduledTime;
        var expectedTime = baseTime.Add(estimatedWait);

        // Ensure we don't overlap with other queued events
        foreach (var queued in existingQueue.OrderBy(e => e.ExpectedExecutionTime))
        {
            if (Math.Abs((queued.ExpectedExecutionTime - expectedTime).TotalMinutes) < 2)
            {
                // Add 2 minute buffer if times are too close
                expectedTime = queued.ExpectedExecutionTime.AddMinutes(2);
            }
        }

        return expectedTime;
    }
}

/// <summary>
/// Result of evaluating whether an event should be queued
/// </summary>
public sealed record QueueEvaluationResult
{
    public bool ShouldExecuteImmediately { get; init; }
    public DateTime? ExpectedExecutionTime { get; init; }
    public TimeSpan DelayDuration { get; init; }
    public int QueuePosition { get; init; }
    public string? QueueReason { get; init; }

    public static QueueEvaluationResult ExecuteImmediately()
    {
        return new QueueEvaluationResult
        {
            ShouldExecuteImmediately = true,
            DelayDuration = TimeSpan.Zero,
            QueuePosition = 0
        };
    }

    public static QueueEvaluationResult ShouldQueue(
        DateTime expectedTime,
        TimeSpan delay,
        int position,
        string reason)
    {
        return new QueueEvaluationResult
        {
            ShouldExecuteImmediately = false,
            ExpectedExecutionTime = expectedTime,
            DelayDuration = delay,
            QueuePosition = position,
            QueueReason = reason
        };
    }
}

/// <summary>
/// Statistics about queue usage for analysis
/// </summary>
public sealed record QueueStatistics(
    int TotalQueuedEvents,
    TimeSpan AverageDelay,
    TimeSpan MaxDelay,
    IReadOnlyList<HourlyQueuePattern> PeakQueueHours);

public sealed record HourlyQueuePattern(int Hour, int QueueCount);




