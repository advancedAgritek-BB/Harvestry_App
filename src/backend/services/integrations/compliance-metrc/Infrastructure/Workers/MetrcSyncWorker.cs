using System.Diagnostics;
using Harvestry.Compliance.Metrc.Application.Interfaces;
using Harvestry.Compliance.Metrc.Domain.Enums;
using Harvestry.Compliance.Metrc.Domain.JurisdictionRules;
using Harvestry.Compliance.Metrc.Infrastructure.External;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Harvestry.Compliance.Metrc.Infrastructure.Workers;

/// <summary>
/// Background worker that processes the METRC sync queue
/// </summary>
public sealed class MetrcSyncWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MetrcSyncWorker> _logger;
    private readonly MetrcSyncWorkerOptions _options;

    public MetrcSyncWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<MetrcSyncWorker> logger,
        IOptions<MetrcSyncWorkerOptions> options)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? new MetrcSyncWorkerOptions();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("METRC Sync Worker starting (interval: {Interval}s)",
            _options.ProcessingIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessQueueAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Normal shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in METRC Sync Worker processing loop");
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.ProcessingIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("METRC Sync Worker stopped");
    }

    private async Task ProcessQueueAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var licenseService = scope.ServiceProvider.GetRequiredService<IMetrcLicenseService>();
        var queueService = scope.ServiceProvider.GetRequiredService<IMetrcQueueService>();
        var syncJobRepo = scope.ServiceProvider.GetRequiredService<IMetrcSyncJobRepository>();
        var licenseRepo = scope.ServiceProvider.GetRequiredService<IMetrcLicenseRepository>();
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<MetrcHttpClientFactory>();
        var jurisdictionFactory = scope.ServiceProvider.GetRequiredService<JurisdictionRulesFactory>();

        // Get licenses due for auto-sync
        var licensesDue = await licenseService.GetLicensesDueForSyncAsync(stoppingToken);

        foreach (var license in licensesDue)
        {
            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                await ProcessLicenseQueueAsync(
                    license.LicenseNumber,
                    license.StateCode,
                    license.UseSandbox,
                    license.VendorApiKeyEncrypted!,
                    license.UserApiKeyEncrypted!,
                    queueService,
                    httpClientFactory,
                    jurisdictionFactory,
                    stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error processing queue for license {LicenseNumber}",
                    license.LicenseNumber);
            }
        }
    }

    private async Task ProcessLicenseQueueAsync(
        string licenseNumber,
        string stateCode,
        bool useSandbox,
        string vendorApiKeyEncrypted,
        string userApiKeyEncrypted,
        IMetrcQueueService queueService,
        MetrcHttpClientFactory httpClientFactory,
        JurisdictionRulesFactory jurisdictionFactory,
        CancellationToken stoppingToken)
    {
        var jurisdictionRules = jurisdictionFactory.GetRules(stateCode);
        var rateLimitPerMinute = jurisdictionRules.ApiRateLimitPerMinute;
        var delayBetweenCalls = TimeSpan.FromMilliseconds(60000.0 / rateLimitPerMinute);

        // Create HTTP client for this license
        var client = httpClientFactory.CreateClient(
            stateCode,
            DecryptCredential(vendorApiKeyEncrypted),
            DecryptCredential(userApiKeyEncrypted),
            useSandbox);

        // Get next batch of items
        var items = await queueService.GetNextBatchAsync(licenseNumber, _options.BatchSize, stoppingToken);

        if (items.Count == 0)
        {
            return;
        }

        _logger.LogDebug(
            "Processing {Count} queue items for license {LicenseNumber}",
            items.Count, licenseNumber);

        var processedCount = 0;
        var successCount = 0;
        var failCount = 0;

        foreach (var item in items)
        {
            if (stoppingToken.IsCancellationRequested) break;

            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Mark as processing
                item.MarkProcessing();

                // Execute the operation based on entity type and operation type
                var (success, metrcId, metrcLabel, response, errorMessage, errorCode) =
                    await ExecuteOperationAsync(item, client, licenseNumber, stoppingToken);

                if (success)
                {
                    await queueService.CompleteItemAsync(
                        item.Id, metrcId, metrcLabel, response, stoppingToken);
                    successCount++;
                }
                else
                {
                    await queueService.FailItemAsync(
                        item.Id, errorMessage ?? "Unknown error", errorCode, response, stoppingToken);
                    failCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error processing queue item {ItemId} for {EntityType} {EntityId}",
                    item.Id, item.EntityType, item.HarvestryEntityId);

                await queueService.FailItemAsync(
                    item.Id, ex.Message, "EXCEPTION", null, stoppingToken);
                failCount++;
            }

            processedCount++;
            stopwatch.Stop();

            _logger.LogDebug(
                "Processed item {ItemId} in {Duration}ms (success: {Success})",
                item.Id, stopwatch.ElapsedMilliseconds, successCount > failCount);

            // Rate limiting delay
            await Task.Delay(delayBetweenCalls, stoppingToken);
        }

        _logger.LogInformation(
            "Processed {Processed} items for license {LicenseNumber}: {Success} success, {Failed} failed",
            processedCount, licenseNumber, successCount, failCount);
    }

    private async Task<(bool Success, long? MetrcId, string? MetrcLabel, string? Response, string? ErrorMessage, string? ErrorCode)>
        ExecuteOperationAsync(
            Domain.Entities.MetrcQueueItem item,
            MetrcHttpClient client,
            string licenseNumber,
            CancellationToken stoppingToken)
    {
        // Route to appropriate METRC API based on entity type and operation
        var endpoint = GetEndpoint(item);

        if (string.IsNullOrEmpty(endpoint))
        {
            return (false, null, null, null, $"Unsupported operation: {item.EntityType}/{item.OperationType}", "UNSUPPORTED");
        }

        // For read operations, use GET
        if (item.OperationType == MetrcOperationType.Read)
        {
            var getResponse = await client.GetAsync<object>(endpoint, licenseNumber, stoppingToken);
            if (getResponse.IsSuccess)
            {
                return (true, item.MetrcId, item.MetrcLabel, null, null, null);
            }
            return (false, null, null, null, getResponse.ErrorMessage, getResponse.StatusCode.ToString());
        }

        // For write operations, use POST/PUT/DELETE
        var payload = System.Text.Json.JsonSerializer.Deserialize<object>(item.PayloadJson);
        Harvestry.Compliance.Metrc.Infrastructure.External.MetrcApiResponse<object> response;

        response = item.OperationType switch
        {
            MetrcOperationType.Create or MetrcOperationType.Adjust or MetrcOperationType.Move or MetrcOperationType.ChangePhase
                or MetrcOperationType.Harvest or MetrcOperationType.Package or MetrcOperationType.Finish
                or MetrcOperationType.RecordWaste or MetrcOperationType.Remediate
                => await client.PostAsync<object, object>(endpoint, licenseNumber, payload!, stoppingToken),
            MetrcOperationType.Update
                => await client.PutAsync<object, object>(endpoint, licenseNumber, payload!, stoppingToken),
            MetrcOperationType.Delete
                => await client.DeleteAsync(endpoint, licenseNumber, stoppingToken),
            _ => await client.PostAsync<object, object>(endpoint, licenseNumber, payload!, stoppingToken)
        };

        if (response.IsSuccess)
        {
            // Extract METRC ID from response if available
            // This would need to be customized based on METRC response format
            return (true, item.MetrcId, item.MetrcLabel, null, null, null);
        }

        return (false, null, null, null, response.ErrorMessage, response.StatusCode.ToString());
    }

    private static string? GetEndpoint(Domain.Entities.MetrcQueueItem item)
    {
        return (item.EntityType, item.OperationType) switch
        {
            (MetrcEntityType.PlantBatch, MetrcOperationType.Create) => "plantbatches/v1/createplantings",
            (MetrcEntityType.PlantBatch, MetrcOperationType.ChangePhase) => "plantbatches/v1/changegrowthphase",
            (MetrcEntityType.Plant, MetrcOperationType.Move) => "plants/v1/moveplants",
            (MetrcEntityType.Plant, MetrcOperationType.Delete) => "plants/v1/destroyplants",
            (MetrcEntityType.Plant, MetrcOperationType.Harvest) => "plants/v1/harvestplants",
            (MetrcEntityType.Harvest, MetrcOperationType.Package) => "harvests/v1/createpackages",
            (MetrcEntityType.Harvest, MetrcOperationType.RecordWaste) => "harvests/v1/waste",
            (MetrcEntityType.Harvest, MetrcOperationType.Finish) => "harvests/v1/finish",
            (MetrcEntityType.Package, MetrcOperationType.Create) => "packages/v1/create",
            (MetrcEntityType.Package, MetrcOperationType.Adjust) => "packages/v1/adjust",
            (MetrcEntityType.Package, MetrcOperationType.Remediate) => "packages/v1/remediate",
            (MetrcEntityType.Package, MetrcOperationType.Finish) => "packages/v1/finish",
            (MetrcEntityType.Package, MetrcOperationType.Move) => "packages/v1/change/locations",
            (MetrcEntityType.Item, MetrcOperationType.Create) => "items/v1/create",
            (MetrcEntityType.Item, MetrcOperationType.Update) => "items/v1/update",
            (MetrcEntityType.Strain, MetrcOperationType.Create) => "strains/v1/create",
            (MetrcEntityType.Strain, MetrcOperationType.Update) => "strains/v1/update",
            (MetrcEntityType.Location, MetrcOperationType.Create) => "locations/v1/create",
            (MetrcEntityType.Transfer, MetrcOperationType.Create) => "transfers/v1/templates",
            (MetrcEntityType.Transfer, MetrcOperationType.Update) => "transfers/v1/templates",
            (MetrcEntityType.Transfer, MetrcOperationType.Delete) => item.MetrcId.HasValue ? $"transfers/v1/templates/{item.MetrcId.Value}" : null,
            _ => null
        };
    }

    // Placeholder decryption - in production use Azure Key Vault or similar
    private static string DecryptCredential(string encrypted)
    {
        return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encrypted));
    }
}

/// <summary>
/// Configuration options for the METRC sync worker
/// </summary>
public sealed class MetrcSyncWorkerOptions
{
    public const string SectionName = "Metrc:SyncWorker";

    /// <summary>
    /// Interval in seconds between processing cycles
    /// </summary>
    public int ProcessingIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Number of items to process per batch
    /// </summary>
    public int BatchSize { get; set; } = 50;

    /// <summary>
    /// Whether the worker is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;
}
