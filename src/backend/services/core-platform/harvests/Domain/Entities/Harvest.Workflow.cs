using Harvestry.Harvests.Domain.Enums;

namespace Harvestry.Harvests.Domain.Entities;

/// <summary>
/// Harvest workflow extensions - tracks the full harvest lifecycle
/// </summary>
public sealed partial class Harvest
{
    private readonly List<WeightAdjustment> _weightAdjustments = new();

    // ===== WORKFLOW PHASE =====
    public HarvestPhase Phase { get; private set; } = HarvestPhase.WetHarvest;

    // ===== DRYING TRACKING =====
    public DateOnly? DryingStartDate { get; private set; }
    public DateOnly? DryingEndDate { get; private set; }
    public int? DryingDurationDays { get; private set; }
    public Guid? DryingLocationId { get; private set; }
    public string? DryingLocationName { get; private set; }

    // ===== BUCKING TRACKING =====
    public DateOnly? BuckingDate { get; private set; }
    public decimal BuckedFlowerWeight { get; private set; }
    public decimal TotalStemWaste { get; private set; }
    public decimal TotalLeafWaste { get; private set; }
    public decimal TotalOtherWaste { get; private set; }

    // ===== CALCULATED METRICS =====
    /// <summary>Moisture loss percentage: (wet - dry) / wet * 100</summary>
    public decimal? MoistureLossPercent { get; private set; }

    /// <summary>Dry to wet ratio: dry / wet</summary>
    public decimal? DryToWetRatio { get; private set; }

    /// <summary>Usable flower percentage: bucked flower / dry * 100</summary>
    public decimal? UsableFlowerPercent { get; private set; }

    /// <summary>Total waste percentage: total waste / wet * 100</summary>
    public decimal? TotalWastePercent { get; private set; }

    // ===== WEIGHT LOCK STATUS =====
    public bool WetWeightLocked { get; private set; }
    public bool DryWeightLocked { get; private set; }
    public DateTime? WetWeightLockedAt { get; private set; }
    public DateTime? DryWeightLockedAt { get; private set; }
    public Guid? WetWeightLockedByUserId { get; private set; }
    public Guid? DryWeightLockedByUserId { get; private set; }

    // ===== BATCHING =====
    public HarvestBatchingMode? BatchingMode { get; private set; }
    public Guid? ParentHarvestId { get; private set; }
    public List<Guid> ChildHarvestIds { get; private set; } = new();

    // ===== NAVIGATION =====
    public IReadOnlyCollection<WeightAdjustment> WeightAdjustments => _weightAdjustments.AsReadOnly();

    #region Drying Methods

