using Microsoft.Extensions.Logging;

namespace Harvestry.Compliance.Metrc.Infrastructure.External;

/// <summary>
/// Factory for creating state-specific METRC HTTP clients
/// </summary>
public sealed class MetrcHttpClientFactory : IMetrcHttpClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggerFactory _loggerFactory;

    public MetrcHttpClientFactory(
        IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    /// <summary>
    /// Create a METRC client configured for a specific state and credentials
    /// </summary>
    public MetrcHttpClient CreateClient(
        string stateCode,
        string vendorApiKey,
        string userApiKey,
        bool useSandbox = false)
    {
        if (string.IsNullOrWhiteSpace(stateCode))
            throw new ArgumentException("State code is required", nameof(stateCode));

        if (string.IsNullOrWhiteSpace(vendorApiKey))
            throw new ArgumentException("Vendor API key is required", nameof(vendorApiKey));

        if (string.IsNullOrWhiteSpace(userApiKey))
            throw new ArgumentException("User API key is required", nameof(userApiKey));

        var baseUrl = useSandbox
            ? MetrcApiConfiguration.GetSandboxUrlForState(stateCode)
            : MetrcApiConfiguration.GetBaseUrlForState(stateCode);

        var config = new MetrcApiConfiguration
        {
            BaseUrl = baseUrl,
            VendorApiKey = vendorApiKey
        };

        var httpClient = _httpClientFactory.CreateClient($"Metrc_{stateCode}");
        var logger = _loggerFactory.CreateLogger<MetrcHttpClient>();

        var client = new MetrcHttpClient(
            httpClient,
            logger,
            Microsoft.Extensions.Options.Options.Create(config));

        client.SetAuthentication(vendorApiKey, userApiKey);

        return client;
    }
}

/// <summary>
/// Interface for METRC HTTP client factory
/// </summary>
public interface IMetrcHttpClientFactory
{
    /// <summary>
    /// Create a METRC client for a specific state
    /// </summary>
    MetrcHttpClient CreateClient(
        string stateCode,
        string vendorApiKey,
        string userApiKey,
        bool useSandbox = false);
}








