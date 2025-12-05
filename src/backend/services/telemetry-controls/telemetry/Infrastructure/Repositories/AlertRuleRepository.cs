using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Domain.Entities;
using Harvestry.Telemetry.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Telemetry.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for alert rule operations.
/// </summary>
public class AlertRuleRepository : IAlertRuleRepository
{
    private readonly TelemetryDbContext _context;
    
    public AlertRuleRepository(TelemetryDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    public async Task<AlertRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AlertRules
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }
    
    public async Task<List<AlertRule>> GetActiveBySiteIdAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        return await _context.AlertRules
            .AsNoTracking()
            .Where(r => r.SiteId == siteId && r.IsActive)
            .OrderBy(r => r.RuleName)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AlertRule>> GetBySiteIdAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        return await _context.AlertRules
            .AsNoTracking()
            .Where(r => r.SiteId == siteId)
            .OrderBy(r => r.RuleName)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AlertRule>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AlertRules
            .AsNoTracking()
            .Where(r => r.IsActive)
            .OrderBy(r => r.SiteId)
            .ThenBy(r => r.RuleName)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<List<AlertRule>> GetByStreamIdAsync(Guid streamId, CancellationToken cancellationToken = default)
    {
        return await _context.AlertRules
            .AsNoTracking()
            .Where(r => r.StreamIds.Contains(streamId) && r.IsActive)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<AlertRule> CreateAsync(AlertRule rule, CancellationToken cancellationToken = default)
    {
        _context.AlertRules.Add(rule);
        await _context.SaveChangesAsync(cancellationToken);
        return rule;
    }
    
    public async Task UpdateAsync(AlertRule rule, CancellationToken cancellationToken = default)
    {
        _context.AlertRules.Update(rule);
        await _context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rule = await _context.AlertRules.FindAsync(new object[] { id }, cancellationToken);
        if (rule != null)
        {
            _context.AlertRules.Remove(rule);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
