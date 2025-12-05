using Microsoft.Extensions.Logging;

namespace Harvestry.Irrigation.Application.Services;

/// <summary>
/// Service for generating smart suggestions to optimize irrigation schedules
/// when users repeatedly encounter queue delays
/// </summary>
public interface ISmartSuggestionService
{
    /// <summary>
    /// Analyze queue patterns and generate optimization suggestions
    /// </summary>
    Task<IReadOnlyList<ScheduleSuggestion>> GenerateSuggestionsAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if suggestions should be shown based on queue history
    /// </summary>
    Task<bool> ShouldShowSuggestionsAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);
}

public sealed class SmartSuggestionService : ISmartSuggestionService
{
    private readonly IIrrigationQueueService _queueService;
    private readonly IIrrigationSettingsRepository _settingsRepository;
    private readonly IIrrigationProgramRepository _programRepository;
    private readonly IZoneEmitterConfigurationRepository _emitterRepository;
    private readonly ILogger<SmartSuggestionService> _logger;

    public SmartSuggestionService(
        IIrrigationQueueService queueService,
        IIrrigationSettingsRepository settingsRepository,
        IIrrigationProgramRepository programRepository,
        IZoneEmitterConfigurationRepository emitterRepository,
        ILogger<SmartSuggestionService> logger)
    {
        _queueService = queueService;
        _settingsRepository = settingsRepository;
        _programRepository = programRepository;
        _emitterRepository = emitterRepository;
        _logger = logger;
    }

    public async Task<bool> ShouldShowSuggestionsAsync(
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var settings = await _settingsRepository.GetBySiteIdAsync(siteId, cancellationToken);
        if (settings == null || !settings.EnableSmartSuggestions)
            return false;

        var stats = await _queueService.GetQueueStatisticsAsync(
            siteId,
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow,
            cancellationToken);

        return stats.TotalQueuedEvents >= settings.SuggestionThresholdCount;
    }

    public async Task<IReadOnlyList<ScheduleSuggestion>> GenerateSuggestionsAsync(
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var suggestions = new List<ScheduleSuggestion>();

        // Get queue statistics for the past week
        var stats = await _queueService.GetQueueStatisticsAsync(
            siteId,
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow,
            cancellationToken);

        if (stats.TotalQueuedEvents == 0)
            return suggestions;

        // Analyze peak hours and suggest time shifts
        var peakHourSuggestions = GeneratePeakHourSuggestions(stats);
        suggestions.AddRange(peakHourSuggestions);

        // Analyze zone configurations and suggest sequential scheduling
        var sequentialSuggestions = await GenerateSequentialSchedulingSuggestionsAsync(siteId, stats, cancellationToken);
        suggestions.AddRange(sequentialSuggestions);

        // Analyze if system capacity is consistently exceeded
        var capacitySuggestions = GenerateCapacitySuggestions(stats);
        suggestions.AddRange(capacitySuggestions);

        // Prioritize and limit suggestions
        return suggestions
            .OrderByDescending(s => s.Priority)
            .ThenByDescending(s => s.EstimatedImpactMinutes)
            .Take(5)
            .ToList();
    }

    private IEnumerable<ScheduleSuggestion> GeneratePeakHourSuggestions(QueueStatistics stats)
    {
        foreach (var peakHour in stats.PeakQueueHours.Take(2))
        {
            // Find a less busy hour nearby
            var suggestedHour = FindLessCongestedHour(peakHour.Hour, stats.PeakQueueHours);
            
            if (suggestedHour.HasValue)
            {
                yield return new ScheduleSuggestion
                {
                    Type = SuggestionType.TimeShift,
                    Title = "Shift schedule to reduce congestion",
                    Description = $"Schedules at {FormatHour(peakHour.Hour)} are frequently delayed. " +
                                  $"Consider moving some to {FormatHour(suggestedHour.Value)} to reduce queue times.",
                    CurrentValue = FormatHour(peakHour.Hour),
                    SuggestedValue = FormatHour(suggestedHour.Value),
                    EstimatedImpactMinutes = (int)(stats.AverageDelay.TotalMinutes * 0.5),
                    Priority = SuggestionPriority.High,
                    AffectedPrograms = Array.Empty<Guid>() // Would need to be populated from actual data
                };
            }
        }
    }

