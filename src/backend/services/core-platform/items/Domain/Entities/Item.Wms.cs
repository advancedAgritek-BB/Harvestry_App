namespace Harvestry.Items.Domain.Entities;

/// <summary>
/// Item entity WMS extensions - reorder management, pricing, classification, tracking
/// </summary>
public sealed partial class Item
{
    // =========================================================================
    // INVENTORY CLASSIFICATION
    // =========================================================================

    /// <summary>
    /// Default inventory classification for packages of this item
    /// </summary>
    public string InventoryCategory { get; private set; } = "finished_good";

    // =========================================================================
    // TRACKING FLAGS
    // =========================================================================

    /// <summary>
    /// Whether this item requires lot/batch tracking
    /// </summary>
    public bool IsLotTracked { get; private set; } = true;

    /// <summary>
    /// Whether this item requires serial number tracking
    /// </summary>
    public bool IsSerialTracked { get; private set; }

    // =========================================================================
    // REORDER MANAGEMENT
    // =========================================================================

    /// <summary>
    /// Quantity threshold that triggers reorder alert
    /// </summary>
    public decimal? ReorderPoint { get; private set; }

    /// <summary>
    /// Suggested quantity to reorder
    /// </summary>
    public decimal? ReorderQuantity { get; private set; }

    /// <summary>
    /// Minimum stock level to maintain
    /// </summary>
    public decimal? SafetyStock { get; private set; }

    /// <summary>
    /// Expected days from order to delivery
    /// </summary>
    public int? LeadTimeDays { get; private set; }

    /// <summary>
    /// Minimum quantity for purchase orders
    /// </summary>
    public decimal? MinOrderQuantity { get; private set; }

    /// <summary>
    /// Maximum quantity for purchase orders
    /// </summary>
    public decimal? MaxOrderQuantity { get; private set; }

    // =========================================================================
    // PRICING
    // =========================================================================

    /// <summary>
    /// Standard retail/list price
    /// </summary>
    public decimal? ListPrice { get; private set; }

    /// <summary>
    /// Wholesale/bulk price
    /// </summary>
    public decimal? WholesalePrice { get; private set; }

    /// <summary>
    /// Standard cost for inventory valuation
    /// </summary>
    public decimal? CostPrice { get; private set; }

    /// <summary>
    /// Target margin percentage
    /// </summary>
    public decimal? MarginPercent { get; private set; }

    // =========================================================================
    // PRODUCT FLAGS
    // =========================================================================

    /// <summary>
    /// Can be sold to customers
    /// </summary>
    public bool IsSellable { get; private set; } = true;

    /// <summary>
    /// Can be purchased from vendors
    /// </summary>
    public bool IsPurchasable { get; private set; }

    /// <summary>
    /// Can be produced/manufactured
    /// </summary>
    public bool IsProducible { get; private set; }

    /// <summary>
    /// Currently available for sale
    /// </summary>
    public bool IsActiveForSale { get; private set; } = true;

    // =========================================================================
    // DEFAULT LOCATIONS
    // =========================================================================

    /// <summary>
    /// Default location for receiving this item
    /// </summary>
    public Guid? DefaultReceivingLocationId { get; private set; }

    /// <summary>
    /// Default storage location
    /// </summary>
    public Guid? DefaultStorageLocationId { get; private set; }

    /// <summary>
    /// Default location for production output
    /// </summary>
    public Guid? DefaultProductionLocationId { get; private set; }

    // =========================================================================
    // SHELF LIFE
    // =========================================================================

    /// <summary>
    /// Default shelf life in days for new packages
    /// </summary>
    public int? ShelfLifeDays { get; private set; }

    /// <summary>
    /// Whether packages must have expiration date
    /// </summary>
    public bool RequiresExpirationDate { get; private set; }

    // =========================================================================
    // WEIGHT TRACKING
    // =========================================================================

    /// <summary>
    /// Standard weight for count-based items
    /// </summary>
    public decimal? StandardWeight { get; private set; }

    /// <summary>
    /// Unit of measure for standard weight
    /// </summary>
    public string? StandardWeightUom { get; private set; }

    /// <summary>
    /// Acceptable variance percent for weight
    /// </summary>
    public decimal WeightTolerancePercent { get; private set; } = 5.0m;

    // =========================================================================
    // REORDER MANAGEMENT METHODS
    // =========================================================================

