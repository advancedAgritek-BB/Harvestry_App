using System.Diagnostics;
using Harvestry.Integration.Growlink.Application.DTOs;
using Harvestry.Integration.Growlink.Application.Interfaces;
using Harvestry.Integration.Growlink.Domain.Enums;
using Harvestry.Integration.Growlink.Infrastructure.External;
using Harvestry.Telemetry.Application.DTOs;
using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Harvestry.Integration.Growlink.Application.Services;

/// <summary>
/// Orchestrates syncing data from Growlink to Harvestry telemetry.
/// </summary>
public sealed class GrowlinkSyncService : IGrowlinkSyncService
{
    private readonly IGrowlinkApiClient _apiClient;
    private readonly IGrowlinkCredentialRepository _credentialRepository;
    private readonly IGrowlinkStreamMapper _streamMapper;
    private readonly ITelemetryIngestService _ingestService;
    private readonly GrowlinkApiConfiguration _config;
    private readonly ILogger<GrowlinkSyncService> _logger;

    public GrowlinkSyncService(
        IGrowlinkApiClient apiClient,
        IGrowlinkCredentialRepository credentialRepository,
        IGrowlinkStreamMapper streamMapper,
        ITelemetryIngestService ingestService,
        IOptions<GrowlinkApiConfiguration> config,
        ILogger<GrowlinkSyncService> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _credentialRepository = credentialRepository ?? throw new ArgumentNullException(nameof(credentialRepository));
        _streamMapper = streamMapper ?? throw new ArgumentNullException(nameof(streamMapper));
        _ingestService = ingestService ?? throw new ArgumentNullException(nameof(ingestService));
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GrowlinkSyncResultDto> SyncLatestReadingsAsync(
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Starting Growlink sync for site {SiteId}", siteId);

        // Get credentials for site
        var credential = await _credentialRepository.GetBySiteIdAsync(siteId, cancellationToken);

        if (credential == null || !credential.IsUsable())
        {
            _logger.LogWarning("No usable Growlink credentials for site {SiteId}", siteId);
            return CreateSkippedResult(siteId, stopwatch.ElapsedMilliseconds, "No valid credentials");
        }

        // Check if token needs refresh
        if (credential.NeedsTokenRefresh(TimeSpan.FromMinutes(_config.TokenRefreshBufferMinutes)))
        {
            var refreshed = await RefreshTokenAsync(credential, cancellationToken);
            if (!refreshed)
            {
                return CreateErrorResult(siteId, stopwatch.ElapsedMilliseconds,
                    GrowlinkSyncStatus.AuthenticationError, "Token refresh failed");
            }
        }

        // Fetch latest readings from Growlink
        var readingsResponse = await _apiClient.GetLatestReadingsAsync(
            credential.AccessToken,
            cancellationToken);

        if (!readingsResponse.IsSuccess)
        {
            await HandleSyncFailureAsync(credential, readingsResponse.ErrorMessage ?? "API error", cancellationToken);

            var status = readingsResponse.IsRateLimited
                ? GrowlinkSyncStatus.RateLimited
                : GrowlinkSyncStatus.ApiError;

            return CreateErrorResult(siteId, stopwatch.ElapsedMilliseconds, status, readingsResponse.ErrorMessage);
        }

        var batch = readingsResponse.Data;
        if (batch == null || batch.Readings.Count == 0)
        {
            _logger.LogDebug("No readings returned from Growlink for site {SiteId}", siteId);
            await RecordSuccessAsync(credential, cancellationToken);
            return new GrowlinkSyncResultDto(
                siteId,
                GrowlinkSyncStatus.Success,
                ReadingsReceived: 0,
                ReadingsIngested: 0,
                ReadingsRejected: 0,
                ReadingsDuplicate: 0,
                stopwatch.ElapsedMilliseconds);
        }

        // Map and ingest readings
        var result = await MapAndIngestReadingsAsync(siteId, credential, batch, cancellationToken);

        stopwatch.Stop();

        await RecordSuccessAsync(credential, cancellationToken);

        _logger.LogInformation(
            "Growlink sync completed for site {SiteId}: Received={Received}, Ingested={Ingested}, Rejected={Rejected}, Duplicates={Duplicates}, Time={TimeMs}ms",
            siteId, result.ReadingsReceived, result.ReadingsIngested, result.ReadingsRejected, result.ReadingsDuplicate, stopwatch.ElapsedMilliseconds);

        return result with { ProcessingTimeMs = stopwatch.ElapsedMilliseconds };
    }

    public async Task<List<GrowlinkSyncResultDto>> SyncAllSitesAsync(
        CancellationToken cancellationToken = default)
    {
        var credentials = await _credentialRepository.GetActiveCredentialsAsync(cancellationToken);

        _logger.LogInformation("Starting Growlink sync for {Count} sites", credentials.Count);

        var results = new List<GrowlinkSyncResultDto>();

        foreach (var credential in credentials)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                var result = await SyncLatestReadingsAsync(credential.SiteId, cancellationToken);
                results.Add(result);

                // Brief delay between sites to avoid rate limiting
                await Task.Delay(100, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing site {SiteId}", credential.SiteId);
                results.Add(CreateErrorResult(credential.SiteId, 0, GrowlinkSyncStatus.ApiError, ex.Message));
            }
        }

        return results;
    }

