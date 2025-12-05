using Harvestry.Telemetry.Application.DTOs;
using Harvestry.Telemetry.Domain.Entities;
using Harvestry.Telemetry.Domain.Enums;

namespace Harvestry.Telemetry.Application.Interfaces;

/// <summary>
/// Service for ingesting telemetry data from multiple protocols.
/// Handles normalization, deduplication, and bulk insertion.
/// </summary>
public interface ITelemetryIngestService
{
    /// <summary>
    /// Ingests a batch of telemetry readings.
    /// </summary>
    Task<IngestResultDto> IngestBatchAsync(
        Guid siteId,
        IngestTelemetryRequestDto request,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Ingests MQTT message payload.
    /// </summary>
    Task<IngestResultDto> IngestMqttMessageAsync(
        string topic,
        byte[] payload,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Ingests HTTP POST message.
    /// </summary>
    Task<IngestResultDto> IngestHttpMessageAsync(
        Guid equipmentId,
        SensorReadingDto[] readings,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Starts a new ingestion session for device tracking.
    /// </summary>
    Task<IngestionSession> StartSessionAsync(
        Guid siteId,
        Guid equipmentId,
        IngestionProtocol protocol,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates session heartbeat (device still connected).
    /// </summary>
    Task UpdateSessionHeartbeatAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Ends an ingestion session.
    /// </summary>
    Task EndSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets recent ingestion errors for monitoring.
    /// </summary>
    Task<List<IngestionErrorDto>> GetRecentErrorsAsync(
        Guid siteId,
        int limit,
        CancellationToken cancellationToken = default);
}