    /// <summary>
    /// Set reorder parameters
    /// </summary>
    public void SetReorderParameters(
        decimal? reorderPoint,
        decimal? reorderQuantity,
        decimal? safetyStock,
        int? leadTimeDays,
        Guid userId)
    {
        ValidateUserId(userId);

        if (reorderPoint.HasValue && reorderPoint.Value < 0)
            throw new ArgumentException("Reorder point cannot be negative", nameof(reorderPoint));
        if (reorderQuantity.HasValue && reorderQuantity.Value <= 0)
            throw new ArgumentException("Reorder quantity must be positive", nameof(reorderQuantity));
        if (safetyStock.HasValue && safetyStock.Value < 0)
            throw new ArgumentException("Safety stock cannot be negative", nameof(safetyStock));
        if (leadTimeDays.HasValue && leadTimeDays.Value < 0)
            throw new ArgumentException("Lead time days cannot be negative", nameof(leadTimeDays));

        ReorderPoint = reorderPoint;
        ReorderQuantity = reorderQuantity;
        SafetyStock = safetyStock;
        LeadTimeDays = leadTimeDays;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set order quantity constraints
    /// </summary>
    public void SetOrderQuantityConstraints(decimal? minQuantity, decimal? maxQuantity, Guid userId)
    {
        ValidateUserId(userId);

        if (minQuantity.HasValue && minQuantity.Value < 0)
            throw new ArgumentException("Minimum order quantity cannot be negative", nameof(minQuantity));
        if (maxQuantity.HasValue && maxQuantity.Value < 0)
            throw new ArgumentException("Maximum order quantity cannot be negative", nameof(maxQuantity));
        if (minQuantity.HasValue && maxQuantity.HasValue && minQuantity.Value > maxQuantity.Value)
            throw new ArgumentException("Minimum order quantity cannot exceed maximum");

        MinOrderQuantity = minQuantity;
        MaxOrderQuantity = maxQuantity;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    // =========================================================================
    // PRICING METHODS
    // =========================================================================

    /// <summary>
    /// Set pricing information
    /// </summary>
    public void SetPricing(
        decimal? listPrice,
        decimal? wholesalePrice,
        decimal? costPrice,
        decimal? marginPercent,
        Guid userId)
    {
        ValidateUserId(userId);

        if (listPrice.HasValue && listPrice.Value < 0)
            throw new ArgumentException("List price cannot be negative", nameof(listPrice));
        if (wholesalePrice.HasValue && wholesalePrice.Value < 0)
            throw new ArgumentException("Wholesale price cannot be negative", nameof(wholesalePrice));
        if (costPrice.HasValue && costPrice.Value < 0)
            throw new ArgumentException("Cost price cannot be negative", nameof(costPrice));
        if (marginPercent.HasValue && (marginPercent.Value < 0 || marginPercent.Value > 100))
            throw new ArgumentException("Margin percent must be between 0 and 100", nameof(marginPercent));

        ListPrice = listPrice;
        WholesalePrice = wholesalePrice;
        CostPrice = costPrice;
        MarginPercent = marginPercent;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    // =========================================================================
    // CLASSIFICATION METHODS
    // =========================================================================

    /// <summary>
    /// Set inventory category
    /// </summary>
    public void SetInventoryCategory(string category, Guid userId)
    {
        ValidateUserId(userId);

        var validCategories = new[] { "raw_material", "work_in_progress", "finished_good", "consumable", "byproduct" };
        if (!validCategories.Contains(category.ToLowerInvariant()))
            throw new ArgumentException($"Invalid inventory category: {category}", nameof(category));

        InventoryCategory = category.ToLowerInvariant();
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set tracking flags
    /// </summary>
    public void SetTrackingFlags(bool isLotTracked, bool isSerialTracked, Guid userId)
    {
        ValidateUserId(userId);

        IsLotTracked = isLotTracked;
        IsSerialTracked = isSerialTracked;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set product flags
    /// </summary>
    public void SetProductFlags(bool isSellable, bool isPurchasable, bool isProducible, bool isActiveForSale, Guid userId)
    {
        ValidateUserId(userId);

        IsSellable = isSellable;
        IsPurchasable = isPurchasable;
        IsProducible = isProducible;
        IsActiveForSale = isActiveForSale;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    // =========================================================================
    // DEFAULT LOCATION METHODS
    // =========================================================================

    /// <summary>
    /// Set default locations
    /// </summary>
    public void SetDefaultLocations(
        Guid? receivingLocationId,
        Guid? storageLocationId,
        Guid? productionLocationId,
        Guid userId)
    {
        ValidateUserId(userId);

        DefaultReceivingLocationId = receivingLocationId;
        DefaultStorageLocationId = storageLocationId;
        DefaultProductionLocationId = productionLocationId;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    // =========================================================================
    // SHELF LIFE METHODS
    // =========================================================================

    /// <summary>
    /// Set shelf life parameters
    /// </summary>
    public void SetShelfLife(int? shelfLifeDays, bool requiresExpirationDate, Guid userId)
    {
        ValidateUserId(userId);

        if (shelfLifeDays.HasValue && shelfLifeDays.Value < 0)
            throw new ArgumentException("Shelf life days cannot be negative", nameof(shelfLifeDays));

        ShelfLifeDays = shelfLifeDays;
        RequiresExpirationDate = requiresExpirationDate;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    // =========================================================================
    // WEIGHT METHODS
    // =========================================================================

    /// <summary>
    /// Set weight tracking parameters
    /// </summary>
    public void SetWeightParameters(decimal? standardWeight, string? uom, decimal tolerancePercent, Guid userId)
    {
        ValidateUserId(userId);

        if (standardWeight.HasValue && standardWeight.Value < 0)
            throw new ArgumentException("Standard weight cannot be negative", nameof(standardWeight));
        if (tolerancePercent < 0 || tolerancePercent > 100)
            throw new ArgumentException("Tolerance percent must be between 0 and 100", nameof(tolerancePercent));

        StandardWeight = standardWeight;
        StandardWeightUom = uom?.Trim();
        WeightTolerancePercent = tolerancePercent;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
    }
}



