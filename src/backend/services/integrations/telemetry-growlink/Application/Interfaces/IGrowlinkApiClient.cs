using Harvestry.Integration.Growlink.Application.DTOs;

namespace Harvestry.Integration.Growlink.Application.Interfaces;

/// <summary>
/// Client for interacting with the Growlink API.
/// </summary>
public interface IGrowlinkApiClient
{
    /// <summary>
    /// Exchanges an authorization code for access tokens.
    /// </summary>
    Task<GrowlinkApiResponse<GrowlinkTokenResponse>> ExchangeCodeForTokensAsync(
        string authorizationCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes an access token using a refresh token.
    /// </summary>
    Task<GrowlinkApiResponse<GrowlinkTokenResponse>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the authenticated user's account information.
    /// </summary>
    Task<GrowlinkApiResponse<GrowlinkAccountDto>> GetAccountAsync(
        string accessToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all devices for the authenticated account.
    /// </summary>
    Task<GrowlinkApiResponse<List<GrowlinkDeviceDto>>> GetDevicesAsync(
        string accessToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest sensor readings for all devices.
    /// </summary>
    Task<GrowlinkApiResponse<GrowlinkReadingsBatchDto>> GetLatestReadingsAsync(
        string accessToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets sensor readings within a time range.
    /// </summary>
    Task<GrowlinkApiResponse<GrowlinkReadingsBatchDto>> GetReadingsAsync(
        string accessToken,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes the access token.
    /// </summary>
    Task<GrowlinkApiResponse<bool>> RevokeTokenAsync(
        string accessToken,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Response wrapper for Growlink API calls.
/// </summary>
public sealed class GrowlinkApiResponse<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int StatusCode { get; private set; }
    public bool IsRateLimited { get; private set; }
    public TimeSpan? RetryAfter { get; private set; }

    private GrowlinkApiResponse() { }

    public static GrowlinkApiResponse<T> Success(T? data, int statusCode = 200)
    {
        return new GrowlinkApiResponse<T>
        {
            IsSuccess = true,
            Data = data,
            StatusCode = statusCode
        };
    }

    public static GrowlinkApiResponse<T> Failure(string errorMessage, int statusCode = 0)
    {
        return new GrowlinkApiResponse<T>
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            StatusCode = statusCode
        };
    }

    public static GrowlinkApiResponse<T> RateLimited(TimeSpan retryAfter)
    {
        return new GrowlinkApiResponse<T>
        {
            IsSuccess = false,
            IsRateLimited = true,
            RetryAfter = retryAfter,
            StatusCode = 429,
            ErrorMessage = $"Rate limited. Retry after {retryAfter.TotalSeconds} seconds."
        };
    }
}
