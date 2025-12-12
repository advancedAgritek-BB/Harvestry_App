using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.ProcessingJobs.Domain.Entities;

/// <summary>
/// Input package used in a processing job
/// </summary>
public sealed class ProcessingJobInput : Entity<Guid>
{
    // Private constructor for EF Core
    private ProcessingJobInput(Guid id) : base(id) { }

    private ProcessingJobInput(
        Guid id,
        Guid processingJobId,
        string packageLabel,
        decimal quantity,
        string unitOfMeasure) : base(id)
    {
        ProcessingJobId = processingJobId;
        PackageLabel = packageLabel;
        Quantity = quantity;
        UnitOfMeasure = unitOfMeasure;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid ProcessingJobId { get; private set; }
    public string PackageLabel { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public string UnitOfMeasure { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Factory method to create a new input
    /// </summary>
    public static ProcessingJobInput Create(
        Guid processingJobId,
        string packageLabel,
        decimal quantity,
        string unitOfMeasure)
    {
        if (processingJobId == Guid.Empty)
            throw new ArgumentException("Processing job ID cannot be empty", nameof(processingJobId));

        if (string.IsNullOrWhiteSpace(packageLabel))
            throw new ArgumentException("Package label cannot be empty", nameof(packageLabel));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

        if (string.IsNullOrWhiteSpace(unitOfMeasure))
            throw new ArgumentException("Unit of measure cannot be empty", nameof(unitOfMeasure));

        return new ProcessingJobInput(
            Guid.NewGuid(),
            processingJobId,
            packageLabel.Trim().ToUpperInvariant(),
            quantity,
            unitOfMeasure.Trim());
    }

    /// <summary>
    /// Restore from persistence
    /// </summary>
    public static ProcessingJobInput Restore(
        Guid id,
        Guid processingJobId,
        string packageLabel,
        decimal quantity,
        string unitOfMeasure,
        DateTime createdAt)
    {
        return new ProcessingJobInput(id)
        {
            ProcessingJobId = processingJobId,
            PackageLabel = packageLabel,
            Quantity = quantity,
            UnitOfMeasure = unitOfMeasure,
            CreatedAt = createdAt
        };
    }
}









