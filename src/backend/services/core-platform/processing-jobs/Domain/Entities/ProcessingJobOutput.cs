using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.ProcessingJobs.Domain.Entities;

/// <summary>
/// Output package created from a processing job
/// </summary>
public sealed class ProcessingJobOutput : Entity<Guid>
{
    // Private constructor for EF Core
    private ProcessingJobOutput(Guid id) : base(id) { }

    private ProcessingJobOutput(
        Guid id,
        Guid processingJobId,
        string packageLabel,
        Guid itemId,
        string itemName,
        decimal quantity,
        string unitOfMeasure) : base(id)
    {
        ProcessingJobId = processingJobId;
        PackageLabel = packageLabel;
        ItemId = itemId;
        ItemName = itemName;
        Quantity = quantity;
        UnitOfMeasure = unitOfMeasure;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid ProcessingJobId { get; private set; }
    public string PackageLabel { get; private set; } = string.Empty;
    public Guid ItemId { get; private set; }
    public string ItemName { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public string UnitOfMeasure { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Factory method to create a new output
    /// </summary>
    public static ProcessingJobOutput Create(
        Guid processingJobId,
        string packageLabel,
        Guid itemId,
        string itemName,
        decimal quantity,
        string unitOfMeasure)
    {
        if (processingJobId == Guid.Empty)
            throw new ArgumentException("Processing job ID cannot be empty", nameof(processingJobId));

        if (string.IsNullOrWhiteSpace(packageLabel))
            throw new ArgumentException("Package label cannot be empty", nameof(packageLabel));

        if (itemId == Guid.Empty)
            throw new ArgumentException("Item ID cannot be empty", nameof(itemId));

        if (string.IsNullOrWhiteSpace(itemName))
            throw new ArgumentException("Item name cannot be empty", nameof(itemName));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

        if (string.IsNullOrWhiteSpace(unitOfMeasure))
            throw new ArgumentException("Unit of measure cannot be empty", nameof(unitOfMeasure));

        return new ProcessingJobOutput(
            Guid.NewGuid(),
            processingJobId,
            packageLabel.Trim().ToUpperInvariant(),
            itemId,
            itemName.Trim(),
            quantity,
            unitOfMeasure.Trim());
    }

    /// <summary>
    /// Restore from persistence
    /// </summary>
    public static ProcessingJobOutput Restore(
        Guid id,
        Guid processingJobId,
        string packageLabel,
        Guid itemId,
        string itemName,
        decimal quantity,
        string unitOfMeasure,
        DateTime createdAt)
    {
        return new ProcessingJobOutput(id)
        {
            ProcessingJobId = processingJobId,
            PackageLabel = packageLabel,
            ItemId = itemId,
            ItemName = itemName,
            Quantity = quantity,
            UnitOfMeasure = unitOfMeasure,
            CreatedAt = createdAt
        };
    }
}








