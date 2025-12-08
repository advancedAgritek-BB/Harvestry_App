namespace Harvestry.Integration.Growlink.Application.DTOs;

/// <summary>
/// OAuth token response from Growlink.
/// </summary>
public sealed record GrowlinkTokenResponse(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    int ExpiresIn,
    string? Scope);

/// <summary>
/// OAuth token request for authorization code exchange.
/// </summary>
public sealed record GrowlinkTokenRequest(
    string GrantType,
    string Code,
    string RedirectUri,
    string ClientId,
    string ClientSecret);

/// <summary>
/// OAuth token refresh request.
/// </summary>
public sealed record GrowlinkRefreshTokenRequest(
    string GrantType,
    string RefreshToken,
    string ClientId,
    string ClientSecret);

/// <summary>
/// Growlink account information from the API.
/// </summary>
public sealed record GrowlinkAccountDto(
    string AccountId,
    string Email,
    string Name,
    string? Organization);
