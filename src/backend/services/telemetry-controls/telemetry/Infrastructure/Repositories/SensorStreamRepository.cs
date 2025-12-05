using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Domain.Entities;
using Harvestry.Telemetry.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Telemetry.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for sensor stream operations.
/// </summary>
public class SensorStreamRepository : ISensorStreamRepository
{
    private readonly TelemetryDbContext _context;
    
    public SensorStreamRepository(TelemetryDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    public async Task<SensorStream?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SensorStreams
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }
    
    public async Task<List<SensorStream>> GetBySiteIdAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        return await _context.SensorStreams
            .AsNoTracking()
            .Where(s => s.SiteId == siteId)
            .OrderBy(s => s.DisplayName)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<List<SensorStream>> GetByEquipmentIdAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        return await _context.SensorStreams
            .AsNoTracking()
            .Where(s => s.EquipmentId == equipmentId)
            .OrderBy(s => s.DisplayName)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<SensorStream> CreateAsync(SensorStream stream, CancellationToken cancellationToken = default)
    {
        _context.SensorStreams.Add(stream);
        await _context.SaveChangesAsync(cancellationToken);
        return stream;
    }
    
    public async Task UpdateAsync(SensorStream stream, CancellationToken cancellationToken = default)
    {
        _context.SensorStreams.Update(stream);
        await _context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SensorStreams
            .AnyAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<List<SensorStream>> GetByIdsAsync(IEnumerable<Guid> streamIds, CancellationToken cancellationToken = default)
    {
        var ids = streamIds?.Distinct().ToArray() ?? Array.Empty<Guid>();
        if (ids.Length == 0)
        {
            return new List<SensorStream>();
        }

        return await _context.SensorStreams
            .AsNoTracking()
            .Where(s => ids.Contains(s.Id))
            .ToListAsync(cancellationToken);
    }
}
