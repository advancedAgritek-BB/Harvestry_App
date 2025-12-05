using Harvestry.Genetics.Application.Interfaces;
using Harvestry.Genetics.Domain.Entities;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using System.Text.Json;

namespace Harvestry.Genetics.Infrastructure.Persistence;

/// <summary>
/// Repository for Phenotype entity with RLS support
/// </summary>
public sealed class PhenotypeRepository : IPhenotypeRepository
{
    private const string PhenotypeSelect = @"
SELECT
    id,
    site_id,
    genetics_id,
    name,
    description,
    expression_notes,
    visual_characteristics,
    aroma_profile,
    growth_pattern,
    created_at,
    created_by_user_id,
    updated_at,
    updated_by_user_id
FROM phenotypes";

    private readonly GeneticsDbContext _dbContext;
    private readonly IRlsContextAccessor _rlsContextAccessor;
    private readonly ILogger<PhenotypeRepository> _logger;

    public PhenotypeRepository(
        GeneticsDbContext dbContext,
        IRlsContextAccessor rlsContextAccessor,
        ILogger<PhenotypeRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _rlsContextAccessor = rlsContextAccessor ?? throw new ArgumentNullException(nameof(rlsContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task AddAsync(Phenotype phenotype, CancellationToken cancellationToken = default)
    {
        if (phenotype == null) throw new ArgumentNullException(nameof(phenotype));

        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
INSERT INTO phenotypes (
    id,
    site_id,
    genetics_id,
    name,
    description,
    expression_notes,
    visual_characteristics,
    aroma_profile,
    growth_pattern,
    created_at,
    created_by_user_id,
    updated_at,
    updated_by_user_id
) VALUES (
    @id,
    @site_id,
    @genetics_id,
    @name,
    @description,
    @expression_notes,
    @visual_characteristics,
    @aroma_profile,
    @growth_pattern,
    @created_at,
    @created_by_user_id,
    @updated_at,
    @updated_by_user_id);";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        PopulateParameters(command, phenotype);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Created phenotype {PhenotypeId} ({Name})", phenotype.Id, phenotype.Name);
    }

    public async Task UpdateAsync(Phenotype phenotype, CancellationToken cancellationToken = default)
    {
        if (phenotype == null) throw new ArgumentNullException(nameof(phenotype));

        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
UPDATE phenotypes SET
    site_id = @site_id,
    genetics_id = @genetics_id,
    name = @name,
    description = @description,
    expression_notes = @expression_notes,
    visual_characteristics = @visual_characteristics,
    aroma_profile = @aroma_profile,
    growth_pattern = @growth_pattern,
    updated_at = @updated_at,
    updated_by_user_id = @updated_by_user_id
WHERE id = @id;";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        PopulateParameters(command, phenotype);

        var rows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        if (rows == 0)
        {
            throw new InvalidOperationException($"Phenotype {phenotype.Id} not found.");
        }

        _logger.LogInformation("Updated phenotype {PhenotypeId} ({Name})", phenotype.Id, phenotype.Name);
    }

    public async Task DeleteAsync(Guid phenotypeId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM phenotypes WHERE id = @id;";
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = phenotypeId;

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Deleted phenotype {PhenotypeId}", phenotypeId);
    }

    public async Task<Phenotype?> GetByIdAsync(Guid phenotypeId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = PhenotypeSelect + " WHERE id = @id LIMIT 1;";
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = phenotypeId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return MapPhenotype(reader);
    }

    public async Task<Phenotype?> GetByNameAsync(
        Guid siteId,
        Guid geneticsId,
        string name,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or whitespace", nameof(name));
        }

        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = PhenotypeSelect + " WHERE site_id = @site_id AND genetics_id = @genetics_id AND name = @name LIMIT 1;";
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = siteId;
        command.Parameters.Add("genetics_id", NpgsqlDbType.Uuid).Value = geneticsId;
        command.Parameters.Add("name", NpgsqlDbType.Varchar).Value = name;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return MapPhenotype(reader);
    }

    public async Task<IReadOnlyList<Phenotype>> GetByGeneticsAsync(
        Guid geneticsId,
        CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = PhenotypeSelect + " WHERE genetics_id = @genetics_id ORDER BY name;";
        command.Parameters.Add("genetics_id", NpgsqlDbType.Uuid).Value = geneticsId;

        var phenotypes = new List<Phenotype>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            phenotypes.Add(MapPhenotype(reader));
        }

        return phenotypes;
    }

