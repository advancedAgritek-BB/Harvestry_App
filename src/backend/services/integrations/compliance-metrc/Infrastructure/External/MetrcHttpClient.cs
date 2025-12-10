using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Harvestry.Compliance.Metrc.Infrastructure.External;

/// <summary>
/// HTTP client for METRC API communication with authentication, rate limiting, and retry logic
/// </summary>
public sealed class MetrcHttpClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MetrcHttpClient> _logger;
    private readonly MetrcApiConfiguration _config;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed;

    public MetrcHttpClient(
        HttpClient httpClient,
        ILogger<MetrcHttpClient> logger,
        IOptions<MetrcApiConfiguration> config)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
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

    /// <summary>
    /// Set authentication headers for a request
    /// </summary>
    public void SetAuthentication(string vendorApiKey, string userApiKey)
    {
        if (string.IsNullOrWhiteSpace(vendorApiKey))
            throw new ArgumentException("Vendor API key is required", nameof(vendorApiKey));

        if (string.IsNullOrWhiteSpace(userApiKey))
            throw new ArgumentException("User API key is required", nameof(userApiKey));

        // METRC uses Basic auth with vendor:user API keys
        var credentials = $"{vendorApiKey}:{userApiKey}";
        var encodedCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", encodedCredentials);
    }

    /// <summary>
    /// Execute a GET request
    /// </summary>
    public async Task<MetrcApiResponse<T>> GetAsync<T>(
        string endpoint,
        string licenseNumber,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(endpoint, licenseNumber);
        
        _logger.LogDebug("METRC GET: {Url}", url);

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            return await ProcessResponseAsync<T>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "METRC GET failed: {Url}", url);
            return MetrcApiResponse<T>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Execute a POST request
    /// </summary>
    public async Task<MetrcApiResponse<TResponse>> PostAsync<TRequest, TResponse>(
        string endpoint,
        string licenseNumber,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(endpoint, licenseNumber);
        var jsonContent = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        _logger.LogDebug("METRC POST: {Url}", url);
        if (_config.EnableDetailedLogging)
        {
            _logger.LogDebug("METRC POST Body: {Body}", jsonContent);
        }

        try
        {
            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            return await ProcessResponseAsync<TResponse>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "METRC POST failed: {Url}", url);
            return MetrcApiResponse<TResponse>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Execute a PUT request
    /// </summary>
    public async Task<MetrcApiResponse<TResponse>> PutAsync<TRequest, TResponse>(
        string endpoint,
        string licenseNumber,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(endpoint, licenseNumber);
        var jsonContent = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        _logger.LogDebug("METRC PUT: {Url}", url);

        try
        {
            var response = await _httpClient.PutAsync(url, content, cancellationToken);
            return await ProcessResponseAsync<TResponse>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "METRC PUT failed: {Url}", url);
            return MetrcApiResponse<TResponse>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Execute a DELETE request
    /// </summary>
    public async Task<MetrcApiResponse<object>> DeleteAsync(
        string endpoint,
        string licenseNumber,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(endpoint, licenseNumber);

        _logger.LogDebug("METRC DELETE: {Url}", url);

        try
        {
            var response = await _httpClient.DeleteAsync(url, cancellationToken);
            return await ProcessResponseAsync<object>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "METRC DELETE failed: {Url}", url);
            return MetrcApiResponse<object>.Failure(ex.Message);
        }
    }

    private string BuildUrl(string endpoint, string licenseNumber)
    {
        var separator = endpoint.Contains('?') ? "&" : "?";
        return $"{endpoint}{separator}licenseNumber={Uri.EscapeDataString(licenseNumber)}";
    }

    private async Task<MetrcApiResponse<T>> ProcessResponseAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var statusCode = (int)response.StatusCode;
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (_config.EnableDetailedLogging)
        {
            _logger.LogDebug("METRC Response [{StatusCode}]: {Body}", statusCode, responseBody);
        }

        if (response.IsSuccessStatusCode)
        {
            try
            {
                var data = string.IsNullOrWhiteSpace(responseBody)
                    ? default
                    : JsonSerializer.Deserialize<T>(responseBody, _jsonOptions);

                return MetrcApiResponse<T>.Success(data, statusCode);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize METRC response");
                return MetrcApiResponse<T>.Failure($"JSON deserialization error: {ex.Message}", statusCode);
            }
        }

        // Handle specific error codes
        var errorMessage = ExtractErrorMessage(responseBody) ?? response.ReasonPhrase ?? "Unknown error";

        _logger.LogWarning("METRC API error [{StatusCode}]: {Error}", statusCode, errorMessage);

        return MetrcApiResponse<T>.Failure(errorMessage, statusCode);
    }

    private static string? ExtractErrorMessage(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            
            // Try common error message patterns
            if (doc.RootElement.TryGetProperty("Message", out var message))
                return message.GetString();

            if (doc.RootElement.TryGetProperty("message", out message))
                return message.GetString();

            if (doc.RootElement.TryGetProperty("error", out var error))
                return error.GetString();

            if (doc.RootElement.TryGetProperty("Error", out error))
                return error.GetString();

            // Return raw body if no known error format
            return responseBody;
        }
        catch
        {
            return responseBody;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _httpClient?.Dispose();
        _disposed = true;
    }
}

/// <summary>
/// Response wrapper for METRC API calls
/// </summary>
public sealed class MetrcApiResponse<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int StatusCode { get; private set; }

    private MetrcApiResponse() { }

    public static MetrcApiResponse<T> Success(T? data, int statusCode = 200)
    {
        return new MetrcApiResponse<T>
        {
            IsSuccess = true,
            Data = data,
            StatusCode = statusCode
        };
    }

    public static MetrcApiResponse<T> Failure(string errorMessage, int statusCode = 0)
    {
        return new MetrcApiResponse<T>
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            StatusCode = statusCode
        };
    }
}








