using Harvestry.Integration.Growlink.Domain.Entities;

namespace Harvestry.Integration.Growlink.Application.Interfaces;

/// <summary>
/// Repository for Growlink credentials.
/// </summary>
public interface IGrowlinkCredentialRepository
{
    /// <summary>
    /// Gets the credential for a site.
    /// </summary>
    Task<GrowlinkCredential?> GetBySiteIdAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active credentials for sync.
    /// </summary>
    Task<List<GrowlinkCredential>> GetActiveCredentialsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new credential.
    /// </summary>
    Task CreateAsync(
        GrowlinkCredential credential,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing credential.
    /// </summary>
    Task UpdateAsync(
        GrowlinkCredential credential,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a credential.
    /// </summary>
    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}





