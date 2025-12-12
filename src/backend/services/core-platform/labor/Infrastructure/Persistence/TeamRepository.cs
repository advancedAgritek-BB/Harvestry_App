using Harvestry.Labor.Application.Interfaces;
using Harvestry.Labor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Labor.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for Team aggregate
/// </summary>
public class TeamRepository : ITeamRepository
{
    private readonly LaborDbContext _context;

    public TeamRepository(LaborDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Team?> GetByIdAsync(Guid teamId, CancellationToken ct = default)
    {
        return await _context.Teams
            .FirstOrDefaultAsync(t => t.Id == teamId, ct);
    }

    public async Task<Team?> GetByIdWithMembersAsync(Guid teamId, CancellationToken ct = default)
    {
        return await _context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == teamId, ct);
    }

    public async Task<IReadOnlyList<Team>> GetBySiteIdAsync(Guid siteId, CancellationToken ct = default)
    {
        return await _context.Teams
            .Where(t => t.SiteId == siteId && t.Status == TeamStatus.Active)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Team>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Teams
            .Include(t => t.Members)
            .Where(t => t.Status == TeamStatus.Active &&
                        t.Members.Any(m => m.UserId == userId && m.RemovedAt == null))
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Team>> GetByTeamLeadAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Teams
            .Include(t => t.Members)
            .Where(t => t.Status == TeamStatus.Active &&
                        t.Members.Any(m => m.UserId == userId && m.IsTeamLead && m.RemovedAt == null))
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsWithNameAsync(Guid siteId, string name, Guid? excludeTeamId = null, CancellationToken ct = default)
    {
        var query = _context.Teams
            .Where(t => t.SiteId == siteId &&
                        t.Name.ToLower() == name.ToLower() &&
                        t.Status == TeamStatus.Active);

        if (excludeTeamId.HasValue)
        {
            query = query.Where(t => t.Id != excludeTeamId.Value);
        }

        return await query.AnyAsync(ct);
    }

    public async Task AddAsync(Team team, CancellationToken ct = default)
    {
        await _context.Teams.AddAsync(team, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Team team, CancellationToken ct = default)
    {
        _context.Teams.Update(team);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid teamId, CancellationToken ct = default)
    {
        var team = await GetByIdAsync(teamId, ct);
        if (team != null)
        {
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync(ct);
        }
    }
}
