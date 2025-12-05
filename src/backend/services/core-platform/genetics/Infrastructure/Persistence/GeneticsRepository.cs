using Harvestry.Genetics.Application.Interfaces;
using Harvestry.Genetics.Domain.Enums;
using Harvestry.Genetics.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using System.Text.Json;

namespace Harvestry.Genetics.Infrastructure.Persistence;

/// <summary>
/// Repository for Genetics aggregate with RLS support
/// </summary>
public sealed class GeneticsRepository : IGeneticsRepository
{
    private const string GeneticsSelect = @"
SELECT
    id,
    site_id,
    name,
    description,
    genetic_type,
    thc_min_percentage,
    thc_max_percentage,
    cbd_min_percentage,
    cbd_max_percentage,
    flowering_time_days,
    yield_potential,
    growth_characteristics,
    terpene_profile,
    breeding_notes,
    created_at,
    created_by_user_id,
    updated_at,
    updated_by_user_id
FROM genetics";

    private readonly GeneticsDbContext _dbContext;
    private readonly IRlsContextAccessor _rlsContextAccessor;
    private readonly ILogger<GeneticsRepository> _logger;

    public GeneticsRepository(
        GeneticsDbContext dbContext,
        IRlsContextAccessor rlsContextAccessor,
        ILogger<GeneticsRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _rlsContextAccessor = rlsContextAccessor ?? throw new ArgumentNullException(nameof(rlsContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task AddAsync(Domain.Entities.Genetics genetics, CancellationToken cancellationToken = default)
    {
        if (genetics == null) throw new ArgumentNullException(nameof(genetics));

        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
INSERT INTO genetics (
    id,
    site_id,
    name,
    description,
    genetic_type,
    thc_min_percentage,
    thc_max_percentage,
    cbd_min_percentage,
    cbd_max_percentage,
    flowering_time_days,
    yield_potential,
    growth_characteristics,
    terpene_profile,
    breeding_notes,
    created_at,
    created_by_user_id,
    updated_at,
    updated_by_user_id
) VALUES (
    @id,
    @site_id,
    @name,
    @description,
    @genetic_type,
    @thc_min_percentage,
    @thc_max_percentage,
    @cbd_min_percentage,
    @cbd_max_percentage,
    @flowering_time_days,
    @yield_potential,
    @growth_characteristics,
    @terpene_profile,
    @breeding_notes,
    @created_at,
    @created_by_user_id,
    @updated_at,
    @updated_by_user_id);";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        PopulateParameters(command, genetics);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Created genetics {GeneticsId} ({Name})", genetics.Id, genetics.Name);
    }

    public async Task UpdateAsync(Domain.Entities.Genetics genetics, CancellationToken cancellationToken = default)
    {
        if (genetics == null) throw new ArgumentNullException(nameof(genetics));

        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
UPDATE genetics SET
    site_id = @site_id,
    name = @name,
    description = @description,
    genetic_type = @genetic_type,
    thc_min_percentage = @thc_min_percentage,
    thc_max_percentage = @thc_max_percentage,
    cbd_min_percentage = @cbd_min_percentage,
    cbd_max_percentage = @cbd_max_percentage,
    flowering_time_days = @flowering_time_days,
    yield_potential = @yield_potential,
    growth_characteristics = @growth_characteristics,
    terpene_profile = @terpene_profile,
    breeding_notes = @breeding_notes,
    updated_at = @updated_at,
    updated_by_user_id = @updated_by_user_id
WHERE id = @id;";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        PopulateParameters(command, genetics);

        var rows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        if (rows == 0)
        {
            throw new InvalidOperationException($"Genetics {genetics.Id} not found.");
        }

        _logger.LogInformation("Updated genetics {GeneticsId} ({Name})", genetics.Id, genetics.Name);
    }

    public async Task DeleteAsync(Guid geneticsId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM genetics WHERE id = @id;";
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = geneticsId;

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Deleted genetics {GeneticsId}", geneticsId);
    }

    public async Task<Domain.Entities.Genetics?> GetByIdAsync(Guid geneticsId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = GeneticsSelect + " WHERE id = @id LIMIT 1;";
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = geneticsId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return MapGenetics(reader);
    }

    public async Task<Domain.Entities.Genetics?> GetByNameAsync(
        Guid siteId,
        string name,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or whitespace", nameof(name));
        }

        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = GeneticsSelect + " WHERE site_id = @site_id AND name = @name LIMIT 1;";
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = siteId;
        command.Parameters.Add("name", NpgsqlDbType.Varchar).Value = name;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return MapGenetics(reader);
    }

    public async Task<IReadOnlyList<Domain.Entities.Genetics>> GetBySiteAsync(
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = GeneticsSelect + " WHERE site_id = @site_id ORDER BY name;";
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = siteId;

        var geneticsList = new List<Domain.Entities.Genetics>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            geneticsList.Add(MapGenetics(reader));
        }

        return geneticsList;
    }

    public async Task<bool> HasDependentStrainsAsync(Guid geneticsId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT EXISTS(SELECT 1 FROM genetics.strains WHERE genetics_id = @genetics_id LIMIT 1);";
        command.Parameters.Add("genetics_id", NpgsqlDbType.Uuid).Value = geneticsId;

        var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return result is bool exists && exists;
    }

