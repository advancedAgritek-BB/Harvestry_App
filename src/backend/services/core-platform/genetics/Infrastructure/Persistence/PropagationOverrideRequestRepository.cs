using Harvestry.Genetics.Application.Interfaces;
using Harvestry.Genetics.Domain.Entities;
using Harvestry.Genetics.Domain.Enums;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Harvestry.Genetics.Infrastructure.Persistence;

/// <summary>
/// Repository for propagation override requests.
/// </summary>
public sealed class PropagationOverrideRequestRepository : IPropagationOverrideRequestRepository
{
    private readonly GeneticsDbContext _dbContext;
    private readonly ILogger<PropagationOverrideRequestRepository> _logger;

    public PropagationOverrideRequestRepository(GeneticsDbContext dbContext, ILogger<PropagationOverrideRequestRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PropagationOverrideRequest>> GetBySiteAsync(Guid siteId, PropagationOverrideStatus? status, CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT id, site_id, requested_by_user_id, mother_plant_id, batch_id,
                   requested_quantity, reason, status, requested_on,
                   approved_by_user_id, resolved_on, decision_notes
            FROM genetics.propagation_override_requests
            WHERE site_id = @SiteId";

        if (status.HasValue)
        {
            sql += " AND status = @Status";
        }

        sql += " ORDER BY requested_on DESC";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("SiteId", siteId);
        if (status.HasValue)
        {
            command.Parameters.AddWithValue("Status", status.Value.ToString());
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var overrides = new List<PropagationOverrideRequest>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            overrides.Add(Map(reader));
        }

        return overrides;
    }

    public async Task<PropagationOverrideRequest?> GetByIdAsync(Guid siteId, Guid overrideId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, requested_by_user_id, mother_plant_id, batch_id,
                   requested_quantity, reason, status, requested_on,
                   approved_by_user_id, resolved_on, decision_notes
            FROM genetics.propagation_override_requests
            WHERE site_id = @SiteId AND id = @Id";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("SiteId", siteId);
        command.Parameters.AddWithValue("Id", overrideId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return Map(reader);
    }

    public async Task<PropagationOverrideRequest> AddAsync(PropagationOverrideRequest request, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO genetics.propagation_override_requests (
                id, site_id, requested_by_user_id, mother_plant_id, batch_id,
                requested_quantity, reason, status, requested_on,
                approved_by_user_id, resolved_on, decision_notes
            ) VALUES (
                @Id, @SiteId, @RequestedByUserId, @MotherPlantId, @BatchId,
                @RequestedQuantity, @Reason, @Status, @RequestedOn,
                @ApprovedByUserId, @ResolvedOn, @DecisionNotes
            )";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(connection, request.SiteId, cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("Id", request.Id);
        command.Parameters.AddWithValue("SiteId", request.SiteId);
        command.Parameters.AddWithValue("RequestedByUserId", request.RequestedByUserId);
        command.Parameters.AddWithValue("MotherPlantId", request.MotherPlantId is null ? DBNull.Value : request.MotherPlantId);
        command.Parameters.AddWithValue("BatchId", request.BatchId is null ? DBNull.Value : request.BatchId);
        command.Parameters.AddWithValue("RequestedQuantity", request.RequestedQuantity);
        command.Parameters.AddWithValue("Reason", request.Reason);
        command.Parameters.AddWithValue("Status", request.Status.ToString());
        command.Parameters.AddWithValue("RequestedOn", request.RequestedOn);
        command.Parameters.AddWithValue("ApprovedByUserId", request.ApprovedByUserId is null ? DBNull.Value : request.ApprovedByUserId);
        command.Parameters.AddWithValue("ResolvedOn", request.ResolvedOn is null ? DBNull.Value : request.ResolvedOn);
        command.Parameters.AddWithValue("DecisionNotes", request.DecisionNotes is null ? DBNull.Value : request.DecisionNotes);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Created propagation override request {OverrideId}", request.Id);
        return request;
    }

    public async Task UpdateAsync(PropagationOverrideRequest request, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE genetics.propagation_override_requests SET
                status = @Status,
                approved_by_user_id = @ApprovedByUserId,
                resolved_on = @ResolvedOn,
                decision_notes = @DecisionNotes
            WHERE site_id = @SiteId AND id = @Id";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(connection, request.SiteId, cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("SiteId", request.SiteId);
        command.Parameters.AddWithValue("Id", request.Id);
        command.Parameters.AddWithValue("Status", request.Status.ToString());
        command.Parameters.AddWithValue("ApprovedByUserId", request.ApprovedByUserId is null ? DBNull.Value : request.ApprovedByUserId);
        command.Parameters.AddWithValue("ResolvedOn", request.ResolvedOn is null ? DBNull.Value : request.ResolvedOn);
        command.Parameters.AddWithValue("DecisionNotes", request.DecisionNotes is null ? DBNull.Value : request.DecisionNotes);

        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        
        if (affectedRows != 1)
        {
            throw new InvalidOperationException(
                $"Failed to update propagation override {request.Id} for site {request.SiteId}. " +
                $"Expected 1 row affected, but got {affectedRows}. " +
                "This may indicate the row was deleted, RLS blocked access, or the site_id was incorrect.");
        }
        
        _logger.LogInformation("Updated propagation override {OverrideId} with status {Status}", request.Id, request.Status);
    }

    private PropagationOverrideRequest Map(NpgsqlDataReader reader)
    {
        return PropagationOverrideRequest.FromPersistence(
            reader.GetGuid(reader.GetOrdinal("id")),
            reader.GetGuid(reader.GetOrdinal("site_id")),
            reader.GetGuid(reader.GetOrdinal("requested_by_user_id")),
            reader.GetInt32(reader.GetOrdinal("requested_quantity")),
            reader.GetString(reader.GetOrdinal("reason")),
            reader.IsDBNull(reader.GetOrdinal("mother_plant_id")) ? null : reader.GetGuid(reader.GetOrdinal("mother_plant_id")),
            reader.IsDBNull(reader.GetOrdinal("batch_id")) ? null : reader.GetGuid(reader.GetOrdinal("batch_id")),
            TryParseStatus(reader.GetString(reader.GetOrdinal("status"))),
            reader.GetDateTime(reader.GetOrdinal("requested_on")),
            reader.IsDBNull(reader.GetOrdinal("approved_by_user_id")) ? null : reader.GetGuid(reader.GetOrdinal("approved_by_user_id")),
            reader.IsDBNull(reader.GetOrdinal("resolved_on")) ? null : reader.GetDateTime(reader.GetOrdinal("resolved_on")),
            reader.IsDBNull(reader.GetOrdinal("decision_notes")) ? null : reader.GetString(reader.GetOrdinal("decision_notes"))
        );
    }

    private PropagationOverrideStatus TryParseStatus(string statusString)
    {
        if (Enum.TryParse<PropagationOverrideStatus>(statusString, ignoreCase: true, out var status))
        {
            return status;
        }

        _logger.LogWarning("Invalid PropagationOverrideStatus value '{Status}' encountered in database; defaulting to Pending.", statusString);
        return PropagationOverrideStatus.Pending;
    }
}
