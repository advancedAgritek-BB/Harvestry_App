using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Packages.Domain.Entities;

/// <summary>
/// Package remediation record (METRC package remediate)
/// </summary>
public sealed class PackageRemediation : Entity<Guid>
{
    // Private constructor for EF Core
    private PackageRemediation(Guid id) : base(id) { }

    private PackageRemediation(
        Guid id,
        Guid packageId,
        string remediationMethodName,
        DateOnly remediationDate,
        Guid performedByUserId,
        string? remediationSteps = null) : base(id)
    {
        PackageId = packageId;
        RemediationMethodName = remediationMethodName;
        RemediationDate = remediationDate;
        PerformedByUserId = performedByUserId;
        RemediationSteps = remediationSteps;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid PackageId { get; private set; }
    public string RemediationMethodName { get; private set; } = string.Empty;
    public string? RemediationSteps { get; private set; }
    public DateOnly RemediationDate { get; private set; }
    public Guid PerformedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // METRC sync
    public long? MetrcRemediationId { get; private set; }

    /// <summary>
    /// Factory method to create a new remediation record
    /// </summary>
    public static PackageRemediation Create(
        Guid packageId,
        string remediationMethodName,
        DateOnly remediationDate,
        Guid performedByUserId,
        string? remediationSteps = null)
    {
        if (packageId == Guid.Empty)
            throw new ArgumentException("Package ID cannot be empty", nameof(packageId));

        if (string.IsNullOrWhiteSpace(remediationMethodName))
            throw new ArgumentException("Remediation method name cannot be empty", nameof(remediationMethodName));

        if (performedByUserId == Guid.Empty)
            throw new ArgumentException("Performed by user ID cannot be empty", nameof(performedByUserId));

        return new PackageRemediation(
            Guid.NewGuid(),
            packageId,
            remediationMethodName.Trim(),
            remediationDate,
            performedByUserId,
            remediationSteps?.Trim());
    }

    /// <summary>
    /// Update METRC sync information
    /// </summary>
    public void UpdateMetrcSync(long metrcRemediationId)
    {
        MetrcRemediationId = metrcRemediationId;
    }

    /// <summary>
    /// Restore from persistence
    /// </summary>
    public static PackageRemediation Restore(
        Guid id,
        Guid packageId,
        string remediationMethodName,
        string? remediationSteps,
        DateOnly remediationDate,
        Guid performedByUserId,
        DateTime createdAt,
        long? metrcRemediationId)
    {
        return new PackageRemediation(id)
        {
            PackageId = packageId,
            RemediationMethodName = remediationMethodName,
            RemediationSteps = remediationSteps,
            RemediationDate = remediationDate,
            PerformedByUserId = performedByUserId,
            CreatedAt = createdAt,
            MetrcRemediationId = metrcRemediationId
        };
    }
}








