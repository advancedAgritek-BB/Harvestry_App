using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Spatial.Application.DTOs;
using Harvestry.Spatial.Application.Interfaces;
using Harvestry.Spatial.Domain.Entities;
using Harvestry.Spatial.Domain.Enums;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Harvestry.Spatial.Infrastructure.Persistence;

public sealed class EquipmentRepository : IEquipmentRepository
{
    private readonly SpatialDbContext _dbContext;
    private readonly IRlsContextAccessor _rlsContextAccessor;
    private readonly ILogger<EquipmentRepository> _logger;

    private const string EquipmentColumns = @"
SELECT
    id,
    site_id,
    code,
    type_code,
    core_type,
    status,
    installed_at,
    decommissioned_at,
    location_id,
    manufacturer,
    model,
    serial_number,
    firmware_version,
    ip_address,
    mac_address,
    mqtt_topic,
    device_twin_json,
    last_calibration_at,
    next_calibration_due_at,
    calibration_interval_days,
    last_heartbeat_at,
    online,
    signal_strength_dbm,
    battery_percent,
    error_count,
    uptime_seconds,
    notes,
    metadata_json,
    created_at,
    created_by_user_id,
    updated_at,
    updated_by_user_id
FROM equipment";

    public EquipmentRepository(SpatialDbContext dbContext, IRlsContextAccessor rlsContextAccessor, ILogger<EquipmentRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _rlsContextAccessor = rlsContextAccessor ?? throw new ArgumentNullException(nameof(rlsContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Equipment?> GetByIdAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = EquipmentColumns + " WHERE id = @id LIMIT 1;";
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = equipmentId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return MapEquipment(reader);
    }

    public async Task<Equipment?> GetByCodeAsync(Guid siteId, string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code cannot be null or whitespace", nameof(code));
        }

        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = EquipmentColumns + " WHERE site_id = @site_id AND code = @code LIMIT 1;";
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = siteId;
        command.Parameters.Add("code", NpgsqlDbType.Varchar).Value = code.Trim();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return MapEquipment(reader);
    }

    public async Task<EquipmentListResult> GetBySiteAsync(Guid siteId, EquipmentListQuery query, CancellationToken cancellationToken = default)
    {
        if (query == null) throw new ArgumentNullException(nameof(query));

        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        var sqlBuilder = new StringBuilder();
        sqlBuilder.AppendLine("WITH filtered AS (");
        sqlBuilder.AppendLine(EquipmentColumns);
        sqlBuilder.AppendLine("WHERE site_id = @site_id");
        sqlBuilder.AppendLine(BuildFilterClause(query));
        sqlBuilder.AppendLine("ORDER BY code");
        sqlBuilder.AppendLine("LIMIT @limit OFFSET @offset");
        sqlBuilder.AppendLine(") SELECT *, (SELECT COUNT(*) FROM equipment WHERE site_id = @site_id" + BuildFilterClause(query, true) + ") AS total_count FROM filtered;");

        await using var command = connection.CreateCommand();
        command.CommandText = sqlBuilder.ToString();
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = siteId;
        command.Parameters.Add("limit", NpgsqlDbType.Integer).Value = query.PageSize <= 0 ? 50 : Math.Min(query.PageSize, 200);
        command.Parameters.Add("offset", NpgsqlDbType.Integer).Value = Math.Max(0, (query.Page - 1) * Math.Max(query.PageSize, 1));

        AddFilterParameters(command, query);

        var items = new List<Equipment>();
        var total = 0;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            items.Add(MapEquipment(reader));
            if (total == 0 && !reader.IsDBNull(reader.FieldCount - 1))
            {
                total = reader.GetInt32(reader.FieldCount - 1);
            }
        }

        return new EquipmentListResult(items, total);
    }

    public async Task<Guid> InsertAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        if (equipment == null) throw new ArgumentNullException(nameof(equipment));

        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
INSERT INTO equipment (
    id,
    site_id,
    code,
    type_code,
    core_type,
    status,
    installed_at,
    decommissioned_at,
    location_id,
    manufacturer,
    model,
    serial_number,
    firmware_version,
    ip_address,
    mac_address,
    mqtt_topic,
    device_twin_json,
    last_calibration_at,
    next_calibration_due_at,
    calibration_interval_days,
    last_heartbeat_at,
    online,
    signal_strength_dbm,
    battery_percent,
    error_count,
    uptime_seconds,
    notes,
    metadata_json,
    created_at,
    created_by_user_id,
    updated_at,
    updated_by_user_id
) VALUES (
    @id,
    @site_id,
    @code,
    @type_code,
    @core_type,
    @status,
    @installed_at,
    @decommissioned_at,
    @location_id,
    @manufacturer,
    @model,
    @serial_number,
    @firmware_version,
    @ip_address,
    @mac_address,
    @mqtt_topic,
    @device_twin_json,
    @last_calibration_at,
    @next_calibration_due_at,
    @calibration_interval_days,
    @last_heartbeat_at,
    @online,
    @signal_strength_dbm,
    @battery_percent,
    @error_count,
    @uptime_seconds,
    @notes,
    @metadata_json,
    @created_at,
    @created_by_user_id,
    @updated_at,
    @updated_by_user_id);
";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        PopulateParameters(command, equipment);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return equipment.Id;
    }

    public async Task UpdateAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        if (equipment == null) throw new ArgumentNullException(nameof(equipment));

        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
