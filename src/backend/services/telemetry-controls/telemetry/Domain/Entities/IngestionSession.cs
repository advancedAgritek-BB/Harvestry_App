using Harvestry.Telemetry.Domain.Enums;

namespace Harvestry.Telemetry.Domain.Entities;

/// <summary>
/// Represents a telemetry ingestion session from a device.
/// Tracks device connection lifecycle and message statistics.
/// </summary>
public class IngestionSession : Entity<Guid>
{
    public Guid SiteId { get; private set; }
    public Guid EquipmentId { get; private set; }
    public IngestionProtocol Protocol { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset LastHeartbeatAt { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }
    public long MessageCount { get; private set; }
    public int ErrorCount { get; private set; }
    public Dictionary<string, object>? Metadata { get; private set; }
    
    // For EF Core
    private IngestionSession() { }
    
    private IngestionSession(
        Guid id,
        Guid siteId,
        Guid equipmentId,
        IngestionProtocol protocol)
    {
        Id = id;
        SiteId = siteId;
        EquipmentId = equipmentId;
        Protocol = protocol;
        StartedAt = DateTimeOffset.UtcNow;
        LastHeartbeatAt = DateTimeOffset.UtcNow;
        MessageCount = 0;
        ErrorCount = 0;
    }
    
    /// <summary>
    /// Starts a new ingestion session.
    /// </summary>
    public static IngestionSession Start(
        Guid siteId,
        Guid equipmentId,
        IngestionProtocol protocol,
        Dictionary<string, object>? metadata = null)
    {
        var session = new IngestionSession(Guid.NewGuid(), siteId, equipmentId, protocol)
        {
            Metadata = metadata
        };
        
        return session;
    }
    
    /// <summary>
    /// Rehydrates ingestion session from persistence layer.
    /// </summary>
    public static IngestionSession FromPersistence(
        Guid id,
        Guid siteId,
        Guid equipmentId,
        IngestionProtocol protocol,
        DateTimeOffset startedAt,
        DateTimeOffset lastHeartbeatAt,
        DateTimeOffset? endedAt,
        long messageCount,
        int errorCount,
        Dictionary<string, object>? metadata)
    {
        return new IngestionSession
        {
            Id = id,
            SiteId = siteId,
            EquipmentId = equipmentId,
            Protocol = protocol,
            StartedAt = startedAt,
            LastHeartbeatAt = lastHeartbeatAt,
            EndedAt = endedAt,
            MessageCount = messageCount,
            ErrorCount = errorCount,
            Metadata = metadata
        };
    }
    
    /// <summary>
    /// Updates heartbeat timestamp (device is still connected).
    /// </summary>
    public void UpdateHeartbeat()
    {
        if (EndedAt.HasValue)
            throw new InvalidOperationException("Session already ended");
            
        LastHeartbeatAt = DateTimeOffset.UtcNow;
    }
    
    /// <summary>
    /// Increments message count (successful ingestion).
    /// </summary>
    public void IncrementMessageCount(int count = 1)
    {
        MessageCount += count;
        LastHeartbeatAt = DateTimeOffset.UtcNow;
    }
    
    /// <summary>
    /// Increments error count (failed ingestion).
    /// </summary>
    public void IncrementErrorCount(int count = 1)
    {
        ErrorCount += count;
        LastHeartbeatAt = DateTimeOffset.UtcNow;
    }
    
    /// <summary>
    /// Ends the session.
    /// </summary>
    public void End()
    {
        if (EndedAt.HasValue)
            throw new InvalidOperationException("Session already ended");
            
        EndedAt = DateTimeOffset.UtcNow;
    }
    
    /// <summary>
    /// Checks if session is currently active.
    /// </summary>
    public bool IsActive() => !EndedAt.HasValue;
    
    /// <summary>
    /// Gets session duration.
    /// </summary>
    public TimeSpan GetDuration()
    {
        var endTime = EndedAt ?? DateTimeOffset.UtcNow;
        return endTime - StartedAt;
    }
    
    /// <summary>
    /// Gets average throughput (messages per second).
    /// </summary>
    public double GetAverageThroughput()
    {
        var duration = GetDuration();
        if (duration.TotalSeconds == 0) return 0;
        return MessageCount / duration.TotalSeconds;
    }
    
    /// <summary>
    /// Checks if session appears stale (no heartbeat in expected timeframe).
    /// </summary>
    public bool IsStale(TimeSpan staleThreshold)
    {
        if (!IsActive()) return false;
        return DateTimeOffset.UtcNow - LastHeartbeatAt > staleThreshold;
    }
}