    private async Task<NpgsqlConnection> PrepareConnectionAsync(CancellationToken cancellationToken)
    {
        var context = _rlsContextAccessor.Current;
        var role = string.IsNullOrWhiteSpace(context.Role) ? "service_account" : context.Role;
        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(context.UserId, role, context.SiteId, cancellationToken).ConfigureAwait(false);
        return connection;
    }

    private static Domain.Entities.Genetics MapGenetics(NpgsqlDataReader reader)
    {
        var id = reader.GetGuid(0);
        var siteId = reader.GetGuid(1);
        var name = reader.GetString(2);
        var description = reader.GetString(3);

        var geneticTypeString = reader.GetString(4);
        if (!Enum.TryParse<GeneticType>(geneticTypeString, ignoreCase: true, out var geneticType))
        {
            throw new InvalidOperationException($"Invalid GeneticType value '{geneticTypeString}' for genetics ID {id}");
        }

        var thcMin = reader.GetDecimal(5);
        var thcMax = reader.GetDecimal(6);
        var cbdMin = reader.GetDecimal(7);
        var cbdMax = reader.GetDecimal(8);
        var floweringTimeDays = reader.IsDBNull(9) ? (int?)null : reader.GetInt32(9);

        var yieldPotentialString = reader.IsDBNull(10) ? null : reader.GetString(10);
        YieldPotential yieldPotential = YieldPotential.Medium; // Default
        if (yieldPotentialString != null && !Enum.TryParse<YieldPotential>(yieldPotentialString, ignoreCase: true, out yieldPotential))
        {
            throw new InvalidOperationException($"Invalid YieldPotential value '{yieldPotentialString}' for genetics ID {id}");
        }

        // Parse JSON fields for value objects
        var growthCharacteristicsJson = reader.IsDBNull(11) ? null : reader.GetString(11);
        var growthCharacteristics = growthCharacteristicsJson != null
            ? JsonSerializer.Deserialize<GeneticProfile>(growthCharacteristicsJson)!
            : GeneticProfile.Empty;

        var terpeneProfileJson = reader.IsDBNull(12) ? null : reader.GetString(12);
        var terpeneProfile = terpeneProfileJson != null
            ? JsonSerializer.Deserialize<TerpeneProfile>(terpeneProfileJson)!
            : TerpeneProfile.Empty;

        var breedingNotes = reader.IsDBNull(13) ? null : reader.GetString(13);
        var createdAt = reader.GetDateTime(14);
        var createdBy = reader.GetGuid(15);
        var updatedAt = reader.GetDateTime(16);
        var updatedBy = reader.GetGuid(17);

        return Domain.Entities.Genetics.FromPersistence(
            id,
            siteId,
            name,
            description,
            geneticType,
            (thcMin, thcMax),
            (cbdMin, cbdMax),
            floweringTimeDays,
            yieldPotential,
            growthCharacteristics,
            terpeneProfile,
            createdBy,
            breedingNotes,
            createdAt,
            updatedAt,
            updatedBy);
    }

    private static void PopulateParameters(NpgsqlCommand command, Domain.Entities.Genetics genetics)
    {
        command.Parameters.Clear();
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = genetics.Id;
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = genetics.SiteId;
        command.Parameters.Add("name", NpgsqlDbType.Varchar).Value = genetics.Name;
        command.Parameters.Add("description", NpgsqlDbType.Text).Value = genetics.Description;
        command.Parameters.Add("genetic_type", NpgsqlDbType.Varchar).Value = genetics.GeneticType.ToString();
        command.Parameters.Add("thc_min_percentage", NpgsqlDbType.Numeric).Value = genetics.ThcMinPercentage;
        command.Parameters.Add("thc_max_percentage", NpgsqlDbType.Numeric).Value = genetics.ThcMaxPercentage;
        command.Parameters.Add("cbd_min_percentage", NpgsqlDbType.Numeric).Value = genetics.CbdMinPercentage;
        command.Parameters.Add("cbd_max_percentage", NpgsqlDbType.Numeric).Value = genetics.CbdMaxPercentage;
        command.Parameters.Add("flowering_time_days", NpgsqlDbType.Integer).Value = (object?)genetics.FloweringTimeDays ?? DBNull.Value;
        command.Parameters.Add("yield_potential", NpgsqlDbType.Varchar).Value = genetics.YieldPotential.ToString();
        command.Parameters.Add("growth_characteristics", NpgsqlDbType.Jsonb).Value = JsonSerializer.Serialize(genetics.GrowthCharacteristics);
        command.Parameters.Add("terpene_profile", NpgsqlDbType.Jsonb).Value = JsonSerializer.Serialize(genetics.TerpeneProfile);
        command.Parameters.Add("breeding_notes", NpgsqlDbType.Text).Value = (object?)genetics.BreedingNotes ?? DBNull.Value;
        command.Parameters.Add("created_at", NpgsqlDbType.TimestampTz).Value = genetics.CreatedAt;
        command.Parameters.Add("created_by_user_id", NpgsqlDbType.Uuid).Value = genetics.CreatedByUserId;
        command.Parameters.Add("updated_at", NpgsqlDbType.TimestampTz).Value = genetics.UpdatedAt;
        command.Parameters.Add("updated_by_user_id", NpgsqlDbType.Uuid).Value = genetics.UpdatedByUserId;
    }
}