    private async Task<NpgsqlConnection> PrepareConnectionAsync(CancellationToken cancellationToken)
    {
        var context = _rlsContextAccessor.Current;
        var role = string.IsNullOrWhiteSpace(context.Role) ? "service_account" : context.Role;
        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(context.UserId, role, context.SiteId, cancellationToken).ConfigureAwait(false);
        return connection;
    }

    private static Phenotype MapPhenotype(NpgsqlDataReader reader)
    {
        var id = reader.GetGuid(0);
        var siteId = reader.GetGuid(1);
        var geneticsId = reader.GetGuid(2);
        var name = reader.GetString(3);
        var description = reader.GetString(4);
        var expressionNotes = reader.IsDBNull(5) ? null : reader.GetString(5);

        // Parse JSON fields
        var visualCharacteristicsJson = reader.IsDBNull(6) ? null : reader.GetString(6);
        var visualCharacteristics = visualCharacteristicsJson != null
            ? JsonSerializer.Deserialize<Dictionary<string, object>>(visualCharacteristicsJson)!
            : new Dictionary<string, object>();

        var aromaProfileJson = reader.IsDBNull(7) ? null : reader.GetString(7);
        var aromaProfile = aromaProfileJson != null
            ? JsonSerializer.Deserialize<Dictionary<string, object>>(aromaProfileJson)!
            : new Dictionary<string, object>();

        var growthPatternJson = reader.IsDBNull(8) ? null : reader.GetString(8);
        var growthPattern = growthPatternJson != null
            ? JsonSerializer.Deserialize<Dictionary<string, object>>(growthPatternJson)!
            : new Dictionary<string, object>();

        var createdAt = reader.GetDateTime(9);
        var createdBy = reader.GetGuid(10);
        var updatedAt = reader.GetDateTime(11);
        var updatedBy = reader.GetGuid(12);

        return Phenotype.FromPersistence(
            id,
            siteId,
            geneticsId,
            name,
            description,
            expressionNotes,
            visualCharacteristics,
            aromaProfile,
            growthPattern,
            createdAt,
            createdBy,
            updatedAt,
            updatedBy);
    }

    private static void PopulateParameters(NpgsqlCommand command, Phenotype phenotype)
    {
        command.Parameters.Clear();
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = phenotype.Id;
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = phenotype.SiteId;
        command.Parameters.Add("genetics_id", NpgsqlDbType.Uuid).Value = phenotype.GeneticsId;
        command.Parameters.Add("name", NpgsqlDbType.Varchar).Value = phenotype.Name;
        command.Parameters.Add("description", NpgsqlDbType.Text).Value = phenotype.Description;
        command.Parameters.Add("expression_notes", NpgsqlDbType.Text).Value = (object?)phenotype.ExpressionNotes ?? DBNull.Value;
        command.Parameters.Add("visual_characteristics", NpgsqlDbType.Jsonb).Value = JsonSerializer.Serialize(phenotype.VisualCharacteristics);
        command.Parameters.Add("aroma_profile", NpgsqlDbType.Jsonb).Value = JsonSerializer.Serialize(phenotype.AromaProfile);
        command.Parameters.Add("growth_pattern", NpgsqlDbType.Jsonb).Value = JsonSerializer.Serialize(phenotype.GrowthPattern);
        command.Parameters.Add("created_at", NpgsqlDbType.TimestampTz).Value = phenotype.CreatedAt;
        command.Parameters.Add("created_by_user_id", NpgsqlDbType.Uuid).Value = phenotype.CreatedByUserId;
        command.Parameters.Add("updated_at", NpgsqlDbType.TimestampTz).Value = phenotype.UpdatedAt;
        command.Parameters.Add("updated_by_user_id", NpgsqlDbType.Uuid).Value = phenotype.UpdatedByUserId;
    }
}

