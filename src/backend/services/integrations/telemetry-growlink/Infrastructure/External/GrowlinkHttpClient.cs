using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Harvestry.Integration.Growlink.Application.DTOs;
using Harvestry.Integration.Growlink.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Harvestry.Integration.Growlink.Infrastructure.External;

/// <summary>
/// HTTP client for Growlink API with OAuth, rate limiting, and retry support.
/// </summary>
public sealed class GrowlinkHttpClient : IGrowlinkApiClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GrowlinkHttpClient> _logger;
    private readonly GrowlinkApiConfiguration _config;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed;

    public GrowlinkHttpClient(
        HttpClient httpClient,
        ILogger<GrowlinkHttpClient> logger,
        IOptions<GrowlinkApiConfiguration> config)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<GrowlinkApiResponse<GrowlinkTokenResponse>> ExchangeCodeForTokensAsync(
        string authorizationCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(authorizationCode))
            throw new ArgumentException("Authorization code is required", nameof(authorizationCode));

        var tokenRequest = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = authorizationCode,
            ["redirect_uri"] = _config.RedirectUri,
            ["client_id"] = _config.ClientId,
            ["client_secret"] = _config.ClientSecret
        };

        return await PostFormAsync<GrowlinkTokenResponse>(
            _config.TokenEndpoint,
            tokenRequest,
            cancellationToken);
    }

    public async Task<GrowlinkApiResponse<GrowlinkTokenResponse>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentException("Refresh token is required", nameof(refreshToken));

        var tokenRequest = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
            ["client_id"] = _config.ClientId,
            ["client_secret"] = _config.ClientSecret
        };

        return await PostFormAsync<GrowlinkTokenResponse>(
            _config.TokenEndpoint,
            tokenRequest,
            cancellationToken);
    }

    public async Task<GrowlinkApiResponse<GrowlinkAccountDto>> GetAccountAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<GrowlinkAccountDto>(
            "/api/v1/account",
            accessToken,
            cancellationToken);
    }

    public async Task<GrowlinkApiResponse<List<GrowlinkDeviceDto>>> GetDevicesAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<GrowlinkDeviceDto>>(
            "/api/v1/devices",
            accessToken,
            cancellationToken);
    }

    public async Task<GrowlinkApiResponse<GrowlinkReadingsBatchDto>> GetLatestReadingsAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        var response = await GetAsync<GrowlinkReadingsApiResponse>(
            "/api/v1/readings/latest",
            accessToken,
            cancellationToken);

        if (!response.IsSuccess)
        {
            return GrowlinkApiResponse<GrowlinkReadingsBatchDto>.Failure(
                response.ErrorMessage ?? "Failed to get readings",
                response.StatusCode);
        }

        var batch = MapToReadingsBatch(response.Data);
        return GrowlinkApiResponse<GrowlinkReadingsBatchDto>.Success(batch, response.StatusCode);
    }

    public async Task<GrowlinkApiResponse<GrowlinkReadingsBatchDto>> GetReadingsAsync(
        string accessToken,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        var fromStr = from.ToString("o");
        var toStr = to.ToString("o");
        var endpoint = $"/api/v1/readings?from={Uri.EscapeDataString(fromStr)}&to={Uri.EscapeDataString(toStr)}";

        var response = await GetAsync<GrowlinkReadingsApiResponse>(
            endpoint,
            accessToken,
            cancellationToken);

        if (!response.IsSuccess)
        {
            return GrowlinkApiResponse<GrowlinkReadingsBatchDto>.Failure(
                response.ErrorMessage ?? "Failed to get readings",
                response.StatusCode);
        }

        var batch = MapToReadingsBatch(response.Data);
        return GrowlinkApiResponse<GrowlinkReadingsBatchDto>.Success(batch, response.StatusCode);
    }

    public async Task<GrowlinkApiResponse<bool>> RevokeTokenAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        var request = new Dictionary<string, string>
        {
            ["token"] = accessToken,
            ["client_id"] = _config.ClientId,
            ["client_secret"] = _config.ClientSecret
        };

        var response = await PostFormAsync<object>(
            "/oauth/revoke",
            request,
            cancellationToken);

        return response.IsSuccess
            ? GrowlinkApiResponse<bool>.Success(true, response.StatusCode)
            : GrowlinkApiResponse<bool>.Failure(response.ErrorMessage ?? "Revoke failed", response.StatusCode);
    }

    private async Task<GrowlinkApiResponse<T>> GetAsync<T>(
        string endpoint,
        string accessToken,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        _logger.LogDebug("Growlink GET: {Endpoint}", endpoint);

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            return await ProcessResponseAsync<T>(response, cancellationToken);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Growlink GET timeout: {Endpoint}", endpoint);
            return GrowlinkApiResponse<T>.Failure("Request timed out");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Growlink GET network error: {Endpoint}", endpoint);
            return GrowlinkApiResponse<T>.Failure($"Network error: {ex.Message}");
        }
    }

    private async Task<GrowlinkApiResponse<T>> PostFormAsync<T>(
        string endpoint,
        Dictionary<string, string> formData,
        CancellationToken cancellationToken)
    {
        using var content = new FormUrlEncodedContent(formData);
        
        _logger.LogDebug("Growlink POST: {Endpoint}", endpoint);

        try
        {
            using var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
            return await ProcessResponseAsync<T>(response, cancellationToken);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Growlink POST timeout: {Endpoint}", endpoint);
            return GrowlinkApiResponse<T>.Failure("Request timed out");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Growlink POST network error: {Endpoint}", endpoint);
            return GrowlinkApiResponse<T>.Failure($"Network error: {ex.Message}");
        }
    }

    private async Task<GrowlinkApiResponse<T>> ProcessResponseAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var statusCode = (int)response.StatusCode;
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (_config.EnableDetailedLogging)
        {
            _logger.LogDebug("Growlink Response [{StatusCode}]: {Body}", statusCode, responseBody);
        }

        // Handle rate limiting
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(60);
            _logger.LogWarning("Growlink rate limited. Retry after: {RetryAfter}s", retryAfter.TotalSeconds);
            return GrowlinkApiResponse<T>.RateLimited(retryAfter);
        }

        if (response.IsSuccessStatusCode)
        {
            try
            {
                var data = string.IsNullOrWhiteSpace(responseBody)
                    ? default
                    : JsonSerializer.Deserialize<T>(responseBody, _jsonOptions);

                return GrowlinkApiResponse<T>.Success(data, statusCode);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize Growlink response");
                return GrowlinkApiResponse<T>.Failure($"JSON deserialization error: {ex.Message}", statusCode);
            }
        }

        var errorMessage = ExtractErrorMessage(responseBody) ?? response.ReasonPhrase ?? "Unknown error";
        _logger.LogWarning("Growlink API error [{StatusCode}]: {Error}", statusCode, errorMessage);

        return GrowlinkApiResponse<T>.Failure(errorMessage, statusCode);
    }

    private static string? ExtractErrorMessage(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(responseBody);

            if (doc.RootElement.TryGetProperty("error_description", out var errorDesc))
                return errorDesc.GetString();

            if (doc.RootElement.TryGetProperty("error", out var error))
                return error.GetString();

            if (doc.RootElement.TryGetProperty("message", out var message))
                return message.GetString();

            return responseBody;
        }
        catch
        {
            return responseBody;
        }
    }

    private static GrowlinkReadingsBatchDto MapToReadingsBatch(GrowlinkReadingsApiResponse? apiResponse)
    {
        if (apiResponse?.Readings == null)
        {
            return new GrowlinkReadingsBatchDto(new List<GrowlinkSensorReadingDto>(), DateTimeOffset.UtcNow);
        }

        var readings = apiResponse.Readings.Select(r => new GrowlinkSensorReadingDto(
            r.DeviceId,
            r.SensorId,
            r.Value,
            r.Unit,
            r.Timestamp)).ToList();

        return new GrowlinkReadingsBatchDto(readings, DateTimeOffset.UtcNow);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _httpClient?.Dispose();
        _disposed = true;
    }

    /// <summary>
    /// Internal API response model for readings.
    /// </summary>
    private sealed class GrowlinkReadingsApiResponse
    {
        public List<GrowlinkReadingApiItem>? Readings { get; set; }
    }

    private sealed class GrowlinkReadingApiItem
    {
        public string DeviceId { get; set; } = string.Empty;
        public string SensorId { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTimeOffset Timestamp { get; set; }
    }
}
