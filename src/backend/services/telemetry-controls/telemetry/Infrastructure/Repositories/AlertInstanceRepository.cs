using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Domain.Entities;
using Harvestry.Telemetry.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harvestry.Telemetry.Infrastructure.Repositories;

/// <summary>
/// EF Core backed repository for alert instance persistence.
/// </summary>
public sealed class AlertInstanceRepository : IAlertInstanceRepository
{
    private readonly TelemetryDbContext _context;
    private readonly ILogger<AlertInstanceRepository> _logger;

    public AlertInstanceRepository(
        TelemetryDbContext context,
        ILogger<AlertInstanceRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AlertInstance?> GetActiveByRuleAndStreamAsync(Guid ruleId, Guid streamId, CancellationToken cancellationToken = default)
    {
        return await _context.AlertInstances
            .AsNoTracking()
            .Where(ai => ai.RuleId == ruleId && ai.StreamId == streamId && ai.ClearedAt == null)
            .OrderByDescending(ai => ai.FiredAt)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<AlertInstance>> GetActiveBySiteAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        return await _context.AlertInstances
            .AsNoTracking()
            .Where(ai => ai.SiteId == siteId && ai.ClearedAt == null)
            .OrderByDescending(ai => ai.FiredAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<AlertInstance?> GetByIdAsync(Guid alertId, CancellationToken cancellationToken = default)
    {
        return await _context.AlertInstances
            .FirstOrDefaultAsync(ai => ai.Id == alertId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task CreateAsync(AlertInstance alert, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(alert);

        await _context.AlertInstances.AddAsync(alert, cancellationToken).ConfigureAwait(false);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(AlertInstance alert, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(alert);

        _context.AlertInstances.Update(alert);
        var rows = await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (rows == 0)
        {
            _logger.LogWarning("Alert instance update affected zero rows for alert {AlertId}", alert.Id);
        }
    }
}
