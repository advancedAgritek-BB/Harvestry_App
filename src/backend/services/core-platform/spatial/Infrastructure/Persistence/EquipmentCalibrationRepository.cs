using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Spatial.Application.Interfaces;
using Harvestry.Spatial.Domain.Entities;
using Harvestry.Spatial.Domain.Enums;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Harvestry.Spatial.Infrastructure.Persistence;

public sealed class EquipmentCalibrationRepository : ICalibrationRepository
{
    private readonly SpatialDbContext _dbContext;
    private readonly IRlsContextAccessor _rlsContextAccessor;
    private readonly ILogger<EquipmentCalibrationRepository> _logger;

    private const string BaseColumns = @"
SELECT
    c.id,
    c.equipment_id,
    c.channel_code,
    c.method,
    c.reference_value,
    c.measured_value,
    c.result,
    c.deviation,
    c.deviation_pct,
    c.performed_at,
    c.performed_by_user_id,
    c.next_due_at,
    c.notes,
    c.attachment_url,
    c.coefficients_json,
    c.firmware_version_at_calibration
FROM equipment_calibrations c";

    public EquipmentCalibrationRepository(
        SpatialDbContext dbContext,
        IRlsContextAccessor rlsContextAccessor,
        ILogger<EquipmentCalibrationRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _rlsContextAccessor = rlsContextAccessor ?? throw new ArgumentNullException(nameof(rlsContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Guid> InsertAsync(Calibration calibration, CancellationToken cancellationToken = default)
    {
        if (calibration == null) throw new ArgumentNullException(nameof(calibration));

        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
INSERT INTO equipment_calibrations (
    id,
    equipment_id,
    channel_code,
    method,
    reference_value,
    measured_value,
    coefficients_json,
    result,
    deviation,
    deviation_pct,
    performed_at,
    performed_by_user_id,
    next_due_at,
    notes,
    attachment_url,
    firmware_version_at_calibration
) VALUES (
    @id,
    @equipment_id,
    @channel_code,
    @method,
    @reference_value,
    @measured_value,
    @coefficients_json,
    @result,
    @deviation,
    @deviation_pct,
    @performed_at,
    @performed_by_user_id,
    @next_due_at,
    @notes,
    @attachment_url,
    @firmware_version_at_calibration
);
";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = calibration.Id;
        command.Parameters.Add("equipment_id", NpgsqlDbType.Uuid).Value = calibration.EquipmentId;
        command.Parameters.Add("channel_code", NpgsqlDbType.Varchar).Value = (object?)calibration.ChannelCode ?? DBNull.Value;
        command.Parameters.Add("method", NpgsqlDbType.Text).Value = calibration.Method.ToString();
        command.Parameters.Add("reference_value", NpgsqlDbType.Numeric).Value = calibration.ReferenceValue;
        command.Parameters.Add("measured_value", NpgsqlDbType.Numeric).Value = calibration.MeasuredValue;
        command.Parameters.Add("coefficients_json", NpgsqlDbType.Jsonb).Value = (object?)calibration.CoefficientsJson ?? DBNull.Value;
        command.Parameters.Add("result", NpgsqlDbType.Text).Value = calibration.Result.ToString();
        command.Parameters.Add("deviation", NpgsqlDbType.Numeric).Value = calibration.Deviation;
        command.Parameters.Add("deviation_pct", NpgsqlDbType.Numeric).Value = calibration.DeviationPct;
        command.Parameters.Add("performed_at", NpgsqlDbType.TimestampTz).Value = calibration.PerformedAt;
        command.Parameters.Add("performed_by_user_id", NpgsqlDbType.Uuid).Value = calibration.PerformedByUserId;
        command.Parameters.Add("next_due_at", NpgsqlDbType.TimestampTz).Value = (object?)calibration.NextDueAt ?? DBNull.Value;
        command.Parameters.Add("notes", NpgsqlDbType.Text).Value = (object?)calibration.Notes ?? DBNull.Value;
        command.Parameters.Add("attachment_url", NpgsqlDbType.Text).Value = (object?)calibration.AttachmentUrl ?? DBNull.Value;
        command.Parameters.Add("firmware_version_at_calibration", NpgsqlDbType.Varchar).Value = (object?)calibration.FirmwareVersionAtCalibration ?? DBNull.Value;

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return calibration.Id;
    }

    public async Task<IReadOnlyList<Calibration>> GetByEquipmentIdAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = BaseColumns + " WHERE c.equipment_id = @equipment_id ORDER BY c.performed_at DESC";
        command.Parameters.Add("equipment_id", NpgsqlDbType.Uuid).Value = equipmentId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var results = new List<Calibration>();

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(MapCalibration(reader));
        }

        return results;
    }

    public async Task<Calibration?> GetLatestByEquipmentIdAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = BaseColumns + " WHERE c.equipment_id = @equipment_id ORDER BY c.performed_at DESC LIMIT 1";
        command.Parameters.Add("equipment_id", NpgsqlDbType.Uuid).Value = equipmentId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return MapCalibration(reader);
    }

