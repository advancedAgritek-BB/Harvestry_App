using Harvestry.Compliance.Metrc.Application.Interfaces;
using Harvestry.Compliance.Metrc.Domain.Entities;
using Harvestry.Compliance.Metrc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Compliance.Metrc.Infrastructure.Repositories;

/// <summary>
/// Repository for METRC license persistence
/// </summary>
public sealed class MetrcLicenseRepository : IMetrcLicenseRepository
{
    private readonly MetrcDbContext _context;

    public MetrcLicenseRepository(MetrcDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<MetrcLicense?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Licenses
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<MetrcLicense?> GetByLicenseNumberAsync(
        string licenseNumber,
        CancellationToken cancellationToken = default)
    {
        return await _context.Licenses
            .FirstOrDefaultAsync(l => l.LicenseNumber == licenseNumber.ToUpperInvariant(), cancellationToken);
    }

    public async Task<IReadOnlyList<MetrcLicense>> GetBySiteIdAsync(
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Licenses
            .Where(l => l.SiteId == siteId)
            .OrderBy(l => l.LicenseNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MetrcLicense>> GetDueForSyncAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        return await _context.Licenses
            .Where(l => l.IsActive
                && l.AutoSyncEnabled
                && l.VendorApiKeyEncrypted != null
                && l.UserApiKeyEncrypted != null
                && (l.LastSyncAt == null || l.LastSyncAt.Value.AddMinutes(l.SyncIntervalMinutes) <= now))
            .OrderBy(l => l.LastSyncAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MetrcLicense>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Licenses
            .Where(l => l.IsActive)
            .OrderBy(l => l.LicenseNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(MetrcLicense license, CancellationToken cancellationToken = default)
    {
        await _context.Licenses.AddAsync(license, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(MetrcLicense license, CancellationToken cancellationToken = default)
    {
        _context.Licenses.Update(license);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string licenseNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Licenses
            .AnyAsync(l => l.LicenseNumber == licenseNumber.ToUpperInvariant(), cancellationToken);
    }
}