    private async Task<GrowlinkSyncResultDto> MapAndIngestReadingsAsync(
        Guid siteId,
        Domain.Entities.GrowlinkCredential credential,
        GrowlinkReadingsBatchDto batch,
        CancellationToken cancellationToken)
    {
        var readings = new List<SensorReadingDto>();
        var unmappedCount = 0;

        // Get existing mappings for fast lookup
        var mappings = await _streamMapper.GetMappingsAsync(siteId, cancellationToken);

        foreach (var growlinkReading in batch.Readings)
        {
            var sensorKey = $"{growlinkReading.DeviceId}:{growlinkReading.SensorId}";

            Guid? streamId;
            if (mappings.TryGetValue(sensorKey, out var existingStreamId))
            {
                streamId = existingStreamId;
            }
            else
            {
                // Try to auto-create mapping
                streamId = await _streamMapper.GetHarvestryStreamIdAsync(
                    siteId,
                    growlinkReading.DeviceId,
                    growlinkReading.SensorId,
                    growlinkReading.SensorId, // Use sensor ID as name if not available
                    InferSensorType(growlinkReading.Unit),
                    autoCreate: true,
                    cancellationToken);

                if (streamId.HasValue)
                {
                    mappings[sensorKey] = streamId.Value;
                }
            }

            if (!streamId.HasValue)
            {
                unmappedCount++;
                continue;
            }

            var unit = MapUnit(growlinkReading.Unit);
            var messageId = $"growlink:{growlinkReading.DeviceId}:{growlinkReading.SensorId}:{growlinkReading.Timestamp.ToUnixTimeMilliseconds()}";

            readings.Add(new SensorReadingDto(
                streamId.Value,
                growlinkReading.Timestamp,
                growlinkReading.Value,
                unit,
                growlinkReading.Timestamp,
                messageId));
        }

        if (readings.Count == 0)
        {
            _logger.LogWarning(
                "All {Count} Growlink readings unmapped for site {SiteId}",
                batch.Readings.Count, siteId);

            return new GrowlinkSyncResultDto(
                siteId,
                GrowlinkSyncStatus.PartialSuccess,
                batch.Readings.Count,
                ReadingsIngested: 0,
                ReadingsRejected: unmappedCount,
                ReadingsDuplicate: 0,
                ProcessingTimeMs: 0);
        }

        // Derive equipment ID for ingestion
        var equipmentId = DeriveEquipmentId(siteId, "growlink-sync");

        var ingestRequest = new IngestTelemetryRequestDto(
            siteId,
            equipmentId,
            IngestionProtocol.Http, // Use HTTP as protocol identifier for external APIs
            readings);

        var ingestResult = await _ingestService.IngestBatchAsync(siteId, ingestRequest, cancellationToken);

        return new GrowlinkSyncResultDto(
            siteId,
            unmappedCount > 0 ? GrowlinkSyncStatus.PartialSuccess : GrowlinkSyncStatus.Success,
            batch.Readings.Count,
            ingestResult.Accepted,
            ingestResult.Rejected + unmappedCount,
            ingestResult.Duplicates,
            ingestResult.ProcessingTimeMs);
    }

