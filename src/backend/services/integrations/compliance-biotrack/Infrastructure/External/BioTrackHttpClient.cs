using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Harvestry.Compliance.BioTrack.Infrastructure.External;

/// <summary>
/// HTTP client for BioTrack API communication.
/// BioTrack uses JSON-RPC style API over HTTPS.
/// </summary>
public sealed class BioTrackHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _username;
    private readonly string _password;
    private readonly ILogger<BioTrackHttpClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public BioTrackHttpClient(
        HttpClient httpClient,
        string baseUrl,
        string username,
        string password,
        ILogger<BioTrackHttpClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        _username = username ?? throw new ArgumentNullException(nameof(username));
        _password = password ?? throw new ArgumentNullException(nameof(password));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Executes a BioTrack API action
    /// </summary>
    public async Task<BioTrackApiResponse<T>> ExecuteAsync<T>(
        string action,
        object parameters,
        CancellationToken cancellationToken = default)
    {
        var request = new BioTrackRequest
        {
            Action = action,
            Username = _username,
            Password = _password,
            Data = parameters
        };

        try
        {
            _logger.LogDebug("Executing BioTrack action: {Action}", action);

            var response = await _httpClient.PostAsJsonAsync(
                _baseUrl,
                request,
                _jsonOptions,
                cancellationToken);

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "BioTrack API error: {StatusCode} - {Content}",
                    response.StatusCode, responseContent);

                return new BioTrackApiResponse<T>
                {
                    Success = false,
                    ErrorCode = ((int)response.StatusCode).ToString(),
                    ErrorMessage = $"HTTP {response.StatusCode}: {responseContent}"
                };
            }

            var result = JsonSerializer.Deserialize<BioTrackRawResponse<T>>(
                responseContent, _jsonOptions);

            if (result == null)
            {
                return new BioTrackApiResponse<T>
                {
                    Success = false,
                    ErrorCode = "PARSE_ERROR",
                    ErrorMessage = "Failed to parse BioTrack response"
                };
            }

            if (result.Success != 1)
            {
                return new BioTrackApiResponse<T>
                {
                    Success = false,
                    ErrorCode = result.ErrorCode,
                    ErrorMessage = result.ErrorMessage
                };
            }

            return new BioTrackApiResponse<T>
            {
                Success = true,
                Data = result.Data,
                TransactionId = result.TransactionId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BioTrack API exception for action {Action}", action);
            return new BioTrackApiResponse<T>
            {
                Success = false,
                ErrorCode = "EXCEPTION",
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Syncs plants from BioTrack
    /// </summary>
    public Task<BioTrackApiResponse<IEnumerable<BioTrackPlant>>> SyncPlantsAsync(
        string licenseNumber,
        DateTime? lastSyncDate = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new { license_number = licenseNumber };
        return ExecuteAsync<IEnumerable<BioTrackPlant>>("sync_plant", parameters, cancellationToken);
    }

    /// <summary>
    /// Syncs inventory from BioTrack
    /// </summary>
    public Task<BioTrackApiResponse<IEnumerable<BioTrackInventory>>> SyncInventoryAsync(
        string licenseNumber,
        DateTime? lastSyncDate = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new { license_number = licenseNumber };
        return ExecuteAsync<IEnumerable<BioTrackInventory>>("sync_inventory", parameters, cancellationToken);
    }

    /// <summary>
    /// Creates inventory in BioTrack
    /// </summary>
    public Task<BioTrackApiResponse<object>> CreateInventoryAsync(
        string licenseNumber,
        object inventoryData,
        CancellationToken cancellationToken = default)
    {
        var parameters = new { license_number = licenseNumber, inventory = inventoryData };
        return ExecuteAsync<object>("inventory_new", parameters, cancellationToken);
    }

    /// <summary>
    /// Adjusts inventory in BioTrack
    /// </summary>
    public Task<BioTrackApiResponse<object>> AdjustInventoryAsync(
        string licenseNumber,
        string inventoryId,
        decimal quantity,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var parameters = new 
        { 
            license_number = licenseNumber, 
            inventoryid = inventoryId,
            quantity = quantity,
            reason = reason
        };
        return ExecuteAsync<object>("inventory_adjust", parameters, cancellationToken);
    }
}

/// <summary>
/// BioTrack API request format
/// </summary>
internal sealed class BioTrackRequest
{
    public string Action { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public object? Data { get; set; }
}

/// <summary>
/// Raw BioTrack API response
/// </summary>
internal sealed class BioTrackRawResponse<T>
{
    public int Success { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? TransactionId { get; set; }
    public T? Data { get; set; }
}

/// <summary>
/// Wrapped BioTrack API response
/// </summary>
public sealed class BioTrackApiResponse<T>
{
    public bool Success { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? TransactionId { get; set; }
    public T? Data { get; set; }
}

/// <summary>
/// BioTrack plant data
/// </summary>
public sealed record BioTrackPlant
{
    public string PlantId { get; init; } = string.Empty;
    public string Strain { get; init; } = string.Empty;
    public string Room { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public DateTime? PlantedDate { get; init; }
    public DateTime? HarvestedDate { get; init; }
}

/// <summary>
/// BioTrack inventory data
/// </summary>
public sealed record BioTrackInventory
{
    public string InventoryId { get; init; } = string.Empty;
    public string InventoryType { get; init; } = string.Empty;
    public string Strain { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public string UnitOfMeasure { get; init; } = string.Empty;
    public string Room { get; init; } = string.Empty;
}
