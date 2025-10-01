using System;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Application.Interfaces;
using Harvestry.Identity.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Harvestry.Identity.Infrastructure.Health;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly IdentityDbContext _dbContext;
    private readonly IRlsContextAccessor _rlsContextAccessor;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(
        IdentityDbContext dbContext,
        IRlsContextAccessor rlsContextAccessor,
        ILogger<DatabaseHealthCheck> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _rlsContextAccessor = rlsContextAccessor ?? throw new ArgumentNullException(nameof(rlsContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _rlsContextAccessor.Set(new RlsContext(Guid.Empty, "service_account", null));
            var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await _dbContext.SetRlsContextAsync(Guid.Empty, "service_account", null, cancellationToken).ConfigureAwait(false);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

            return HealthCheckResult.Healthy("Database connection successful.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Identity database health check failed");
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
        finally
        {
            _rlsContextAccessor.Clear();
        }
    }
}