    private async Task<bool> RefreshTokenAsync(
        Domain.Entities.GrowlinkCredential credential,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Refreshing Growlink token for site {SiteId}", credential.SiteId);

        var response = await _apiClient.RefreshTokenAsync(credential.RefreshToken, cancellationToken);

        if (!response.IsSuccess || response.Data == null)
        {
            _logger.LogError(
                "Failed to refresh Growlink token for site {SiteId}: {Error}",
                credential.SiteId, response.ErrorMessage);

            credential.MarkTokenExpired();
            await _credentialRepository.UpdateAsync(credential, cancellationToken);
            return false;
        }

        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(response.Data.ExpiresIn);
        credential.UpdateTokens(response.Data.AccessToken, response.Data.RefreshToken, expiresAt);
        await _credentialRepository.UpdateAsync(credential, cancellationToken);

        _logger.LogInformation("Refreshed Growlink token for site {SiteId}", credential.SiteId);
        return true;
    }

    private async Task HandleSyncFailureAsync(
        Domain.Entities.GrowlinkCredential credential,
        string error,
        CancellationToken cancellationToken)
    {
        credential.RecordSyncFailure(error);
        await _credentialRepository.UpdateAsync(credential, cancellationToken);
    }

    private async Task RecordSuccessAsync(
        Domain.Entities.GrowlinkCredential credential,
        CancellationToken cancellationToken)
    {
        credential.RecordSuccessfulSync();
        await _credentialRepository.UpdateAsync(credential, cancellationToken);
    }

    private static GrowlinkSyncResultDto CreateSkippedResult(Guid siteId, long processingTimeMs, string? error = null)
    {
        return new GrowlinkSyncResultDto(
            siteId,
            GrowlinkSyncStatus.Skipped,
            ReadingsReceived: 0,
            ReadingsIngested: 0,
            ReadingsRejected: 0,
            ReadingsDuplicate: 0,
            processingTimeMs,
            error);
    }

    private static GrowlinkSyncResultDto CreateErrorResult(
        Guid siteId,
        long processingTimeMs,
        GrowlinkSyncStatus status,
        string? error)
    {
        return new GrowlinkSyncResultDto(
            siteId,
            status,
            ReadingsReceived: 0,
            ReadingsIngested: 0,
            ReadingsRejected: 0,
            ReadingsDuplicate: 0,
            processingTimeMs,
            error);
    }

    private static string InferSensorType(string unit)
    {
        var lowerUnit = unit.ToLowerInvariant();
        return lowerUnit switch
        {
            "°f" or "°c" or "f" or "c" => "temperature",
            "%" or "percent" => "humidity",
            "ppm" => "co2",
            "kpa" => "vpd",
            "ms/cm" or "ec" => "ec",
            "ph" => "ph",
            "umol/m²/s" or "µmol" => "ppfd",
            "gpm" or "lpm" => "flow_rate",
            "w" or "kw" => "power",
            "kwh" => "energy",
            _ => "custom"
        };
    }

    private static Unit MapUnit(string growlinkUnit)
    {
        var lower = growlinkUnit.ToLowerInvariant();
        return lower switch
        {
            "°f" or "f" => Unit.DegreesFahrenheit,
            "°c" or "c" => Unit.DegreesCelsius,
            "%" or "percent" => Unit.Percent,
            "ppm" => Unit.PartsPerMillion,
            "kpa" => Unit.Kilopascals,
            "ms/cm" or "ec" => Unit.MillisiemensPerCm,
            "ph" => Unit.Ph,
            "umol/m²/s" or "µmol" => Unit.Micromoles,
            "lux" => Unit.Lux,
            "gpm" => Unit.GallonsPerMinute,
            "lpm" => Unit.LitersPerMinute,
            "w" => Unit.Watts,
            "kw" => Unit.Kilowatts,
            "kwh" => Unit.KilowattHours,
            _ => Unit.Count
        };
    }

    private static Guid DeriveEquipmentId(Guid siteId, string identifier)
    {
        var input = $"{siteId}:growlink:{identifier}";
        var hash = System.Security.Cryptography.MD5.HashData(
            System.Text.Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}





