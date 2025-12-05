using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Harvestry.Telemetry.Application.Interfaces;

/// <summary>
/// Factory for creating Npgsql connections configured with telemetry RLS context.
/// </summary>
public interface ITelemetryConnectionFactory
{
    Task<NpgsqlConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);

    Task ConfigureConnectionAsync(NpgsqlConnection connection, CancellationToken cancellationToken = default);
}