    private async Task<IEnumerable<ScheduleSuggestion>> GenerateSequentialSchedulingSuggestionsAsync(
        Guid siteId,
        QueueStatistics stats,
        CancellationToken cancellationToken)
    {
        var suggestions = new List<ScheduleSuggestion>();

        // If average delay is significant, suggest sequential scheduling
        if (stats.AverageDelay.TotalMinutes > 5)
        {
            suggestions.Add(new ScheduleSuggestion
            {
                Type = SuggestionType.SequentialScheduling,
                Title = "Use sequential zone scheduling",
                Description = "Instead of scheduling multiple zones at the same time, " +
                              "stagger zone starts by 5-10 minutes to avoid flow rate limits.",
                CurrentValue = "Simultaneous",
                SuggestedValue = "Sequential (5-10 min stagger)",
                EstimatedImpactMinutes = (int)stats.AverageDelay.TotalMinutes,
                Priority = SuggestionPriority.Medium,
                AffectedPrograms = Array.Empty<Guid>()
            });
        }

        return suggestions;
    }

    private IEnumerable<ScheduleSuggestion> GenerateCapacitySuggestions(QueueStatistics stats)
    {
        // If we're seeing consistent high delays, suggest reviewing system capacity
        if (stats.MaxDelay.TotalMinutes > 30 || stats.TotalQueuedEvents > 20)
        {
            yield return new ScheduleSuggestion
            {
                Type = SuggestionType.SystemCapacity,
                Title = "Review system flow capacity",
                Description = "Your irrigation system is frequently hitting flow rate limits. " +
                              "Consider reviewing your maximum flow rate setting or upgrading system capacity.",
                CurrentValue = "Frequent queue delays",
                SuggestedValue = "Increase max flow rate or upgrade pump/lines",
                EstimatedImpactMinutes = (int)stats.MaxDelay.TotalMinutes,
                Priority = SuggestionPriority.Low,
                AffectedPrograms = Array.Empty<Guid>()
            };
        }
    }

    private int? FindLessCongestedHour(int peakHour, IReadOnlyList<HourlyQueuePattern> patterns)
    {
        var busyHours = patterns.Select(p => p.Hour).ToHashSet();
        
        // Look for nearby hours that are less congested
        for (int offset = 1; offset <= 3; offset++)
        {
            var earlier = (peakHour - offset + 24) % 24;
            var later = (peakHour + offset) % 24;

            // Prefer earlier hours if they're not busy and within reasonable irrigation hours (5-20)
            if (!busyHours.Contains(earlier) && earlier >= 5 && earlier <= 20)
                return earlier;
            
            if (!busyHours.Contains(later) && later >= 5 && later <= 20)
                return later;
        }

        return null;
    }

    private static string FormatHour(int hour)
    {
        var time = new TimeOnly(hour, 0);
        return time.ToString("h tt");
    }
}

/// <summary>
/// A smart suggestion for schedule optimization
/// </summary>
public sealed class ScheduleSuggestion
{
    public SuggestionType Type { get; init; }
    public string Title { get; init; } = null!;
    public string Description { get; init; } = null!;
    public string CurrentValue { get; init; } = null!;
    public string SuggestedValue { get; init; } = null!;
    public int EstimatedImpactMinutes { get; init; }
    public SuggestionPriority Priority { get; init; }
    public Guid[] AffectedPrograms { get; init; } = Array.Empty<Guid>();
}

public enum SuggestionType
{
    TimeShift,
    SequentialScheduling,
    ZoneGrouping,
    SystemCapacity
}

public enum SuggestionPriority
{
    Low,
    Medium,
    High
}



