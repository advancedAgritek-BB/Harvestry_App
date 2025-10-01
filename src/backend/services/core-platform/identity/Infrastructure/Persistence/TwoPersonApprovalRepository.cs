using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Application.DTOs;
using Harvestry.Identity.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Harvestry.Identity.Infrastructure.Persistence;

public sealed class TwoPersonApprovalRepository : ITwoPersonApprovalRepository
{
    private readonly IdentityDbContext _dbContext;
    private readonly IRlsContextAccessor _rlsContextAccessor;
    private readonly ILogger<TwoPersonApprovalRepository> _logger;

    public TwoPersonApprovalRepository(
        IdentityDbContext dbContext,
        IRlsContextAccessor rlsContextAccessor,
        ILogger<TwoPersonApprovalRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _rlsContextAccessor = rlsContextAccessor ?? throw new ArgumentNullException(nameof(rlsContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TwoPersonApprovalResponse> CreateAsync(
        TwoPersonApprovalRequest request,
        DateTime expiresAt,
        CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(request.SiteId, cancellationToken).ConfigureAwait(false);

        const string sql = @"
            INSERT INTO two_person_approvals (
                action,
                resource_type,
                resource_id,
                site_id,
                initiator_user_id,
                initiator_reason,
                initiator_attestation,
                context,
                expires_at)
            VALUES (
                @action,
                @resource_type,
                @resource_id,
                @site_id,
                @initiator_user_id,
                @initiator_reason,
                @initiator_attestation,
                @context,
                @expires_at)
            RETURNING approval_id, initiated_at, status;
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("action", NpgsqlDbType.Varchar).Value = request.Action;
        command.Parameters.Add("resource_type", NpgsqlDbType.Varchar).Value = request.ResourceType;
        command.Parameters.Add("resource_id", NpgsqlDbType.Uuid).Value = request.ResourceId;
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = request.SiteId;
        command.Parameters.Add("initiator_user_id", NpgsqlDbType.Uuid).Value = request.InitiatorUserId;
        command.Parameters.Add("initiator_reason", NpgsqlDbType.Text).Value = request.Reason;
        command.Parameters.Add("initiator_attestation", NpgsqlDbType.Text).Value = (object?)request.Attestation ?? DBNull.Value;
        var contextPayload = request.Context?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            ?? new Dictionary<string, object>();
        command.Parameters.Add("context", NpgsqlDbType.Jsonb).Value = JsonUtilities.SerializeDictionary(contextPayload);
        command.Parameters.Add("expires_at", NpgsqlDbType.TimestampTz).Value = expiresAt;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException("Failed to create two-person approval record");
        }

        var approvalId = reader.GetGuid(reader.GetOrdinal("approval_id"));
        var initiatedAt = reader.GetDateTime(reader.GetOrdinal("initiated_at"));
        var status = reader.GetString(reader.GetOrdinal("status"));

        return new TwoPersonApprovalResponse(
            approvalId,
            status,
            expiresAt,
            initiatedAt,
            request.Action,
            request.ResourceType,
            request.ResourceId,
            request.SiteId,
            request.InitiatorUserId);
    }

    public async Task<TwoPersonApprovalRecord?> GetByIdAsync(Guid approvalId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(null, cancellationToken).ConfigureAwait(false);

        const string sql = @"
            SELECT approval_id, action, resource_type, resource_id, site_id,
                   initiator_user_id, initiator_reason, initiator_attestation,
                   initiated_at, status, expires_at, approver_user_id, approved_at
            FROM two_person_approvals
            WHERE approval_id = @approval_id
            LIMIT 1;
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("approval_id", NpgsqlDbType.Uuid).Value = approvalId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return new TwoPersonApprovalRecord(
            reader.GetGuid(reader.GetOrdinal("approval_id")),
            reader.GetString(reader.GetOrdinal("action")),
            reader.GetString(reader.GetOrdinal("resource_type")),
            reader.GetGuid(reader.GetOrdinal("resource_id")),
            reader.GetGuid(reader.GetOrdinal("site_id")),
            reader.GetGuid(reader.GetOrdinal("initiator_user_id")),
            reader.GetString(reader.GetOrdinal("initiator_reason")),
            reader.IsDBNull(reader.GetOrdinal("initiator_attestation")) ? null : reader.GetString(reader.GetOrdinal("initiator_attestation")),
            reader.GetDateTime(reader.GetOrdinal("initiated_at")),
            reader.GetString(reader.GetOrdinal("status")),
            reader.GetDateTime(reader.GetOrdinal("expires_at")),
            reader.IsDBNull(reader.GetOrdinal("approver_user_id")) ? null : reader.GetGuid(reader.GetOrdinal("approver_user_id")),
            reader.IsDBNull(reader.GetOrdinal("approved_at")) ? null : reader.GetDateTime(reader.GetOrdinal("approved_at")));
    }

    public async Task<bool> ApproveAsync(
        Guid approvalId,
        Guid approverUserId,
        string reason,
        string? attestation,
        CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(null, cancellationToken).ConfigureAwait(false);

        const string sql = @"
            UPDATE two_person_approvals SET
                status = 'approved',
                approver_user_id = @approver_user_id,
                approver_reason = @approver_reason,
                approver_attestation = @approver_attestation,
                approved_at = NOW(),
                completed_at = NOW()
            WHERE approval_id = @approval_id
              AND status = 'pending'
              AND expires_at > NOW()
              AND approver_user_id IS DISTINCT FROM initiator_user_id;
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("approver_user_id", NpgsqlDbType.Uuid).Value = approverUserId;
        command.Parameters.Add("approver_reason", NpgsqlDbType.Text).Value = reason;
        command.Parameters.Add("approver_attestation", NpgsqlDbType.Text).Value = (object?)attestation ?? DBNull.Value;
        command.Parameters.Add("approval_id", NpgsqlDbType.Uuid).Value = approvalId;

        var rows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return rows > 0;
    }

    public async Task<bool> RejectAsync(
        Guid approvalId,
        Guid approverUserId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(null, cancellationToken).ConfigureAwait(false);

        const string sql = @"
            UPDATE two_person_approvals SET
                status = 'rejected',
                approver_user_id = @approver_user_id,
                approver_reason = @approver_reason,
                completed_at = NOW()
            WHERE approval_id = @approval_id
              AND status = 'pending';
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("approver_user_id", NpgsqlDbType.Uuid).Value = approverUserId;
        command.Parameters.Add("approver_reason", NpgsqlDbType.Text).Value = reason;
        command.Parameters.Add("approval_id", NpgsqlDbType.Uuid).Value = approvalId;

        var rows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return rows > 0;
    }

    public async Task<IEnumerable<TwoPersonApprovalResponse>> GetPendingAsync(
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(siteId, cancellationToken).ConfigureAwait(false);

        const string sql = @"
            SELECT approval_id, action, resource_type, resource_id, site_id,
                   initiator_user_id, status, expires_at, initiated_at
            FROM two_person_approvals
            WHERE site_id = @site_id
              AND status = 'pending'
              AND expires_at > NOW()
            ORDER BY initiated_at ASC;
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = siteId;

        var approvals = new List<TwoPersonApprovalResponse>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var approval = new TwoPersonApprovalResponse(
                reader.GetGuid(reader.GetOrdinal("approval_id")),
                reader.GetString(reader.GetOrdinal("status")),
                reader.GetDateTime(reader.GetOrdinal("expires_at")),
                reader.GetDateTime(reader.GetOrdinal("initiated_at")),
                reader.GetString(reader.GetOrdinal("action")),
                reader.GetString(reader.GetOrdinal("resource_type")),
                reader.GetGuid(reader.GetOrdinal("resource_id")),
                reader.GetGuid(reader.GetOrdinal("site_id")),
                reader.GetGuid(reader.GetOrdinal("initiator_user_id")));
            approvals.Add(approval);
        }

        return approvals;
    }

    private async Task<NpgsqlConnection> PrepareConnectionAsync(Guid? siteScope, CancellationToken cancellationToken)
    {
        var context = _rlsContextAccessor.Current;
        var userId = context.UserId == Guid.Empty ? Guid.Empty : context.UserId;
        var role = string.IsNullOrWhiteSpace(context.Role) ? "service_account" : context.Role;
        var effectiveSite = siteScope ?? context.SiteId ?? Guid.Empty;

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(userId, role, effectiveSite, cancellationToken).ConfigureAwait(false);
        return connection;
    }
}
