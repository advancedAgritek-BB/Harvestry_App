using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Domain.Entities;
using System.Text.Json;

namespace Harvestry.Genetics.Application.Mappers;

/// <summary>
/// Mapper for Batch entity and DTOs
/// </summary>
public static class BatchMapper
{
    /// <summary>
    /// Map Batch entity to response DTO
    /// </summary>
    public static BatchResponse ToResponse(Batch batch)
    {
        return new BatchResponse(
            Id: batch.Id,
            SiteId: batch.SiteId,
            StrainId: batch.StrainId,
            BatchCode: batch.BatchCode.Value,
            BatchName: batch.BatchName,
            BatchType: batch.BatchType,
            SourceType: batch.SourceType,
            ParentBatchId: batch.ParentBatchId,
            Generation: batch.Generation,
            PlantCount: batch.PlantCount,
            TargetPlantCount: batch.TargetPlantCount,
            CurrentStageId: batch.CurrentStageId,
            StageStartedAt: batch.StageStartedAt,
            ExpectedHarvestDate: batch.ExpectedHarvestDate,
            ActualHarvestDate: batch.ActualHarvestDate,
            LocationId: batch.LocationId,
            RoomId: batch.RoomId,
            ZoneId: batch.ZoneId,
            Status: batch.Status,
            Notes: batch.Notes,
            Metadata: batch.Metadata,
            CreatedAt: batch.CreatedAt,
            UpdatedAt: batch.UpdatedAt,
            CreatedByUserId: batch.CreatedByUserId,
            UpdatedByUserId: batch.UpdatedByUserId
        );
    }

    /// <summary>
    /// Map list of Batch entities to response DTOs
    /// </summary>
    public static IReadOnlyList<BatchResponse> ToResponseList(IEnumerable<Batch> batches)
    {
        return batches.Select(ToResponse).ToList();
    }

    /// <summary>
    /// Map BatchEvent entity to response DTO
    /// </summary>
    public static BatchEventResponse ToEventResponse(BatchEvent batchEvent)
    {
        var normalizedData = NormalizeEventData(batchEvent.EventData);

        return new BatchEventResponse(
            Id: batchEvent.Id,
            SiteId: batchEvent.SiteId,
            BatchId: batchEvent.BatchId,
            EventType: batchEvent.EventType,
            OccurredAt: batchEvent.PerformedAt,
            FromStageId: GetGuid(normalizedData, "fromStageId"),
            ToStageId: GetGuid(normalizedData, "toStageId"),
            PreviousPlantCount: GetInt(normalizedData, "oldCount"),
            NewPlantCount: GetInt(normalizedData, "newCount"),
            RelatedBatchId: GetGuid(normalizedData, "relatedBatchId"),
            Notes: batchEvent.Notes,
            EventData: normalizedData,
            PerformedByUserId: batchEvent.PerformedByUserId
        );
    }

    /// <summary>
    /// Map list of BatchEvent entities to response DTOs
    /// </summary>
    public static IReadOnlyList<BatchEventResponse> ToEventResponseList(IEnumerable<BatchEvent> events)
    {
        return events.Select(ToEventResponse).ToList();
    }

    /// <summary>
    /// Map BatchRelationship entity to response DTO
    /// </summary>
    public static BatchRelationshipResponse ToRelationshipResponse(BatchRelationship relationship)
    {
        return new BatchRelationshipResponse(
            Id: relationship.Id,
            SiteId: relationship.SiteId,
            ParentBatchId: relationship.ParentBatchId,
            ChildBatchId: relationship.ChildBatchId,
            RelationshipType: relationship.RelationshipType,
            PlantCountTransferred: relationship.PlantCountTransferred,
            TransferDate: relationship.TransferDate,
            CreatedAt: relationship.CreatedAt,
            CreatedByUserId: relationship.CreatedByUserId,
            Notes: relationship.Notes
        );
    }

    /// <summary>
    /// Map list of BatchRelationship entities to response DTOs
    /// </summary>
    public static IReadOnlyList<BatchRelationshipResponse> ToRelationshipResponseList(IEnumerable<BatchRelationship> relationships)
    {
        return relationships.Select(ToRelationshipResponse).ToList();
    }

    private static Dictionary<string, object> NormalizeEventData(Dictionary<string, object> source)
    {
        if (source.Count == 0)
        {
            return new Dictionary<string, object>();
        }

        var normalized = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in source)
        {
            normalized[kvp.Key] = ConvertJsonValue(kvp.Value);
        }

        return normalized;
    }

    private static object? ConvertJsonValue(object value)
    {
        return value switch
        {
            JsonElement json => json.ValueKind switch
            {
                JsonValueKind.String => json.GetString(),
                JsonValueKind.Number when json.TryGetInt64(out var l) => l,
                JsonValueKind.Number when json.TryGetDouble(out var d) => d,
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => json.ToString()
            },
            _ => value
        };
    }

    private static Guid? GetGuid(IReadOnlyDictionary<string, object> data, string key)
    {
        if (!data.TryGetValue(key, out var value))
        {
            return null;
        }

        return value switch
        {
            Guid guid => guid,
            string s when Guid.TryParse(s, out var parsed) => parsed,
            JsonElement json when json.ValueKind == JsonValueKind.String && Guid.TryParse(json.GetString(), out var parsed) => parsed,
            _ => null
        };
    }

    private static int? GetInt(IReadOnlyDictionary<string, object> data, string key)
    {
        if (!data.TryGetValue(key, out var value))
        {
            return null;
        }

        return value switch
        {
            int i => i,
            long l => (int)l,
            double d => (int)d,
            string s when int.TryParse(s, out var parsed) => parsed,
            JsonElement json when json.ValueKind == JsonValueKind.Number && json.TryGetInt32(out var parsed) => parsed,
            JsonElement json when json.ValueKind == JsonValueKind.Number && json.TryGetInt64(out var parsed64) => (int)parsed64,
            _ => null
        };
    }
}