    public async Task<IReadOnlyList<Calibration>> GetOverdueAsync(Guid siteId, DateTime dueBeforeUtc, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        const string sql = BaseColumns + @"
JOIN equipment e ON e.id = c.equipment_id
WHERE e.site_id = @site_id
  AND c.next_due_at IS NOT NULL
  AND c.next_due_at <= @due_before
ORDER BY c.next_due_at ASC;";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = siteId;
        command.Parameters.Add("due_before", NpgsqlDbType.TimestampTz).Value = dueBeforeUtc;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var results = new List<Calibration>();

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(MapCalibration(reader));
        }

        return results;
    }

    private async Task<NpgsqlConnection> PrepareConnectionAsync(CancellationToken cancellationToken)
    {
        var context = _rlsContextAccessor.Current;
        
        if (context == null)
        {
            throw new InvalidOperationException("RLS context is not available. Ensure the RLS middleware has been invoked.");
        }

        if (context.UserId == Guid.Empty)
        {
            throw new InvalidOperationException("RLS context UserId cannot be empty. A valid user ID is required for RLS enforcement.");
        }

        if (context.SiteId == Guid.Empty)
        {
            throw new InvalidOperationException("RLS context SiteId cannot be empty. A valid site ID is required for RLS enforcement.");
        }

        var role = string.IsNullOrWhiteSpace(context.Role) ? "service_account" : context.Role;

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(context.UserId, role, context.SiteId, cancellationToken).ConfigureAwait(false);

        return connection;
    }

    private static Calibration MapCalibration(NpgsqlDataReader reader)
    {
        var calibrationId = reader.GetGuid(reader.GetOrdinal("id"));
        var methodString = reader.GetString(reader.GetOrdinal("method"));
        var resultString = reader.GetString(reader.GetOrdinal("result"));

        if (!Enum.TryParse<CalibrationMethod>(methodString, true, out var method))
        {
            throw new InvalidOperationException($"Invalid CalibrationMethod '{methodString}' found in database for calibration {calibrationId}.");
        }

        if (!Enum.TryParse<CalibrationResult>(resultString, true, out var result))
        {
            throw new InvalidOperationException($"Invalid CalibrationResult '{resultString}' found in database for calibration {calibrationId}.");
        }

        return Calibration.FromPersistence(
            calibrationId,
            reader.GetGuid(reader.GetOrdinal("equipment_id")),
            reader.IsDBNull(reader.GetOrdinal("channel_code")) ? null : reader.GetString(reader.GetOrdinal("channel_code")),
            method,
            reader.GetDecimal(reader.GetOrdinal("reference_value")),
            reader.GetDecimal(reader.GetOrdinal("measured_value")),
            result,
            reader.GetDecimal(reader.GetOrdinal("deviation")),
            reader.GetDecimal(reader.GetOrdinal("deviation_pct")),
            reader.GetDateTime(reader.GetOrdinal("performed_at")),
            reader.GetGuid(reader.GetOrdinal("performed_by_user_id")),
            reader.IsDBNull(reader.GetOrdinal("next_due_at")) ? null : reader.GetDateTime(reader.GetOrdinal("next_due_at")),
            reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString(reader.GetOrdinal("notes")),
            reader.IsDBNull(reader.GetOrdinal("attachment_url")) ? null : reader.GetString(reader.GetOrdinal("attachment_url")),
            reader.IsDBNull(reader.GetOrdinal("coefficients_json")) ? null : reader.GetString(reader.GetOrdinal("coefficients_json")),
            reader.IsDBNull(reader.GetOrdinal("firmware_version_at_calibration")) ? null : reader.GetString(reader.GetOrdinal("firmware_version_at_calibration"))
        );
    }
}
