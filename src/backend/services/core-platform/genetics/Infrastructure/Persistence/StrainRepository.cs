using Harvestry.Genetics.Application.Interfaces;
using Harvestry.Genetics.Domain.Entities;
using Harvestry.Genetics.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using System.Text.Json;

namespace Harvestry.Genetics.Infrastructure.Persistence;

/// <summary>
/// Repository for Strain entity with RLS support
/// </summary>
public sealed class StrainRepository : IStrainRepository
{
    private const string StrainSelect = @"
SELECT
    id,
    site_id,
    genetics_id,
    phenotype_id,
    name,
    description,
    breeder,
    seed_bank,
    cultivation_notes,
    expected_harvest_window_days,
    target_environment,
    compliance_requirements,
    created_at,
    created_by_user_id,
    updated_at,
    updated_by_user_id
FROM genetics.strains";

    private readonly GeneticsDbContext _dbContext;
    private readonly IRlsContextAccessor _rlsContextAccessor;
    private readonly ILogger<StrainRepository> _logger;

    public StrainRepository(
        GeneticsDbContext dbContext,
        IRlsContextAccessor rlsContextAccessor,
        ILogger<StrainRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _rlsContextAccessor = rlsContextAccessor ?? throw new ArgumentNullException(nameof(rlsContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task AddAsync(Strain strain, CancellationToken cancellationToken = default)
    {
        if (strain == null) throw new ArgumentNullException(nameof(strain));

        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
INSERT INTO genetics.strains (
    id,
    site_id,
    genetics_id,
    phenotype_id,
    name,
    description,
    breeder,
    seed_bank,
    cultivation_notes,
    expected_harvest_window_days,
    target_environment,
    compliance_requirements,
    created_at,
    created_by_user_id,
    updated_at,
    updated_by_user_id
) VALUES (
    @id,
    @site_id,
    @genetics_id,
    @phenotype_id,
    @name,
    @description,
    @breeder,
    @seed_bank,
    @cultivation_notes,
    @expected_harvest_window_days,
    @target_environment,
    @compliance_requirements,
    @created_at,
    @created_by_user_id,
    @updated_at,
    @updated_by_user_id);";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        PopulateParameters(command, strain);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Created strain {StrainId} ({Name})", strain.Id, strain.Name);
    }

    public async Task UpdateAsync(Strain strain, CancellationToken cancellationToken = default)
    {
        if (strain == null) throw new ArgumentNullException(nameof(strain));

        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
UPDATE genetics.strains SET
    site_id = @site_id,
    genetics_id = @genetics_id,
    phenotype_id = @phenotype_id,
    name = @name,
    description = @description,
    breeder = @breeder,
    seed_bank = @seed_bank,
    cultivation_notes = @cultivation_notes,
    expected_harvest_window_days = @expected_harvest_window_days,
    target_environment = @target_environment,
    compliance_requirements = @compliance_requirements,
    updated_at = @updated_at,
    updated_by_user_id = @updated_by_user_id
WHERE id = @id;";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        PopulateParameters(command, strain);

        var rows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        if (rows == 0)
        {
            throw new InvalidOperationException($"Strain {strain.Id} not found.");
        }

        _logger.LogInformation("Updated strain {StrainId} ({Name})", strain.Id, strain.Name);
    }

    public async Task DeleteAsync(Guid strainId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM genetics.strains WHERE id = @id;";
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = strainId;

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Deleted strain {StrainId}", strainId);
    }

    public async Task<Strain?> GetByIdAsync(Guid strainId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = StrainSelect + " WHERE id = @id LIMIT 1;";
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = strainId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return MapStrain(reader);
    }

    public async Task<Strain?> GetByNameAsync(
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
        command.CommandText = StrainSelect + " WHERE site_id = @site_id AND name = @name LIMIT 1;";
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = siteId;
        command.Parameters.Add("name", NpgsqlDbType.Varchar).Value = name;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return MapStrain(reader);
    }

    public async Task<IReadOnlyList<Strain>> GetBySiteAsync(
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = StrainSelect + " WHERE site_id = @site_id ORDER BY name;";
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = siteId;

        var strains = new List<Strain>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            strains.Add(MapStrain(reader));
        }

        return strains;
    }

    public async Task<IReadOnlyList<Strain>> GetByGeneticsAsync(
        Guid geneticsId,
        CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = StrainSelect + " WHERE genetics_id = @genetics_id ORDER BY name;";
        command.Parameters.Add("genetics_id", NpgsqlDbType.Uuid).Value = geneticsId;

        var strains = new List<Strain>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            strains.Add(MapStrain(reader));
        }

        return strains;
    }

