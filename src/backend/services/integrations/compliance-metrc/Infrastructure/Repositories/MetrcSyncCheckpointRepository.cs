using Harvestry.Compliance.Metrc.Application.Interfaces;
using Harvestry.Compliance.Metrc.Domain.Entities;
using Harvestry.Compliance.Metrc.Domain.Enums;
using Harvestry.Compliance.Metrc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Compliance.Metrc.Infrastructure.Repositories;

/// <summary>
/// Repository for METRC sync checkpoint persistence
/// </summary>
public sealed class MetrcSyncCheckpointRepository : IMetrcSyncCheckpointRepository
{
    private readonly MetrcDbContext _context;

    public MetrcSyncCheckpointRepository(MetrcDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<MetrcSyncCheckpoint?> GetAsync(
        Guid licenseId,
        MetrcEntityType entityType,
        SyncDirection direction,
        CancellationToken cancellationToken = default)
    {
        return await _context.SyncCheckpoints
            .FirstOrDefaultAsync(c =>
                c.LicenseId == licenseId
                && c.EntityType == entityType
                && c.Direction == direction,
                cancellationToken);
    }

    public async Task<IReadOnlyList<MetrcSyncCheckpoint>> GetByLicenseIdAsync(
        Guid licenseId,
        CancellationToken cancellationToken = default)
    {
        return await _context.SyncCheckpoints
            .Where(c => c.LicenseId == licenseId)
            .OrderBy(c => c.EntityType)
            .ThenBy(c => c.Direction)
            .ToListAsync(cancellationToken);
    }

    public async Task UpsertAsync(
        MetrcSyncCheckpoint checkpoint,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetAsync(
            checkpoint.LicenseId,
            checkpoint.EntityType,
            checkpoint.Direction,
            cancellationToken);

        if (existing == null)
        {
            await _context.SyncCheckpoints.AddAsync(checkpoint, cancellationToken);
        }
        else
        {
            _context.Entry(existing).CurrentValues.SetValues(checkpoint);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetAsync(
        Guid licenseId,
        MetrcEntityType? entityType = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.SyncCheckpoints.Where(c => c.LicenseId == licenseId);

        if (entityType.HasValue)
        {
            query = query.Where(c => c.EntityType == entityType.Value);
        }

        var checkpoints = await query.ToListAsync(cancellationToken);

        foreach (var checkpoint in checkpoints)
        {
            checkpoint.Reset();
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