    /// <summary>
    /// Start the drying phase
    /// </summary>
    public void StartDrying(Guid? dryingLocationId, string? dryingLocationName, Guid userId)
    {
        ValidateUserId(userId);
        ValidateActiveStatus();

        if (Phase != HarvestPhase.WetHarvest)
            throw new InvalidOperationException($"Cannot start drying from phase {Phase}");

        Phase = HarvestPhase.Drying;
        DryingStartDate = DateOnly.FromDateTime(DateTime.UtcNow);
        DryingLocationId = dryingLocationId;
        DryingLocationName = dryingLocationName?.Trim();
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Complete the drying phase
    /// </summary>
    public void CompleteDrying(Guid userId)
    {
        ValidateUserId(userId);
        ValidateActiveStatus();

        if (Phase != HarvestPhase.Drying)
            throw new InvalidOperationException($"Cannot complete drying from phase {Phase}");

        DryingEndDate = DateOnly.FromDateTime(DateTime.UtcNow);
        DryingDurationDays = DryingStartDate.HasValue
            ? DryingEndDate.Value.DayNumber - DryingStartDate.Value.DayNumber
            : null;
        Phase = HarvestPhase.Bucking;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    #endregion

    #region Bucking Methods

    /// <summary>
    /// Record the results of bucking (separating flower from stems)
    /// </summary>
    public void RecordBuckingResults(
        decimal buckedFlowerWeight,
        decimal stemWaste,
        decimal leafWaste,
        decimal otherWaste,
        Guid userId)
    {
        ValidateUserId(userId);
        ValidateActiveStatus();

        if (Phase != HarvestPhase.Bucking && Phase != HarvestPhase.Drying)
            throw new InvalidOperationException($"Cannot record bucking results from phase {Phase}");

        if (buckedFlowerWeight < 0)
            throw new ArgumentException("Bucked flower weight cannot be negative", nameof(buckedFlowerWeight));

        BuckingDate = DateOnly.FromDateTime(DateTime.UtcNow);
        BuckedFlowerWeight = buckedFlowerWeight;
        TotalStemWaste = stemWaste;
        TotalLeafWaste = leafWaste;
        TotalOtherWaste = otherWaste;

        // Update current weight and total waste
        CurrentWeight = buckedFlowerWeight;
        TotalWasteWeight = stemWaste + leafWaste + otherWaste;

        Phase = HarvestPhase.DryWeighed;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;

        // Calculate metrics
        CalculateMetrics();
    }

    #endregion

    #region Weight Lock Methods

    /// <summary>
    /// Lock wet weight to prevent further changes without PIN override
    /// </summary>
    public void LockWetWeight(Guid userId)
    {
        ValidateUserId(userId);

        if (WetWeightLocked)
            throw new InvalidOperationException("Wet weight is already locked");

        WetWeightLocked = true;
        WetWeightLockedAt = DateTime.UtcNow;
        WetWeightLockedByUserId = userId;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Lock dry weight to prevent further changes without PIN override
    /// </summary>
    public void LockDryWeight(Guid userId)
    {
        ValidateUserId(userId);

        if (DryWeightLocked)
            throw new InvalidOperationException("Dry weight is already locked");

        DryWeightLocked = true;
        DryWeightLockedAt = DateTime.UtcNow;
        DryWeightLockedByUserId = userId;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adjust a locked weight with PIN override (creates audit record)
    /// </summary>
    public WeightAdjustment AdjustWeight(
        WeightType weightType,
        decimal newWeight,
        string reasonCode,
        string? notes,
        Guid userId,
        bool pinOverrideUsed)
    {
        ValidateUserId(userId);

        if (newWeight < 0)
            throw new ArgumentException("Weight cannot be negative", nameof(newWeight));

        if (string.IsNullOrWhiteSpace(reasonCode))
            throw new ArgumentException("Reason code is required for weight adjustment", nameof(reasonCode));

        decimal previousWeight = weightType switch
        {
            WeightType.WetPlant => TotalWetWeight,
            WeightType.DryPlant => TotalDryWeight,
            WeightType.BuckedFlower => BuckedFlowerWeight,
            WeightType.StemWaste => TotalStemWaste,
            WeightType.LeafWaste => TotalLeafWaste,
            WeightType.OtherWaste => TotalOtherWaste,
            _ => throw new ArgumentException($"Unknown weight type: {weightType}", nameof(weightType))
        };

        // Create adjustment record
        var adjustment = WeightAdjustment.Create(
            Id,
            weightType,
            previousWeight,
            newWeight,
            reasonCode,
            notes,
            userId,
            pinOverrideUsed);

        _weightAdjustments.Add(adjustment);

        // Apply the new weight
        switch (weightType)
        {
            case WeightType.WetPlant:
                TotalWetWeight = newWeight;
                break;
            case WeightType.DryPlant:
                TotalDryWeight = newWeight;
                break;
            case WeightType.BuckedFlower:
                BuckedFlowerWeight = newWeight;
                CurrentWeight = newWeight;
                break;
            case WeightType.StemWaste:
                TotalStemWaste = newWeight;
                TotalWasteWeight = TotalStemWaste + TotalLeafWaste + TotalOtherWaste;
                break;
            case WeightType.LeafWaste:
                TotalLeafWaste = newWeight;
                TotalWasteWeight = TotalStemWaste + TotalLeafWaste + TotalOtherWaste;
                break;
            case WeightType.OtherWaste:
                TotalOtherWaste = newWeight;
                TotalWasteWeight = TotalStemWaste + TotalLeafWaste + TotalOtherWaste;
                break;
        }

        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;

        // Recalculate metrics
        CalculateMetrics();

        return adjustment;
    }

    #endregion

    #region Batching Methods

    /// <summary>
    /// Set the batching mode for this harvest
    /// </summary>
    public void SetBatchingMode(HarvestBatchingMode mode, Guid? parentHarvestId, Guid userId)
    {
        ValidateUserId(userId);

        if (mode == HarvestBatchingMode.SubLot && !parentHarvestId.HasValue)
            throw new ArgumentException("Parent harvest ID is required for sub-lot mode", nameof(parentHarvestId));

        BatchingMode = mode;
        ParentHarvestId = parentHarvestId;
        Phase = HarvestPhase.Batched;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Add a child harvest ID (when splitting into sub-lots)
    /// </summary>
    public void AddChildHarvestId(Guid childHarvestId, Guid userId)
    {
        ValidateUserId(userId);

        if (childHarvestId == Guid.Empty)
            throw new ArgumentException("Child harvest ID cannot be empty", nameof(childHarvestId));

        if (!ChildHarvestIds.Contains(childHarvestId))
        {
            ChildHarvestIds.Add(childHarvestId);
            UpdatedByUserId = userId;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Mark harvest as having lots created
    /// </summary>
    public void MarkLotsCreated(Guid userId)
    {
        ValidateUserId(userId);

        Phase = HarvestPhase.LotCreated;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark harvest workflow as complete
    /// </summary>
    public void CompleteWorkflow(Guid userId)
    {
        ValidateUserId(userId);

        Phase = HarvestPhase.Complete;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    #endregion

    #region Metric Calculations

    /// <summary>
    /// Calculate and store all harvest metrics
    /// </summary>
    public void CalculateMetrics()
    {
        if (TotalWetWeight > 0)
        {
            // Moisture loss: (wet - dry) / wet * 100
            if (TotalDryWeight > 0 || BuckedFlowerWeight > 0)
            {
                var dryWeight = BuckedFlowerWeight > 0 ? BuckedFlowerWeight : TotalDryWeight;
                MoistureLossPercent = Math.Round((TotalWetWeight - dryWeight) / TotalWetWeight * 100, 2);
                DryToWetRatio = Math.Round(dryWeight / TotalWetWeight, 4);
            }

            // Total waste percentage
            TotalWastePercent = Math.Round(TotalWasteWeight / TotalWetWeight * 100, 2);
        }

        // Usable flower percentage (based on dry weight if available)
        if (TotalDryWeight > 0 && BuckedFlowerWeight > 0)
        {
            UsableFlowerPercent = Math.Round(BuckedFlowerWeight / TotalDryWeight * 100, 2);
        }
    }

    #endregion

    #region Persistence Helpers

    /// <summary>
    /// Set weight adjustments from persistence
    /// </summary>
    public void SetWeightAdjustments(IEnumerable<WeightAdjustment> adjustments)
    {
        _weightAdjustments.Clear();
        if (adjustments != null)
            _weightAdjustments.AddRange(adjustments);
    }

    /// <summary>
    /// Set child harvest IDs from persistence
    /// </summary>
    public void SetChildHarvestIds(IEnumerable<Guid> childIds)
    {
        ChildHarvestIds.Clear();
        if (childIds != null)
            ChildHarvestIds.AddRange(childIds);
    }

    #endregion
}