    public async Task<bool> HasDependentBatchesAsync(Guid strainId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT EXISTS(SELECT 1 FROM genetics.batches WHERE strain_id = @strain_id LIMIT 1);";
        command.Parameters.Add("strain_id", NpgsqlDbType.Uuid).Value = strainId;

        var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return result is bool exists && exists;
    }

    public async Task<bool> ExistsAsync(Guid strainId, Guid siteId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT EXISTS(SELECT 1 FROM genetics.strains WHERE id = @id AND site_id = @site_id LIMIT 1);";
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = strainId;
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = siteId;

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

    private static Strain MapStrain(NpgsqlDataReader reader)
    {
        var id = reader.GetGuid(0);
        var siteId = reader.GetGuid(1);
        var geneticsId = reader.GetGuid(2);
        var phenotypeId = reader.IsDBNull(3) ? (Guid?)null : reader.GetGuid(3);
        var name = reader.GetString(4);
        var description = reader.GetString(5);
        var breeder = reader.IsDBNull(6) ? null : reader.GetString(6);
        var seedBank = reader.IsDBNull(7) ? null : reader.GetString(7);
        var cultivationNotes = reader.IsDBNull(8) ? null : reader.GetString(8);
        var expectedHarvestWindowDays = reader.IsDBNull(9) ? (int?)null : reader.GetInt32(9);

        // Parse JSON fields for value objects
        var targetEnvironmentJson = reader.IsDBNull(10) ? null : reader.GetString(10);
        var targetEnvironment = targetEnvironmentJson != null
            ? JsonSerializer.Deserialize<TargetEnvironment>(targetEnvironmentJson)!
            : TargetEnvironment.Empty;

        var complianceRequirementsJson = reader.IsDBNull(11) ? null : reader.GetString(11);
        var complianceRequirements = complianceRequirementsJson != null
            ? JsonSerializer.Deserialize<ComplianceRequirements>(complianceRequirementsJson)!
            : ComplianceRequirements.Empty;

        var createdAt = reader.GetDateTime(12);
        var createdBy = reader.GetGuid(13);
        var updatedAt = reader.GetDateTime(14);
        var updatedBy = reader.GetGuid(15);

        return Strain.FromPersistence(
            id,
            siteId,
            geneticsId,
            phenotypeId,
            name,
            description,
            breeder,
            seedBank,
            cultivationNotes,
            expectedHarvestWindowDays,
            targetEnvironment,
            complianceRequirements,
            createdAt,
            createdBy,
            updatedAt,
            updatedBy);
    }

    private static void PopulateParameters(NpgsqlCommand command, Strain strain)
    {
        command.Parameters.Clear();
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = strain.Id;
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = strain.SiteId;
        command.Parameters.Add("genetics_id", NpgsqlDbType.Uuid).Value = strain.GeneticsId;
        command.Parameters.Add("phenotype_id", NpgsqlDbType.Uuid).Value = (object?)strain.PhenotypeId ?? DBNull.Value;
        command.Parameters.Add("name", NpgsqlDbType.Varchar).Value = strain.Name;
        command.Parameters.Add("description", NpgsqlDbType.Text).Value = strain.Description;
        command.Parameters.Add("breeder", NpgsqlDbType.Varchar).Value = (object?)strain.Breeder ?? DBNull.Value;
        command.Parameters.Add("seed_bank", NpgsqlDbType.Varchar).Value = (object?)strain.SeedBank ?? DBNull.Value;
        command.Parameters.Add("cultivation_notes", NpgsqlDbType.Text).Value = (object?)strain.CultivationNotes ?? DBNull.Value;
        command.Parameters.Add("expected_harvest_window_days", NpgsqlDbType.Integer).Value = (object?)strain.ExpectedHarvestWindowDays ?? DBNull.Value;
        command.Parameters.Add("target_environment", NpgsqlDbType.Jsonb).Value = JsonSerializer.Serialize(strain.TargetEnvironment);
        command.Parameters.Add("compliance_requirements", NpgsqlDbType.Jsonb).Value = JsonSerializer.Serialize(strain.ComplianceRequirements);
        command.Parameters.Add("created_at", NpgsqlDbType.TimestampTz).Value = strain.CreatedAt;
        command.Parameters.Add("created_by_user_id", NpgsqlDbType.Uuid).Value = strain.CreatedByUserId;
        command.Parameters.Add("updated_at", NpgsqlDbType.TimestampTz).Value = strain.UpdatedAt;
        command.Parameters.Add("updated_by_user_id", NpgsqlDbType.Uuid).Value = strain.UpdatedByUserId;
    }
}
