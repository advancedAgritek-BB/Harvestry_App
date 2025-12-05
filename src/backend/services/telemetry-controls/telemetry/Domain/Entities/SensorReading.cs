using Harvestry.Telemetry.Domain.Enums;

namespace Harvestry.Telemetry.Domain.Entities;

/// <summary>
/// Represents a single sensor reading at a point in time.
/// Composite key: (Time, StreamId) for TimescaleDB hypertable.
/// </summary>
public class SensorReading : Entity<(DateTimeOffset Time, Guid StreamId)>
{
    public DateTimeOffset Time { get; private set; }
    public Guid StreamId { get; private set; }
    public double Value { get; private set; }
    public QualityCode QualityCode { get; private set; }
    public DateTimeOffset? SourceTimestamp { get; private set; }
    public DateTimeOffset IngestionTimestamp { get; private set; }
    public string? MessageId { get; private set; }
    public Dictionary<string, object>? Metadata { get; private set; }
    
    // For EF Core
    private SensorReading() { }
    
    private SensorReading(
        DateTimeOffset time,
        Guid streamId,
        double value,
        QualityCode qualityCode)
    {
        Time = time;
        StreamId = streamId;
        Id = (time, streamId);
        Value = value;
        QualityCode = qualityCode;
        IngestionTimestamp = DateTimeOffset.UtcNow;
    }
    
    /// <summary>
    /// Creates a sensor reading from device ingestion.
    /// Performs validation and quality code determination.
    /// </summary>
    public static SensorReading FromIngestion(
        Guid streamId,
        double value,
        QualityCode qualityCode,
        DateTimeOffset? sourceTimestamp = null,
        string? messageId = null,
        Dictionary<string, object>? metadata = null)
    {
        var time = sourceTimestamp ?? DateTimeOffset.UtcNow;
        
        // Validate timestamp (reject future timestamps beyond clock skew tolerance)
        if (time > DateTimeOffset.UtcNow.AddMinutes(5))
        {
            qualityCode = QualityCode.BadFutureTimestamp;
        }
        
        var reading = new SensorReading(time, streamId, value, qualityCode)
        {
            SourceTimestamp = sourceTimestamp,
            MessageId = messageId,
            Metadata = metadata
        };
        
        return reading;
    }
    
    /// <summary>
    /// Rehydrates sensor reading from persistence layer.
    /// </summary>
    public static SensorReading FromPersistence(
        DateTimeOffset time,
        Guid streamId,
        double value,
        QualityCode qualityCode,
        DateTimeOffset? sourceTimestamp,
        DateTimeOffset ingestionTimestamp,
        string? messageId,
        Dictionary<string, object>? metadata)
    {
        return new SensorReading
        {
            Time = time,
            StreamId = streamId,
            Id = (time, streamId),
            Value = value,
            QualityCode = qualityCode,
            SourceTimestamp = sourceTimestamp,
            IngestionTimestamp = ingestionTimestamp,
            MessageId = messageId,
            Metadata = metadata
        };
    }
    
    /// <summary>
    /// Checks if reading is of good quality.
    /// </summary>
    public bool IsGoodQuality() => QualityCode == QualityCode.Good;
    
    /// <summary>
    /// Checks if reading is within expected range.
    /// </summary>
    public bool IsWithinExpectedRange(double min, double max) => Value >= min && Value <= max;
    
    /// <summary>
    /// Gets latency between source timestamp and ingestion.
    /// </summary>
    public TimeSpan? GetIngestionLatency()
    {
        if (!SourceTimestamp.HasValue) return null;
        return IngestionTimestamp - SourceTimestamp.Value;
    }
}

