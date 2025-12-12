using Harvestry.Compliance.Metrc.Application.DTOs;
using Harvestry.Compliance.Metrc.Application.Interfaces;
using Harvestry.Compliance.Metrc.Domain.Entities;
using Harvestry.Compliance.Metrc.Infrastructure.External;
using Microsoft.Extensions.Logging;

namespace Harvestry.Compliance.Metrc.Application.Services;

/// <summary>
/// Service for managing METRC license configurations
/// </summary>
public sealed class MetrcLicenseService : IMetrcLicenseService
{
    private readonly IMetrcLicenseRepository _licenseRepository;
    private readonly MetrcHttpClientFactory _httpClientFactory;
    private readonly ILogger<MetrcLicenseService> _logger;

    public MetrcLicenseService(
        IMetrcLicenseRepository licenseRepository,
        MetrcHttpClientFactory httpClientFactory,
        ILogger<MetrcLicenseService> logger)
    {
        _licenseRepository = licenseRepository ?? throw new ArgumentNullException(nameof(licenseRepository));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LicenseDto?> GetLicenseAsync(
        Guid licenseId,
        CancellationToken cancellationToken = default)
    {
        var license = await _licenseRepository.GetByIdAsync(licenseId, cancellationToken);
        return license != null ? MapToDto(license) : null;
    }

    public async Task<IReadOnlyList<LicenseDto>> GetLicensesForSiteAsync(
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var licenses = await _licenseRepository.GetBySiteIdAsync(siteId, cancellationToken);
        return licenses.Select(MapToDto).ToList();
    }

    public async Task<LicenseDto?> GetLicenseByNumberAsync(
        string licenseNumber,
        CancellationToken cancellationToken = default)
    {
        var license = await _licenseRepository.GetByLicenseNumberAsync(licenseNumber, cancellationToken);
        return license != null ? MapToDto(license) : null;
    }

    public async Task<LicenseDto> UpsertLicenseAsync(
        UpsertLicenseRequest request,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var existing = await _licenseRepository.GetByLicenseNumberAsync(
            request.LicenseNumber, cancellationToken);

        MetrcLicense license;

        if (existing != null)
        {
            // Update existing
            existing.UpdateSyncSettings(request.AutoSyncEnabled, request.SyncIntervalMinutes, userId);
            existing.SetSandboxMode(request.UseSandbox, userId);
            await _licenseRepository.UpdateAsync(existing, cancellationToken);
            license = existing;

            _logger.LogInformation(
                "Updated METRC license {LicenseNumber} for site {SiteId}",
                request.LicenseNumber, request.SiteId);
        }
        else
        {
            // Create new
            license = MetrcLicense.Create(
                request.SiteId,
                request.LicenseNumber,
                request.StateCode,
                request.FacilityName,
                userId,
                request.UseSandbox,
                request.AutoSyncEnabled,
                request.SyncIntervalMinutes);

            await _licenseRepository.CreateAsync(license, cancellationToken);

            _logger.LogInformation(
                "Created METRC license {LicenseNumber} for site {SiteId} (state: {StateCode})",
                request.LicenseNumber, request.SiteId, request.StateCode);
        }

        return MapToDto(license);
    }

    public async Task<bool> SetCredentialsAsync(
        SetCredentialsRequest request,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var license = await _licenseRepository.GetByIdAsync(request.LicenseId, cancellationToken);
        if (license == null)
        {
            return false;
        }

        // In production, credentials should be encrypted before storage
        // For now, we store them as-is (would use Azure Key Vault or similar)
        license.SetCredentials(
            EncryptCredential(request.VendorApiKey),
            EncryptCredential(request.UserApiKey),
            userId);

        await _licenseRepository.UpdateAsync(license, cancellationToken);

        _logger.LogInformation(
            "Updated credentials for METRC license {LicenseNumber}",
            license.LicenseNumber);

        return true;
    }

    public async Task<bool> ActivateLicenseAsync(
        Guid licenseId,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var license = await _licenseRepository.GetByIdAsync(licenseId, cancellationToken);
        if (license == null)
        {
            return false;
        }

        license.Activate(userId);
        await _licenseRepository.UpdateAsync(license, cancellationToken);

        _logger.LogInformation("Activated METRC license {LicenseNumber}", license.LicenseNumber);
        return true;
    }

    public async Task<bool> DeactivateLicenseAsync(
        Guid licenseId,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var license = await _licenseRepository.GetByIdAsync(licenseId, cancellationToken);
        if (license == null)
        {
            return false;
        }

        license.Deactivate(userId);
        await _licenseRepository.UpdateAsync(license, cancellationToken);

        _logger.LogInformation("Deactivated METRC license {LicenseNumber}", license.LicenseNumber);
        return true;
    }

    public async Task<(bool Success, string Message)> TestConnectionAsync(
        Guid licenseId,
        CancellationToken cancellationToken = default)
    {
        var license = await _licenseRepository.GetByIdAsync(licenseId, cancellationToken);
        if (license == null)
        {
            return (false, "License not found");
        }

        if (!license.HasCredentials)
        {
            return (false, "License does not have credentials configured");
        }

        try
        {
            // Create HTTP client for this license
            var client = _httpClientFactory.CreateClient(
                license.StateCode,
                DecryptCredential(license.VendorApiKeyEncrypted!),
                DecryptCredential(license.UserApiKeyEncrypted!),
                license.UseSandbox);

            // Test connection by fetching facilities
            var response = await client.GetAsync<object>(
                "facilities/v1",
                license.LicenseNumber,
                cancellationToken);

            if (response.IsSuccess)
            {
                _logger.LogInformation(
                    "Connection test successful for METRC license {LicenseNumber}",
                    license.LicenseNumber);
                return (true, "Connection successful");
            }
            else
            {
                _logger.LogWarning(
                    "Connection test failed for METRC license {LicenseNumber}: {Error}",
                    license.LicenseNumber, response.ErrorMessage);
                return (false, response.ErrorMessage ?? "Connection failed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Connection test error for METRC license {LicenseNumber}",
                license.LicenseNumber);
            return (false, $"Connection error: {ex.Message}");
        }
    }

    public async Task<IReadOnlyList<MetrcLicense>> GetLicensesDueForSyncAsync(
        CancellationToken cancellationToken = default)
    {
        return await _licenseRepository.GetDueForSyncAsync(cancellationToken);
    }

    private static LicenseDto MapToDto(MetrcLicense license)
    {
        return new LicenseDto
        {
            Id = license.Id,
            SiteId = license.SiteId,
            LicenseNumber = license.LicenseNumber,
            StateCode = license.StateCode,
            FacilityName = license.FacilityName,
            IsActive = license.IsActive,
            UseSandbox = license.UseSandbox,
            AutoSyncEnabled = license.AutoSyncEnabled,
            SyncIntervalMinutes = license.SyncIntervalMinutes,
            HasCredentials = license.HasCredentials,
            LastSyncAt = license.LastSyncAt,
            LastSuccessfulSyncAt = license.LastSuccessfulSyncAt,
            LastSyncError = license.LastSyncError,
            IsSyncDue = license.IsSyncDue
        };
    }

    // Placeholder encryption/decryption - in production use Azure Key Vault or similar
    private static string EncryptCredential(string credential)
    {
        // TODO: Implement proper encryption using KMS/Vault
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(credential));
    }

    private static string DecryptCredential(string encrypted)
    {
        // TODO: Implement proper decryption using KMS/Vault
        return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encrypted));
    }
}
