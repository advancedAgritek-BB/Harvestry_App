namespace Harvestry.Irrigation.Domain.ValueObjects;

/// <summary>
/// Value object representing queuing information for an irrigation event
/// </summary>
public sealed record QueuedEventInfo
{
    public QueuedEventInfo(
        DateTime originalScheduledTime,
        DateTime expectedExecutionTime,
        TimeSpan delayDuration,
        int queuePosition,
        string delayReason)
    {
        if (expectedExecutionTime < originalScheduledTime)
            throw new ArgumentException("Expected execution time cannot be before original scheduled time");
        if (queuePosition < 0)
            throw new ArgumentException("Queue position cannot be negative", nameof(queuePosition));

        OriginalScheduledTime = originalScheduledTime;
        ExpectedExecutionTime = expectedExecutionTime;
        DelayDuration = delayDuration;
        QueuePosition = queuePosition;
        DelayReason = delayReason;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// The original time this event was scheduled to run
    /// </summary>
    public DateTime OriginalScheduledTime { get; }
    
    /// <summary>
    /// The expected time when this event will actually execute
    /// </summary>
    public DateTime ExpectedExecutionTime { get; }
    
    /// <summary>
    /// The delay duration from original to expected time
    /// </summary>
    public TimeSpan DelayDuration { get; }
    
    /// <summary>
    /// Position in the queue (0 = currently running, 1 = next, etc.)
    /// </summary>
    public int QueuePosition { get; }
    
    /// <summary>
    /// Reason for the delay (e.g., "Flow rate limit exceeded")
    /// </summary>
    public string DelayReason { get; }
    
    /// <summary>
    /// When this queue info was created/calculated
    /// </summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    /// Format delay duration for display (e.g., "+5min", "+1h 30min")
    /// </summary>
    public string FormattedDelay
    {
        get
        {
            if (DelayDuration.TotalMinutes < 1)
                return "+<1min";
            
            if (DelayDuration.TotalHours < 1)
                return $"+{(int)DelayDuration.TotalMinutes}min";
            
            var hours = (int)DelayDuration.TotalHours;
            var minutes = DelayDuration.Minutes;
            
            return minutes > 0 
                ? $"+{hours}h {minutes}min" 
                : $"+{hours}h";
        }
    }

    /// <summary>
    /// Check if this event is currently delayed
    /// </summary>
    public bool IsDelayed => DelayDuration > TimeSpan.Zero;

    /// <summary>
    /// Create a non-queued (immediate execution) event info
    /// </summary>
    public static QueuedEventInfo NotQueued(DateTime scheduledTime)
    {
        return new QueuedEventInfo(
            scheduledTime,
            scheduledTime,
            TimeSpan.Zero,
            0,
            string.Empty);
    }
}









