using Harvestry.Telemetry.Domain.Enums;

namespace Harvestry.Telemetry.Domain.ValueObjects;

/// <summary>
/// Represents a batch of sensor readings for bulk ingestion.
/// Tracks batch statistics for monitoring and optimization.
/// </summary>
public readonly record struct IngestionBatch
{
    public Guid BatchId { get; init; }
    public Guid EquipmentId { get; init; }
    public IngestionProtocol Protocol { get; init; }
    public int TotalReadings { get; init; }
    public int AcceptedReadings { get; init; }
    public int DuplicateReadings { get; init; }
    public int ErrorReadings { get; init; }
    public TimeSpan ProcessingDuration { get; init; }
    public DateTimeOffset ReceivedAt { get; init; }
    public DateTimeOffset ProcessedAt { get; init; }
    
    public IngestionBatch(Guid equipmentId, IngestionProtocol protocol, int totalReadings)
    {
        BatchId = Guid.NewGuid();
        EquipmentId = equipmentId;
        Protocol = protocol;
        TotalReadings = totalReadings;
        AcceptedReadings = 0;
        DuplicateReadings = 0;
        ErrorReadings = 0;
        ProcessingDuration = TimeSpan.Zero;
        ReceivedAt = DateTimeOffset.UtcNow;
        ProcessedAt = DateTimeOffset.UtcNow;
    }
    
    /// <summary>
    /// Creates result after processing batch.
    /// </summary>
    public IngestionBatch WithResults(int accepted, int duplicates, int errors, TimeSpan duration)
    {
        return this with
        {
            AcceptedReadings = accepted,
            DuplicateReadings = duplicates,
            ErrorReadings = errors,
            ProcessingDuration = duration,
            ProcessedAt = DateTimeOffset.UtcNow
        };
    }
    
    /// <summary>
    /// Gets success rate as percentage.
    /// </summary>
    public double GetSuccessRate()
    {
        if (TotalReadings == 0) return 0;
        return (AcceptedReadings / (double)TotalReadings) * 100.0;
    }
    
    /// <summary>
    /// Gets throughput in readings per second.
    /// </summary>
    public double GetThroughput()
    {
        if (ProcessingDuration.TotalSeconds == 0) return 0;
        return TotalReadings / ProcessingDuration.TotalSeconds;
    }
    
    /// <summary>
    /// Checks if batch meets quality thresholds.
    /// </summary>
    public bool MeetsQualityThreshold(double minimumSuccessRate = 95.0)
    {
        return GetSuccessRate() >= minimumSuccessRate;
    }
}

