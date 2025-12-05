using Harvestry.Items.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Items.Domain.Entities;

/// <summary>
/// Item/Product definition aggregate root - defines products for METRC
/// </summary>
public sealed partial class Item : AggregateRoot<Guid>
{
    // Private constructor for EF Core
    private Item(Guid id) : base(id) { }

    private Item(
        Guid id,
        Guid siteId,
        string name,
        ItemCategory category,
        UnitOfMeasure unitOfMeasure,
        Guid createdByUserId,
        Guid? strainId = null,
        string? strainName = null) : base(id)
    {
        ValidateConstructorArgs(siteId, name, createdByUserId);

        SiteId = siteId;
        Name = name.Trim();
        Category = category;
        UnitOfMeasure = unitOfMeasure;
        StrainId = strainId;
        StrainName = strainName?.Trim();
        Status = ItemStatus.Active;
        CreatedByUserId = createdByUserId;
        UpdatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Core identification
    public Guid SiteId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public ItemCategory Category { get; private set; }
    public UnitOfMeasure UnitOfMeasure { get; private set; }
    public ItemStatus Status { get; private set; }

    // Strain association (for strain-specific items)
    public Guid? StrainId { get; private set; }
    public string? StrainName { get; private set; }

    // Unit weight for count-based items with weight reporting
    public decimal? UnitWeight { get; private set; }
    public string? UnitWeightUnitOfMeasure { get; private set; }

    // Default potency values
    public decimal? DefaultThcPercent { get; private set; }
    public decimal? DefaultThcContent { get; private set; }
    public string? DefaultThcContentUnitOfMeasure { get; private set; }
    public decimal? DefaultCbdPercent { get; private set; }
    public decimal? DefaultCbdContent { get; private set; }

    // Lab testing requirements
    public bool RequiresLabTesting { get; private set; }
    public string? DefaultLabTestingState { get; private set; }

    // Additional item properties
    public string? Description { get; private set; }
    public string? Sku { get; private set; }
    public string? Barcode { get; private set; }

    // METRC sync tracking
    public long? MetrcItemId { get; private set; }
    public DateTime? MetrcLastSyncAt { get; private set; }
    public string? MetrcSyncStatus { get; private set; }

    // Metadata
    public Dictionary<string, object> Metadata { get; private set; } = new();

    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    /// <summary>
    /// Factory method to create a new item
    /// </summary>
    public static Item Create(
        Guid siteId,
        string name,
        ItemCategory category,
        UnitOfMeasure unitOfMeasure,
        Guid createdByUserId,
        Guid? strainId = null,
        string? strainName = null)
    {
        return new Item(
            Guid.NewGuid(),
            siteId,
            name,
            category,
            unitOfMeasure,
            createdByUserId,
            strainId,
            strainName);
    }

    /// <summary>
    /// Update item details
    /// </summary>
    public void Update(
        string name,
        ItemCategory category,
        UnitOfMeasure unitOfMeasure,
        Guid userId)
    {
        ValidateUserId(userId);

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        Name = name.Trim();
        Category = category;
        UnitOfMeasure = unitOfMeasure;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set strain association
    /// </summary>
    public void SetStrain(Guid? strainId, string? strainName, Guid userId)
    {
        ValidateUserId(userId);

        StrainId = strainId;
        StrainName = strainName?.Trim();
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set unit weight for count-based items
    /// </summary>
    public void SetUnitWeight(decimal unitWeight, string unitOfMeasure, Guid userId)
    {
        ValidateUserId(userId);

        if (unitWeight <= 0)
            throw new ArgumentException("Unit weight must be greater than 0", nameof(unitWeight));

        UnitWeight = unitWeight;
        UnitWeightUnitOfMeasure = unitOfMeasure?.Trim();
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set default potency values
    /// </summary>
    public void SetDefaultPotency(
        decimal? thcPercent,
        decimal? thcContent,
        string? thcContentUom,
        decimal? cbdPercent,
        decimal? cbdContent,
        Guid userId)
    {
        ValidateUserId(userId);

        DefaultThcPercent = thcPercent;
        DefaultThcContent = thcContent;
        DefaultThcContentUnitOfMeasure = thcContentUom?.Trim();
        DefaultCbdPercent = cbdPercent;
        DefaultCbdContent = cbdContent;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set lab testing requirements
    /// </summary>
    public void SetLabTestingRequirements(bool requiresLabTesting, string? defaultLabTestingState, Guid userId)
    {
        ValidateUserId(userId);

        RequiresLabTesting = requiresLabTesting;
        DefaultLabTestingState = defaultLabTestingState?.Trim();
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set additional properties
    /// </summary>
    public void SetAdditionalProperties(string? description, string? sku, string? barcode, Guid userId)
    {
        ValidateUserId(userId);

        Description = description?.Trim();
        Sku = sku?.Trim();
        Barcode = barcode?.Trim();
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activate the item
    /// </summary>
    public void Activate(Guid userId)
    {
        ValidateUserId(userId);

        Status = ItemStatus.Active;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivate/archive the item
    /// </summary>
    public void Deactivate(Guid userId)
    {
        ValidateUserId(userId);

        Status = ItemStatus.Inactive;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update METRC sync information
    /// </summary>
    public void UpdateMetrcSync(long metrcItemId, string? syncStatus = null)
    {
        MetrcItemId = metrcItemId;
        MetrcLastSyncAt = DateTime.UtcNow;
        MetrcSyncStatus = syncStatus ?? "Synced";
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
    }

    private static void ValidateConstructorArgs(
        Guid siteId,
        string name,
        Guid createdByUserId)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Name cannot exceed 200 characters", nameof(name));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty", nameof(createdByUserId));
    }
}




