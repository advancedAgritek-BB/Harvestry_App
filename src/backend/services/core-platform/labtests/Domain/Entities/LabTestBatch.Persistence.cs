using Harvestry.LabTests.Domain.Enums;

namespace Harvestry.LabTests.Domain.Entities;

public sealed partial class LabTestBatch
{
    /// <summary>
    /// Restores a LabTestBatch entity from persistence
    /// </summary>
    public static LabTestBatch Restore(
        Guid id,
        Guid siteId,
        string packageLabel,
        string labFacilityLicenseNumber,
        string labFacilityName,
        DateOnly? collectedDate,
        DateOnly? receivedDate,
        DateOnly? testCompletedDate,
        LabTestStatus status,
        string? notes,
        long? metrcLabTestId,
        DateTime? metrcLastSyncAt,
        string? metrcSyncStatus,
        string? documentUrl,
        string? documentFileName,
        IDictionary<string, object>? metadata,
        DateTime createdAt,
        Guid createdByUserId,
        DateTime updatedAt,
        Guid updatedByUserId)
    {
        var labTestBatch = new LabTestBatch(id)
        {
            SiteId = siteId,
            PackageLabel = packageLabel,
            LabFacilityLicenseNumber = labFacilityLicenseNumber,
            LabFacilityName = labFacilityName,
            CollectedDate = collectedDate,
            ReceivedDate = receivedDate,
            TestCompletedDate = testCompletedDate,
            Status = status,
            Notes = notes,
            MetrcLabTestId = metrcLabTestId,
            MetrcLastSyncAt = metrcLastSyncAt,
            MetrcSyncStatus = metrcSyncStatus,
            DocumentUrl = documentUrl,
            DocumentFileName = documentFileName,
            Metadata = metadata != null
                ? new Dictionary<string, object>(metadata)
                : new Dictionary<string, object>(),
            CreatedAt = createdAt,
            CreatedByUserId = createdByUserId,
            UpdatedAt = updatedAt,
            UpdatedByUserId = updatedByUserId
        };

        return labTestBatch;
    }
}




