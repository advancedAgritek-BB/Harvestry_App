using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Harvestry.Telemetry.Application.DTOs;
using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Domain.Entities;
using Harvestry.Telemetry.Domain.Enums;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Harvestry.Telemetry.Application.Services;

/// <summary>
/// Main orchestration service for telemetry ingestion.
/// Coordinates normalization, deduplication, and bulk persistence.
/// </summary>
public class TelemetryIngestService : ITelemetryIngestService
{
    private readonly ITelemetryConnectionFactory _connectionFactory;
    private readonly INormalizationService _normalizationService;
    private readonly IIdempotencyService _idempotencyService;
    private readonly ISensorReadingRepository _readingRepository;
    private readonly ISensorStreamRepository _sensorStreamRepository;
    private readonly ITelemetryRealtimeDispatcher _realtimeDispatcher;
    private readonly IIngestionSessionRepository _sessionRepository;
    private readonly IIngestionErrorRepository _errorRepository;
    private readonly ITelemetryRlsContextAccessor _rlsContextAccessor;
    private readonly ILogger<TelemetryIngestService> _logger;
    
    private static readonly Dictionary<string, Unit> UnitCodeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["degf"] = Unit.DegreesFahrenheit,
        ["fahrenheit"] = Unit.DegreesFahrenheit,
        ["degc"] = Unit.DegreesCelsius,
        ["celsius"] = Unit.DegreesCelsius,
        ["k"] = Unit.Kelvin,
        ["kelvin"] = Unit.Kelvin,
        ["pct"] = Unit.Percent,
        ["percent"] = Unit.Percent,
        ["ppm"] = Unit.PartsPerMillion,
        ["ppb"] = Unit.PartsPerBillion,
        ["mg_l"] = Unit.MilligramsPerLiter,
        ["mg/l"] = Unit.MilligramsPerLiter,
        ["kpa"] = Unit.Kilopascals,
        ["psi"] = Unit.Psi,
        ["bar"] = Unit.Bar,
        ["umol"] = Unit.Micromoles,
        ["lux"] = Unit.Lux,
        ["footcandles"] = Unit.Footcandles,
        ["us"] = Unit.Microsiemens,
        ["microsiemens"] = Unit.Microsiemens,
        ["ms_cm"] = Unit.MillisiemensPerCm,
        ["ph"] = Unit.Ph,
        ["l"] = Unit.Liters,
        ["liter"] = Unit.Liters,
        ["liters"] = Unit.Liters,
        ["gal"] = Unit.Gallons,
        ["gallon"] = Unit.Gallons,
        ["gallons"] = Unit.Gallons,
        ["gpm"] = Unit.GallonsPerMinute,
        ["lpm"] = Unit.LitersPerMinute,
        ["w"] = Unit.Watts,
        ["kw"] = Unit.Kilowatts,
        ["hp"] = Unit.Horsepower,
        ["kwh"] = Unit.KilowattHours,
        ["wh"] = Unit.WattHours,
        ["joules"] = Unit.Joules
    };
    
    public TelemetryIngestService(
        ITelemetryConnectionFactory connectionFactory,
        INormalizationService normalizationService,
        IIdempotencyService idempotencyService,
        ISensorReadingRepository readingRepository,
        ISensorStreamRepository sensorStreamRepository,
        ITelemetryRealtimeDispatcher realtimeDispatcher,
        IIngestionSessionRepository sessionRepository,
        IIngestionErrorRepository errorRepository,
        ITelemetryRlsContextAccessor rlsContextAccessor,
        ILogger<TelemetryIngestService> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _normalizationService = normalizationService ?? throw new ArgumentNullException(nameof(normalizationService));
        _idempotencyService = idempotencyService ?? throw new ArgumentNullException(nameof(idempotencyService));
        _readingRepository = readingRepository ?? throw new ArgumentNullException(nameof(readingRepository));
        _sensorStreamRepository = sensorStreamRepository ?? throw new ArgumentNullException(nameof(sensorStreamRepository));
        _realtimeDispatcher = realtimeDispatcher ?? throw new ArgumentNullException(nameof(realtimeDispatcher));
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
        _errorRepository = errorRepository ?? throw new ArgumentNullException(nameof(errorRepository));
        _rlsContextAccessor = rlsContextAccessor ?? throw new ArgumentNullException(nameof(rlsContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task<IngestResultDto> IngestBatchAsync(
        Guid siteId,
        IngestTelemetryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.SiteId != siteId)
        {
            throw new ArgumentException("SiteId parameter must match request.SiteId", nameof(siteId));
        }

        var startTime = DateTimeOffset.UtcNow;
        var result = new IngestResultDto
        {
            TotalReceived = request.Readings.Count,
            Accepted = 0,
            Rejected = 0,
            Duplicates = 0,
            ProcessingTimeMs = 0
        };

        var contextAdjusted = EnsureSiteContext(siteId, out var originalContext);

        try
        {
            _logger.LogInformation(
                "Starting ingestion batch: SiteId={SiteId}, EquipmentId={EquipmentId}, Count={Count}",
                request.SiteId, request.EquipmentId, request.Readings.Count);
            
            // Step 1: Deduplication
            var uniqueReadings = await DeduplicateReadingsAsync(
                request.Readings,
                cancellationToken);
            
            result.Duplicates = request.Readings.Count - uniqueReadings.Count;
            
            if (uniqueReadings.Count == 0)
            {
                _logger.LogWarning("All readings in batch were duplicates");
                result.ProcessingTimeMs = (long)(DateTimeOffset.UtcNow - startTime).TotalMilliseconds;
                return result;
            }
            
            // Step 2: Normalize and validate
            var normalizedReadings = await NormalizeReadingsAsync(
                uniqueReadings,
                cancellationToken);
            
            // Step 3: Separate valid from invalid
            var validReadings = normalizedReadings
                .Where(r => r.QualityCode == QualityCode.Good || 
                           ((short)r.QualityCode & 0x00C0) < 0x00C0) // Not "Bad"
                .ToList();
            
            var invalidReadings = normalizedReadings
                .Where(r => ((short)r.QualityCode & 0x00C0) == 0x00C0) // Bad quality
                .ToList();
            
            // Step 4: Bulk insert valid readings
            if (validReadings.Any())
            {
                await _readingRepository.BulkInsertAsync(validReadings, cancellationToken).ConfigureAwait(false);
                await _realtimeDispatcher.PublishAsync(validReadings, cancellationToken).ConfigureAwait(false);

                result.Accepted = validReadings.Count;
            }
            
            // Step 5: Log errors for invalid readings
            if (invalidReadings.Any())
            {
                await LogIngestionErrorsAsync(
                    invalidReadings,
                    request,
                    cancellationToken);
                
                result.Rejected = invalidReadings.Count;
            }
            
            result.ProcessingTimeMs = (long)(DateTimeOffset.UtcNow - startTime).TotalMilliseconds;
            
            _logger.LogInformation(
                "Completed ingestion batch: Accepted={Accepted}, Rejected={Rejected}, " +
                "Duplicates={Duplicates}, ProcessingTime={ProcessingTimeMs}ms",
                result.Accepted, result.Rejected, result.Duplicates, result.ProcessingTimeMs);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Critical error during telemetry ingestion: SiteId={SiteId}, EquipmentId={EquipmentId}",
                request.SiteId, request.EquipmentId);
            
            result.ProcessingTimeMs = (long)(DateTimeOffset.UtcNow - startTime).TotalMilliseconds;
            throw;
        }
        finally
        {
            RestoreSiteContext(originalContext, contextAdjusted);
        }
    }
    
    public async Task<IngestResultDto> IngestMqttMessageAsync(
        string topic,
        byte[] payload,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(topic))
        {
            throw new ArgumentException("MQTT topic cannot be null or whitespace", nameof(topic));
        }

        if (payload == null || payload.Length == 0)
        {
            throw new ArgumentException("MQTT payload cannot be empty", nameof(payload));
        }

        if (!TryParseMqttTopic(topic, out var siteId, out var equipmentId, out var streamIdFromTopic, out var topicType))
        {
            throw new ArgumentException($"Unsupported MQTT topic format: {topic}", nameof(topic));
        }

        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;
        var messageTimestamp = ResolveTimestamp(root, "timestamp", DateTimeOffset.UtcNow);
        var messageMetadata = ExtractMetadata(root, "metadata");

        var readings = new List<SensorReadingDto>();

        if (topicType == MqttTopicType.Telemetry)
        {
            if (!root.TryGetProperty("readings", out var readingsElement) || readingsElement.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidOperationException("MQTT telemetry payload must include a readings array.");
            }

            foreach (var readingElement in readingsElement.EnumerateArray())
            {
                var streamId = streamIdFromTopic ?? (readingElement.TryGetProperty("stream_id", out var streamElement) &&
                    streamElement.ValueKind == JsonValueKind.String &&
                    Guid.TryParse(streamElement.GetString(), out var parsedStream)
                        ? parsedStream
                        : Guid.Empty);

                if (streamId == Guid.Empty)
                {
                    _logger.LogWarning("Skipping MQTT reading without stream_id (topic: {Topic})", topic);
                    continue;
                }

                if (!TryReadTelemetryValue(readingElement, out var value))
                {
                    _logger.LogWarning("Skipping MQTT reading with invalid value (stream: {StreamId})", streamId);
                    continue;
                }

                var unitCode = readingElement.TryGetProperty("unit", out var unitElement) ? unitElement.GetString() : null;
                if (!TryParseUnitCode(unitCode, out var unit))
                {
                    _logger.LogWarning("Skipping MQTT reading with unsupported unit '{Unit}' (stream: {StreamId})", unitCode, streamId);
                    continue;
                }

                var readingTimestamp = ResolveTimestamp(readingElement, "timestamp", messageTimestamp);
                var messageId = readingElement.TryGetProperty("message_id", out var messageIdElement) ? messageIdElement.GetString() : null;
                var readingMetadata = ExtractMetadata(readingElement, "metadata") ?? messageMetadata;

                readings.Add(new SensorReadingDto(
                    streamId,
                    readingTimestamp,
                    value,
                    unit,
                    readingTimestamp,
                    messageId,
                    readingMetadata));
            }
        }
        else
        {
            if (streamIdFromTopic is null)
            {
                throw new InvalidOperationException("Sensor-specific MQTT topics must include a stream identifier.");
            }

            if (!TryReadTelemetryValue(root, out var value))
            {
                throw new InvalidOperationException("MQTT payload missing numeric value.");
            }

            var unitCode = root.TryGetProperty("unit", out var unitElement) ? unitElement.GetString() : null;
            if (!TryParseUnitCode(unitCode, out var unit))
            {
                throw new InvalidOperationException($"Unsupported unit '{unitCode}' in MQTT payload.");
            }

            var readingTimestamp = ResolveTimestamp(root, "timestamp", messageTimestamp);
            var messageId = root.TryGetProperty("message_id", out var messageIdElement) ? messageIdElement.GetString() : null;
            var readingMetadata = ExtractMetadata(root, "metadata") ?? messageMetadata;

            readings.Add(new SensorReadingDto(
                streamIdFromTopic.Value,
                readingTimestamp,
                value,
                unit,
                readingTimestamp,
                messageId,
                readingMetadata));
        }

        if (readings.Count == 0)
        {
            _logger.LogWarning("No valid readings parsed from MQTT payload (topic: {Topic})", topic);
            return new IngestResultDto();
        }

        var request = new IngestTelemetryRequestDto(
            siteId,
            equipmentId,
            IngestionProtocol.Mqtt,
            readings);

        return await IngestBatchAsync(siteId, request, cancellationToken).ConfigureAwait(false);
    }
    
    public async Task<IngestResultDto> IngestHttpMessageAsync(
        Guid equipmentId,
        SensorReadingDto[] readings,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(readings);

        var siteId = _rlsContextAccessor.Current.SiteId ?? throw new InvalidOperationException(
            "RLS context must include a site identifier before ingesting HTTP telemetry.");

        if (readings.Length == 0)
        {
            throw new ArgumentException("At least one reading is required for HTTP ingestion", nameof(readings));
        }

        var request = new IngestTelemetryRequestDto(
            siteId,
            equipmentId,
            IngestionProtocol.Http,
            readings.ToList());

        return await IngestBatchAsync(siteId, request, cancellationToken).ConfigureAwait(false);
    }
    
    public async Task<IngestionSession> StartSessionAsync(
        Guid siteId,
        Guid equipmentId,
        IngestionProtocol protocol,
        CancellationToken cancellationToken = default)
    {
        var contextAdjusted = EnsureSiteContext(siteId, out var originalContext);

        try
        {
            var session = IngestionSession.Start(siteId, equipmentId, protocol);
            await _sessionRepository.CreateAsync(session, cancellationToken).ConfigureAwait(false);
            return session;
        }
        finally
        {
            RestoreSiteContext(originalContext, contextAdjusted);
        }
    }

    public async Task UpdateSessionHeartbeatAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        await _sessionRepository.UpdateHeartbeatAsync(sessionId, DateTimeOffset.UtcNow, cancellationToken).ConfigureAwait(false);
    }

    public async Task EndSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        await _sessionRepository.EndAsync(sessionId, DateTimeOffset.UtcNow, cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<IngestionErrorDto>> GetRecentErrorsAsync(
        Guid siteId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var contextAdjusted = EnsureSiteContext(siteId, out var originalContext);

        try
        {
            const string query = @"
                SELECT id, site_id, session_id, equipment_id,
                       protocol, error_type, error_message, occurred_at
                FROM ingestion_errors
                WHERE site_id = @SiteId
                ORDER BY occurred_at DESC
                LIMIT @Limit";

            var take = Math.Clamp(limit, 1, 200);
            var results = new List<IngestionErrorDto>(take);

            await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@SiteId", siteId);
            command.Parameters.AddWithValue("@Limit", take);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var protocolValue = reader.GetString(4);
                var errorTypeValue = reader.GetString(5);

                var protocolEnum = Enum.TryParse<IngestionProtocol>(protocolValue, true, out var parsedProtocol)
                    ? parsedProtocol
                    : IngestionProtocol.Mqtt;

                var errorEnum = Enum.TryParse<IngestionErrorType>(errorTypeValue, true, out var parsedError)
                    ? parsedError
                    : IngestionErrorType.ProcessingError;

                results.Add(new IngestionErrorDto(
                    reader.GetGuid(0),
                    reader.GetGuid(1),
                    reader.IsDBNull(2) ? null : reader.GetGuid(2),
                    reader.IsDBNull(3) ? null : reader.GetGuid(3),
                    protocolEnum,
                    errorEnum,
                    reader.GetString(6),
                    reader.GetFieldValue<DateTimeOffset>(7)));
            }

            return results;
        }
        finally
        {
            RestoreSiteContext(originalContext, contextAdjusted);
        }
    }
    
    private async Task<List<SensorReadingDto>> DeduplicateReadingsAsync(
        List<SensorReadingDto> readings,
        CancellationToken cancellationToken)
    {
        if (readings.Count == 0)
        {
            return readings;
        }

        var messageIdsByStream = readings
            .Where(r => !string.IsNullOrWhiteSpace(r.MessageId))
            .GroupBy(r => r.StreamId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(r => r.MessageId!.Trim())
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct(StringComparer.Ordinal)
                    .ToArray());

        HashSet<(Guid StreamId, string MessageId)> existingMessageIds = new();

        if (messageIdsByStream.Count > 0)
        {
            existingMessageIds = await _idempotencyService
                .GetDuplicatesByStreamAsync(messageIdsByStream, cancellationToken)
                .ConfigureAwait(false);
        }

        var uniqueReadings = new List<SensorReadingDto>(readings.Count);

        foreach (var reading in readings)
        {
            if (string.IsNullOrWhiteSpace(reading.MessageId))
            {
                uniqueReadings.Add(reading);
                continue;
            }

            var key = (reading.StreamId, reading.MessageId!);

            if (!existingMessageIds.Contains(key))
            {
                uniqueReadings.Add(reading);
                existingMessageIds.Add(key);
            }
            else
            {
                _logger.LogDebug(
                    "Duplicate reading detected: StreamId={StreamId}, MessageId={MessageId}",
                    reading.StreamId, reading.MessageId);
            }
        }

        return uniqueReadings;
    }
    
    private async Task<List<SensorReading>> NormalizeReadingsAsync(
        List<SensorReadingDto> readings,
        CancellationToken cancellationToken)
    {
        var normalizedReadings = new List<SensorReading>();
        
        // Fetch sensor stream configurations
        var streamIds = readings.Select(r => r.StreamId).Distinct().ToList();
        var streams = await FetchSensorStreamsAsync(streamIds, cancellationToken);
        
        foreach (var reading in readings)
        {
            if (!streams.TryGetValue(reading.StreamId, out var stream))
            {
                _logger.LogWarning(
                    "Sensor stream not found: StreamId={StreamId}",
                    reading.StreamId);
                
                // Create a "bad" reading to log the error
                normalizedReadings.Add(CreateBadReading(
                    reading,
                    QualityCode.BadConfigurationError));
                continue;
            }
            
            // Normalize using the service
            var normalizedReading = await _normalizationService.NormalizeAsync(
                reading,
                stream,
                cancellationToken);
            
            normalizedReadings.Add(normalizedReading);
        }
        
        return normalizedReadings;
    }
    
    private async Task<Dictionary<Guid, SensorStream>> FetchSensorStreamsAsync(
        List<Guid> streamIds,
        CancellationToken cancellationToken)
    {
        if (streamIds.Count == 0)
        {
            return new Dictionary<Guid, SensorStream>();
        }

        var streams = await _sensorStreamRepository
            .GetByIdsAsync(streamIds, cancellationToken)
            .ConfigureAwait(false);

        return streams
            .GroupBy(stream => stream.Id)
            .ToDictionary(group => group.Key, group => group.First());
    }
    
    private async Task LogIngestionErrorsAsync(
        List<SensorReading> invalidReadings,
        IngestTelemetryRequestDto request,
        CancellationToken cancellationToken)
    {
        if (invalidReadings.Count == 0)
        {
            return;
        }

        var errors = invalidReadings.Select(reading =>
        {
            var payload = new Dictionary<string, object>
            {
                ["streamId"] = reading.StreamId,
                ["value"] = reading.Value,
                ["qualityCode"] = reading.QualityCode.ToString()
            };

            return IngestionError.Create(
                request.SiteId,
                request.Protocol,
                DetermineErrorType(reading.QualityCode),
                $"Invalid reading with quality code: {reading.QualityCode}",
                equipmentId: request.EquipmentId,
                rawPayload: payload);
        }).ToArray();

        await _errorRepository.LogAsync(errors, cancellationToken).ConfigureAwait(false);
    }
    
    private SensorReading CreateBadReading(SensorReadingDto dto, QualityCode qualityCode)
    {
        return SensorReading.FromIngestion(
            streamId: dto.StreamId,
            value: dto.Value,
            qualityCode: qualityCode,
            sourceTimestamp: dto.SourceTimestamp,
            messageId: dto.MessageId,
            metadata: null);
    }

    private static bool TryParseUnitCode(string? unitCode, out Unit unit)
    {
        if (!string.IsNullOrWhiteSpace(unitCode))
        {
            var trimmed = unitCode.Trim();
            if (UnitCodeMap.TryGetValue(trimmed, out unit))
            {
                return true;
            }

            if (Enum.TryParse(trimmed, ignoreCase: true, out unit))
            {
                return true;
            }
        }

        unit = Unit.Count;
        return false;
    }

    private static DateTimeOffset ResolveTimestamp(JsonElement root, string propertyName, DateTimeOffset fallback)
    {
        if (root.TryGetProperty(propertyName, out var element) &&
            element.ValueKind == JsonValueKind.String &&
            DateTimeOffset.TryParse(element.GetString(), out var parsed))
        {
            return parsed;
        }

        return fallback;
    }

    private static string? ExtractMetadata(JsonElement root, string propertyName)
    {
        if (root.TryGetProperty(propertyName, out var element) && element.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
        {
            return element.GetRawText();
        }

        return null;
    }

    private bool EnsureSiteContext(Guid siteId, out TelemetryRlsContext previousContext)
    {
        previousContext = _rlsContextAccessor.Current;
        if (previousContext.SiteId == siteId)
        {
            return false;
        }

        _rlsContextAccessor.Set(previousContext with { SiteId = siteId });
        return true;
    }

    private void RestoreSiteContext(TelemetryRlsContext previousContext, bool contextAdjusted)
    {
        if (contextAdjusted)
        {
            _rlsContextAccessor.Set(previousContext);
        }
    }

    private static bool TryParseMqttTopic(
        string topic,
        out Guid siteId,
        out Guid equipmentId,
        out Guid? streamId,
        out MqttTopicType topicType)
    {
        siteId = Guid.Empty;
        equipmentId = Guid.Empty;
        streamId = null;
        topicType = MqttTopicType.Telemetry;

        if (string.IsNullOrWhiteSpace(topic))
        {
            return false;
        }

        var segments = topic.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 5)
        {
            return false;
        }

        if (!segments[0].Equals("site", StringComparison.OrdinalIgnoreCase) ||
            !segments[2].Equals("equipment", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!Guid.TryParse(segments[1], out siteId) || !Guid.TryParse(segments[3], out equipmentId))
        {
            return false;
        }

        var next = segments[4];
        if (next.Equals("telemetry", StringComparison.OrdinalIgnoreCase))
        {
            topicType = MqttTopicType.Telemetry;
            return true;
        }

        if (next.Equals("sensor", StringComparison.OrdinalIgnoreCase) && segments.Length >= 6 && Guid.TryParse(segments[5], out var parsedStream))
        {
            topicType = MqttTopicType.Sensor;
            streamId = parsedStream;
            return true;
        }

        return false;
    }

    private static bool TryReadTelemetryValue(JsonElement element, out double value)
    {
        if (element.TryGetProperty("value", out var valueElement) && valueElement.TryGetDouble(out value))
        {
            return true;
        }

        value = default;
        return false;
    }

    private enum MqttTopicType
    {
        Telemetry,
        Sensor
    }

    private IngestionErrorType DetermineErrorType(QualityCode qualityCode)
    {
        return qualityCode switch
        {
            QualityCode.BadConfigurationError => IngestionErrorType.ValidationFailure,
            QualityCode.Bad => IngestionErrorType.ProcessingError,
            _ => IngestionErrorType.ProcessingError
        };
    }
}