UPDATE equipment SET
    site_id = @site_id,
    code = @code,
    type_code = @type_code,
    core_type = @core_type,
    status = @status,
    installed_at = @installed_at,
    decommissioned_at = @decommissioned_at,
    location_id = @location_id,
    manufacturer = @manufacturer,
    model = @model,
    serial_number = @serial_number,
    firmware_version = @firmware_version,
    ip_address = @ip_address,
    mac_address = @mac_address,
    mqtt_topic = @mqtt_topic,
    device_twin_json = @device_twin_json,
    last_calibration_at = @last_calibration_at,
    next_calibration_due_at = @next_calibration_due_at,
    calibration_interval_days = @calibration_interval_days,
    last_heartbeat_at = @last_heartbeat_at,
    online = @online,
    signal_strength_dbm = @signal_strength_dbm,
    battery_percent = @battery_percent,
    error_count = @error_count,
    uptime_seconds = @uptime_seconds,
    notes = @notes,
    metadata_json = @metadata_json,
    created_at = @created_at,
    created_by_user_id = @created_by_user_id,
    updated_at = @updated_at,
    updated_by_user_id = @updated_by_user_id
WHERE id = @id;
";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        PopulateParameters(command, equipment);

        var rows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        if (rows == 0)
        {
            throw new InvalidOperationException($"Equipment {equipment.Id} not found.");
        }
    }

    private async Task<NpgsqlConnection> PrepareConnectionAsync(CancellationToken cancellationToken)
    {
        var context = _rlsContextAccessor.Current;
        var role = string.IsNullOrWhiteSpace(context.Role) ? "service_account" : context.Role;
        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(context.UserId, role, context.SiteId, cancellationToken).ConfigureAwait(false);
        return connection;
    }

    private static Equipment MapEquipment(NpgsqlDataReader reader)
    {
        var equipment = Equipment.FromPersistence(
            reader.GetGuid(reader.GetOrdinal("id")),
            reader.GetGuid(reader.GetOrdinal("site_id")),
            reader.GetString(reader.GetOrdinal("code")),
            reader.GetString(reader.GetOrdinal("type_code")),
            Enum.Parse<CoreEquipmentType>(reader.GetString(reader.GetOrdinal("core_type")), true),
            Enum.Parse<EquipmentStatus>(reader.GetString(reader.GetOrdinal("status")), true),
            reader.IsDBNull(reader.GetOrdinal("location_id")) ? null : reader.GetGuid(reader.GetOrdinal("location_id")),
            reader.IsDBNull(reader.GetOrdinal("installed_at")) ? null : reader.GetDateTime(reader.GetOrdinal("installed_at")),
            reader.IsDBNull(reader.GetOrdinal("decommissioned_at")) ? null : reader.GetDateTime(reader.GetOrdinal("decommissioned_at")),
            reader.IsDBNull(reader.GetOrdinal("manufacturer")) ? null : reader.GetString(reader.GetOrdinal("manufacturer")),
            reader.IsDBNull(reader.GetOrdinal("model")) ? null : reader.GetString(reader.GetOrdinal("model")),
            reader.IsDBNull(reader.GetOrdinal("serial_number")) ? null : reader.GetString(reader.GetOrdinal("serial_number")),
            reader.IsDBNull(reader.GetOrdinal("firmware_version")) ? null : reader.GetString(reader.GetOrdinal("firmware_version")),
            reader.IsDBNull(reader.GetOrdinal("ip_address"))
                ? null
                : reader.GetValue(reader.GetOrdinal("ip_address")) switch
                {
                    NpgsqlInet inet when inet.Address is not null => inet.Address.ToString(),
                    NpgsqlInet inet => inet.ToString(),
                    IPAddress address => address.ToString(),
                    string address => address,
                    var value => throw new InvalidOperationException($"Unsupported inet value type {value.GetType()} for equipment record." )
                },
            reader.IsDBNull(reader.GetOrdinal("mac_address")) 
                ? null 
                : reader.GetValue(reader.GetOrdinal("mac_address")) switch
                {
                    System.Net.NetworkInformation.PhysicalAddress physAddr => string.Join(":", BitConverter.ToString(physAddr.GetAddressBytes()).Split('-')),
                    string macStr => macStr,
                    var value => throw new InvalidOperationException($"Unsupported macaddr value type {value.GetType()} for equipment record.")
                },
            reader.IsDBNull(reader.GetOrdinal("mqtt_topic")) ? null : reader.GetString(reader.GetOrdinal("mqtt_topic")),
            reader.IsDBNull(reader.GetOrdinal("device_twin_json")) ? null : reader.GetString(reader.GetOrdinal("device_twin_json")),
            reader.IsDBNull(reader.GetOrdinal("last_calibration_at")) ? null : reader.GetDateTime(reader.GetOrdinal("last_calibration_at")),
            reader.IsDBNull(reader.GetOrdinal("next_calibration_due_at")) ? null : reader.GetDateTime(reader.GetOrdinal("next_calibration_due_at")),
            reader.IsDBNull(reader.GetOrdinal("calibration_interval_days")) ? null : reader.GetInt32(reader.GetOrdinal("calibration_interval_days")),
            reader.IsDBNull(reader.GetOrdinal("last_heartbeat_at")) ? null : reader.GetDateTime(reader.GetOrdinal("last_heartbeat_at")),
            reader.IsDBNull(reader.GetOrdinal("signal_strength_dbm")) ? null : reader.GetInt32(reader.GetOrdinal("signal_strength_dbm")),
            reader.IsDBNull(reader.GetOrdinal("battery_percent")) ? null : reader.GetInt32(reader.GetOrdinal("battery_percent")),
            reader.GetInt32(reader.GetOrdinal("error_count")),
            reader.IsDBNull(reader.GetOrdinal("uptime_seconds")) ? null : reader.GetInt64(reader.GetOrdinal("uptime_seconds")),
            reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString(reader.GetOrdinal("notes")),
            reader.IsDBNull(reader.GetOrdinal("metadata_json")) ? null : reader.GetString(reader.GetOrdinal("metadata_json")),
            reader.GetDateTime(reader.GetOrdinal("created_at")),
            reader.GetGuid(reader.GetOrdinal("created_by_user_id")),
            reader.GetDateTime(reader.GetOrdinal("updated_at")),
            reader.GetGuid(reader.GetOrdinal("updated_by_user_id"))
        );

        return equipment;
    }

    private static void PopulateParameters(NpgsqlCommand command, Equipment equipment)
    {
        command.Parameters.Clear();
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = equipment.Id;
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = equipment.SiteId;
        command.Parameters.Add("code", NpgsqlDbType.Varchar).Value = equipment.Code;
        command.Parameters.Add("type_code", NpgsqlDbType.Varchar).Value = equipment.TypeCode;
        command.Parameters.Add("core_type", NpgsqlDbType.Unknown).Value = equipment.CoreType.ToString().ToLowerInvariant();
        command.Parameters.Add("status", NpgsqlDbType.Unknown).Value = equipment.Status.ToString();
        command.Parameters.Add("installed_at", NpgsqlDbType.TimestampTz).Value = (object?)equipment.InstalledAt ?? DBNull.Value;
        command.Parameters.Add("decommissioned_at", NpgsqlDbType.TimestampTz).Value = (object?)equipment.DecommissionedAt ?? DBNull.Value;
        command.Parameters.Add("location_id", NpgsqlDbType.Uuid).Value = (object?)equipment.LocationId ?? DBNull.Value;
        command.Parameters.Add("manufacturer", NpgsqlDbType.Varchar).Value = (object?)equipment.Manufacturer ?? DBNull.Value;
        command.Parameters.Add("model", NpgsqlDbType.Varchar).Value = (object?)equipment.Model ?? DBNull.Value;
        command.Parameters.Add("serial_number", NpgsqlDbType.Varchar).Value = (object?)equipment.SerialNumber ?? DBNull.Value;
        command.Parameters.Add("firmware_version", NpgsqlDbType.Varchar).Value = (object?)equipment.FirmwareVersion ?? DBNull.Value;
        var ipParameter = command.Parameters.Add("ip_address", NpgsqlDbType.Inet);
        if (string.IsNullOrWhiteSpace(equipment.IpAddress))
        {
            ipParameter.Value = DBNull.Value;
        }
        else if (IPAddress.TryParse(equipment.IpAddress, out var parsedIp))
        {
            ipParameter.Value = parsedIp;
        }
        else
        {
            throw new InvalidOperationException($"Invalid IP address '{equipment.IpAddress}' for equipment {equipment.Code}.");
        }
        
        var macParameter = command.Parameters.Add("mac_address", NpgsqlDbType.MacAddr);
        if (string.IsNullOrWhiteSpace(equipment.MacAddress))
        {
            macParameter.Value = DBNull.Value;
        }
        else
        {
            try
            {
                var cleanMac = equipment.MacAddress.Replace(":", "").Replace("-", "");
                macParameter.Value = System.Net.NetworkInformation.PhysicalAddress.Parse(cleanMac);
            }
            catch (Exception ex) when (ex is FormatException || ex is ArgumentException)
            {
                throw new InvalidOperationException($"Invalid MAC address '{equipment.MacAddress}' for equipment {equipment.Code}.", ex);
            }
        }
        command.Parameters.Add("mqtt_topic", NpgsqlDbType.Varchar).Value = (object?)equipment.MqttTopic ?? DBNull.Value;
        command.Parameters.Add("device_twin_json", NpgsqlDbType.Jsonb).Value = (object?)equipment.DeviceTwinJson ?? DBNull.Value;
        command.Parameters.Add("last_calibration_at", NpgsqlDbType.TimestampTz).Value = (object?)equipment.LastCalibrationAt ?? DBNull.Value;
        command.Parameters.Add("next_calibration_due_at", NpgsqlDbType.TimestampTz).Value = (object?)equipment.NextCalibrationDueAt ?? DBNull.Value;
        command.Parameters.Add("calibration_interval_days", NpgsqlDbType.Integer).Value = (object?)equipment.CalibrationIntervalDays ?? DBNull.Value;
        command.Parameters.Add("last_heartbeat_at", NpgsqlDbType.TimestampTz).Value = (object?)equipment.LastHeartbeatAt ?? DBNull.Value;
        command.Parameters.Add("online", NpgsqlDbType.Boolean).Value = equipment.IsOnline();
        command.Parameters.Add("signal_strength_dbm", NpgsqlDbType.Integer).Value = (object?)equipment.SignalStrengthDbm ?? DBNull.Value;
        command.Parameters.Add("battery_percent", NpgsqlDbType.Integer).Value = (object?)equipment.BatteryPercent ?? DBNull.Value;
        command.Parameters.Add("error_count", NpgsqlDbType.Integer).Value = equipment.ErrorCount;
        command.Parameters.Add("uptime_seconds", NpgsqlDbType.Bigint).Value = (object?)equipment.UptimeSeconds ?? DBNull.Value;
        command.Parameters.Add("notes", NpgsqlDbType.Text).Value = (object?)equipment.Notes ?? DBNull.Value;
        command.Parameters.Add("metadata_json", NpgsqlDbType.Jsonb).Value = (object?)equipment.MetadataJson ?? DBNull.Value;
        command.Parameters.Add("created_at", NpgsqlDbType.TimestampTz).Value = equipment.CreatedAt;
        command.Parameters.Add("created_by_user_id", NpgsqlDbType.Uuid).Value = equipment.CreatedByUserId;
        command.Parameters.Add("updated_at", NpgsqlDbType.TimestampTz).Value = equipment.UpdatedAt;
        command.Parameters.Add("updated_by_user_id", NpgsqlDbType.Uuid).Value = equipment.UpdatedByUserId;
    }

    private static string BuildFilterClause(EquipmentListQuery query, bool forCount = false)
    {
        var builder = new StringBuilder();

        if (query.Status.HasValue)
        {
            builder.AppendLine(" AND status = @status");
        }

        if (query.CoreType.HasValue)
        {
            builder.AppendLine(" AND core_type = @core_type");
        }

        if (query.LocationId.HasValue)
        {
            builder.AppendLine(" AND location_id = @location_id");
        }

        if (query.CalibrationDueBefore.HasValue)
        {
            builder.AppendLine(" AND next_calibration_due_at IS NOT NULL AND next_calibration_due_at <= @calibration_due_before");
        }

        return builder.ToString();
    }

    private static void AddFilterParameters(NpgsqlCommand command, EquipmentListQuery query)
    {
        if (query.Status.HasValue)
        {
            command.Parameters.Add("status", NpgsqlDbType.Unknown).Value = query.Status.Value.ToString();
        }

        if (query.CoreType.HasValue)
        {
            command.Parameters.Add("core_type", NpgsqlDbType.Unknown).Value = query.CoreType.Value.ToString().ToLowerInvariant();
        }

        if (query.LocationId.HasValue)
        {
            command.Parameters.Add("location_id", NpgsqlDbType.Uuid).Value = query.LocationId.Value;
        }

        if (query.CalibrationDueBefore.HasValue)
        {
            command.Parameters.Add("calibration_due_before", NpgsqlDbType.TimestampTz).Value = query.CalibrationDueBefore.Value;
        }
    }
}
