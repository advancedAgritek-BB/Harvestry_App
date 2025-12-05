using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Harvestry.Tasks.Infrastructure.Readiness;

public sealed class UserReadinessProvider : IUserReadinessProvider
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<UserReadinessProvider> _logger;

    public UserReadinessProvider(NpgsqlDataSource dataSource, ILogger<UserReadinessProvider> logger)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyCollection<Guid>> GetCompletedSopIdsAsync(Guid userId, IReadOnlyCollection<Guid> requiredSopIds, CancellationToken cancellationToken)
    {
        if (requiredSopIds.Count == 0)
        {
            return Array.Empty<Guid>();
        }

        const string sql = @"
            SELECT sop_id
            FROM sop_signoffs
            WHERE user_id = @userId
              AND revoked_at IS NULL
              AND sop_id = ANY(@sopIds);
        ";

        return await ExecuteGuidQueryAsync(sql, "sopIds", userId, requiredSopIds, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<Guid>> GetCompletedTrainingIdsAsync(Guid userId, IReadOnlyCollection<Guid> requiredTrainingIds, CancellationToken cancellationToken)
    {
        if (requiredTrainingIds.Count == 0)
        {
            return Array.Empty<Guid>();
        }

        const string sql = @"
            SELECT module_id
            FROM training_assignments
            WHERE user_id = @userId
              AND status = 'completed'
              AND (passed IS NULL OR passed = TRUE)
              AND module_id = ANY(@moduleIds);
        ";

        return await ExecuteGuidQueryAsync(sql, "moduleIds", userId, requiredTrainingIds, cancellationToken).ConfigureAwait(false);
    }

    private async Task<IReadOnlyCollection<Guid>> ExecuteGuidQueryAsync(
        string sql,
        string arrayParameterName,
        Guid userId,
        IReadOnlyCollection<Guid> filterIds,
        CancellationToken cancellationToken)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        var userParameter = command.Parameters.Add("userId", NpgsqlDbType.Uuid);
        userParameter.Value = userId;

        var arrayParameter = command.Parameters.Add(arrayParameterName, NpgsqlDbType.Array | NpgsqlDbType.Uuid);
        arrayParameter.Value = filterIds is Guid[] guidArray ? guidArray : new List<Guid>(filterIds).ToArray();

        var results = new List<Guid>(filterIds.Count);

        try
        {
            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                results.Add(reader.GetGuid(0));
            }
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "Failed to query readiness data for user {UserId}.", userId);
            throw;
        }

        return results;
    }
}
