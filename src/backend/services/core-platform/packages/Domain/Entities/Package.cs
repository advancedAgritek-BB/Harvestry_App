using Harvestry.Packages.Domain.Enums;
using Harvestry.Packages.Domain.ValueObjects;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Packages.Domain.Entities;

/// <summary>
/// Package aggregate root - core trackable unit in METRC
/// </summary>
public sealed partial class Package : AggregateRoot<Guid>
{
    private readonly List<PackageAdjustment> _adjustments = new();
    private readonly List<PackageRemediation> _remediations = new();
    private readonly List<string> _sourcePackageLabels = new();

    // Private constructor for EF Core
    private Package(Guid id) : base(id) { }

    private Package(
        Guid id,
        Guid siteId,
        PackageLabel packageLabel,
        Guid itemId,
        string itemName,
        string itemCategory,
        decimal quantity,
        string unitOfMeasure,
        Guid createdByUserId,
        Guid? locationId = null,
        string? sublocationName = null) : base(id)
    {
        ValidateConstructorArgs(siteId, packageLabel, itemId, itemName, quantity, unitOfMeasure, createdByUserId);

        SiteId = siteId;
        PackageLabel = packageLabel;
        ItemId = itemId;
        ItemName = itemName.Trim();
        ItemCategory = itemCategory?.Trim() ?? string.Empty;
        Quantity = quantity;
        InitialQuantity = quantity;
        UnitOfMeasure = unitOfMeasure.Trim();
        LocationId = locationId;
        SublocationName = sublocationName?.Trim();
        Status = PackageStatus.Active;
        LabTestingState = LabTestingState.NotSubmitted;
        PackagedDate = DateOnly.FromDateTime(DateTime.UtcNow);
        CreatedByUserId = createdByUserId;
        UpdatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Core identification
    public Guid SiteId { get; private set; }
    public PackageLabel PackageLabel { get; private set; } = null!;

    // Item information
    public Guid ItemId { get; private set; }
    public string ItemName { get; private set; } = string.Empty;
    public string ItemCategory { get; private set; } = string.Empty;

    // Quantity tracking
    public decimal Quantity { get; private set; }
    public decimal InitialQuantity { get; private set; }
    public string UnitOfMeasure { get; private set; } = string.Empty;

    // Location
    public Guid? LocationId { get; private set; }
    public string? LocationName { get; private set; }
    public string? SublocationName { get; private set; }

    // Source tracking
    public Guid? SourceHarvestId { get; private set; }
    public string? SourceHarvestName { get; private set; }

    // Production batch
    public string? ProductionBatchNumber { get; private set; }
    public bool IsProductionBatch { get; private set; }

    // Special flags
    public bool IsTradeSample { get; private set; }
    public bool IsDonation { get; private set; }
    public bool ProductRequiresRemediation { get; private set; }

    // Medical tracking
    public string? PatientLicenseNumber { get; private set; }

    // Dates
    public DateOnly PackagedDate { get; private set; }
    public DateOnly? ExpirationDate { get; private set; }
    public DateOnly? UseByDate { get; private set; }
    public DateOnly? FinishedDate { get; private set; }

    // Lab testing
    public LabTestingState LabTestingState { get; private set; }
    public bool LabTestingStateRequired { get; private set; }

    // Potency (from lab tests)
    public decimal? ThcPercent { get; private set; }
    public decimal? ThcContent { get; private set; }
    public string? ThcContentUnitOfMeasure { get; private set; }
    public decimal? CbdPercent { get; private set; }
    public decimal? CbdContent { get; private set; }

    // Status
    public PackageStatus Status { get; private set; }
    public PackageType PackageType { get; private set; }
    public string? Notes { get; private set; }

    // METRC sync tracking
    public long? MetrcPackageId { get; private set; }
    public DateTime? MetrcLastSyncAt { get; private set; }
    public string? MetrcSyncStatus { get; private set; }

    // Metadata
    public Dictionary<string, object> Metadata { get; private set; } = new();

    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    // Navigation collections
    public IReadOnlyCollection<PackageAdjustment> Adjustments => _adjustments.AsReadOnly();
    public IReadOnlyCollection<PackageRemediation> Remediations => _remediations.AsReadOnly();
    public IReadOnlyList<string> SourcePackageLabels => _sourcePackageLabels.AsReadOnly();

    /// <summary>
    /// Factory method to create a new package from a harvest
    /// </summary>
    public static Package CreateFromHarvest(
        Guid siteId,
        PackageLabel packageLabel,
        Guid itemId,
        string itemName,
        string itemCategory,
        decimal quantity,
        string unitOfMeasure,
        Guid sourceHarvestId,
        string sourceHarvestName,
        Guid createdByUserId,
        Guid? locationId = null,
        string? sublocationName = null)
    {
        var package = new Package(
            Guid.NewGuid(),
            siteId,
            packageLabel,
            itemId,
            itemName,
            itemCategory,
            quantity,
            unitOfMeasure,
            createdByUserId,
            locationId,
            sublocationName);

        package.SourceHarvestId = sourceHarvestId;
        package.SourceHarvestName = sourceHarvestName;

        return package;
    }

    /// <summary>
    /// Factory method to create a new package from other packages
    /// </summary>
    public static Package CreateFromPackages(
        Guid siteId,
        PackageLabel packageLabel,
        Guid itemId,
        string itemName,
        string itemCategory,
        decimal quantity,
        string unitOfMeasure,
        IEnumerable<string> sourcePackageLabels,
        Guid createdByUserId,
        Guid? locationId = null,
        string? sublocationName = null)
    {
        var package = new Package(
            Guid.NewGuid(),
            siteId,
            packageLabel,
            itemId,
            itemName,
            itemCategory,
            quantity,
            unitOfMeasure,
            createdByUserId,
            locationId,
            sublocationName);

        if (sourcePackageLabels != null)
        {
            package._sourcePackageLabels.AddRange(sourcePackageLabels.Select(l => l.Trim().ToUpperInvariant()));
        }

        return package;
    }

    /// <summary>
    /// Adjust package quantity
    /// </summary>
    public PackageAdjustment Adjust(
        decimal adjustmentQuantity,
        AdjustmentReason reason,
        DateOnly adjustmentDate,
        Guid userId,
        string? reasonNote = null)
    {
        ValidateUserId(userId);
        ValidateActiveStatus();

        var newQuantity = Quantity + adjustmentQuantity;
        if (newQuantity < 0)
            throw new InvalidOperationException($"Adjustment would result in negative quantity: {newQuantity}");

        var adjustment = PackageAdjustment.Create(
            Id,
            adjustmentQuantity,
            UnitOfMeasure,
            reason,
            adjustmentDate,
            userId,
            reasonNote);

        _adjustments.Add(adjustment);
        Quantity = newQuantity;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;

        return adjustment;
    }

    /// <summary>
    /// Record remediation for the package
    /// </summary>
    public PackageRemediation Remediate(
        string remediationMethod,
        DateOnly remediationDate,
        Guid userId,
        string? remediationSteps = null)
    {
        ValidateUserId(userId);
        ValidateActiveStatus();

        if (string.IsNullOrWhiteSpace(remediationMethod))
            throw new ArgumentException("Remediation method is required", nameof(remediationMethod));

        var remediation = PackageRemediation.Create(
            Id,
            remediationMethod,
            remediationDate,
            userId,
            remediationSteps);

        _remediations.Add(remediation);
        ProductRequiresRemediation = false;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;

        return remediation;
    }

    /// <summary>
    /// Update package location
    /// </summary>
    public void UpdateLocation(Guid? locationId, string? locationName, string? sublocationName, Guid userId)
    {
        ValidateUserId(userId);
        ValidateActiveStatus();

        LocationId = locationId;
        LocationName = locationName?.Trim();
        SublocationName = sublocationName?.Trim();
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update item information
    /// </summary>
    public void UpdateItem(Guid itemId, string itemName, Guid userId)
    {
        ValidateUserId(userId);
        ValidateActiveStatus();

        if (itemId == Guid.Empty)
            throw new ArgumentException("Item ID cannot be empty", nameof(itemId));

        if (string.IsNullOrWhiteSpace(itemName))
            throw new ArgumentException("Item name cannot be empty", nameof(itemName));

        ItemId = itemId;
        ItemName = itemName.Trim();
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update lab testing state
    /// </summary>
    public void UpdateLabTestingState(LabTestingState state, Guid userId)
    {
        ValidateUserId(userId);

        LabTestingState = state;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update potency information from lab results
    /// </summary>
    public void UpdatePotency(
        decimal? thcPercent,
        decimal? thcContent,
        string? thcContentUom,
        decimal? cbdPercent,
        decimal? cbdContent,
        Guid userId)
    {
        ValidateUserId(userId);

        ThcPercent = thcPercent;
        ThcContent = thcContent;
        ThcContentUnitOfMeasure = thcContentUom?.Trim();
        CbdPercent = cbdPercent;
        CbdContent = cbdContent;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set use by date
    /// </summary>
    public void SetUseByDate(DateOnly useByDate, Guid userId)
    {
        ValidateUserId(userId);

        UseByDate = useByDate;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set expiration date
    /// </summary>
    public void SetExpirationDate(DateOnly expirationDate, Guid userId)
    {
        ValidateUserId(userId);

        ExpirationDate = expirationDate;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Flag as trade sample
    /// </summary>
    public void FlagAsTradeSample(Guid userId)
    {
        ValidateUserId(userId);
        ValidateActiveStatus();

        IsTradeSample = true;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Unflag as trade sample
    /// </summary>
    public void UnflagAsTradeSample(Guid userId)
    {
        ValidateUserId(userId);
        ValidateActiveStatus();

        IsTradeSample = false;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Flag as donation
    /// </summary>
    public void FlagAsDonation(Guid userId)
    {
        ValidateUserId(userId);
        ValidateActiveStatus();

        IsDonation = true;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Unflag as donation
    /// </summary>
    public void UnflagAsDonation(Guid userId)
    {
        ValidateUserId(userId);
        ValidateActiveStatus();

        IsDonation = false;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark package as requiring remediation
    /// </summary>
    public void MarkRequiresRemediation(Guid userId)
    {
        ValidateUserId(userId);

        ProductRequiresRemediation = true;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Place package on hold
    /// </summary>
    public void PlaceOnHold(string reason, Guid userId)
    {
        ValidateUserId(userId);

        if (Status != PackageStatus.Active)
            throw new InvalidOperationException($"Cannot place package on hold with status {Status}");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Hold reason is required", nameof(reason));

        Status = PackageStatus.OnHold;
        AddNote($"Placed on hold: {reason}", userId);
    }

    /// <summary>
    /// Release package from hold
    /// </summary>
    public void ReleaseFromHold(Guid userId)
    {
        ValidateUserId(userId);

        if (Status != PackageStatus.OnHold)
            throw new InvalidOperationException("Package is not on hold");

        Status = PackageStatus.Active;
        AddNote("Released from hold", userId);
    }

    /// <summary>
    /// Finish/complete the package
    /// </summary>
    public void Finish(DateOnly finishDate, Guid userId)
    {
        ValidateUserId(userId);

        if (Status == PackageStatus.Finished)
            throw new InvalidOperationException("Package is already finished");

        Status = PackageStatus.Finished;
        FinishedDate = finishDate;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Unfinish/reopen the package
    /// </summary>
    public void Unfinish(Guid userId)
    {
        ValidateUserId(userId);

        if (Status != PackageStatus.Finished)
            throw new InvalidOperationException("Package is not finished");

        Status = PackageStatus.Active;
        FinishedDate = null;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update METRC sync information
    /// </summary>
    public void UpdateMetrcSync(long metrcPackageId, string? syncStatus = null)
    {
        MetrcPackageId = metrcPackageId;
        MetrcLastSyncAt = DateTime.UtcNow;
        MetrcSyncStatus = syncStatus ?? "Synced";
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Add notes to the package
    /// </summary>
    public void AddNote(string note, Guid userId)
    {
        ValidateUserId(userId);

        if (string.IsNullOrWhiteSpace(note))
            throw new ArgumentException("Note cannot be empty", nameof(note));

        Notes = string.IsNullOrWhiteSpace(Notes)
            ? note.Trim()
            : $"{Notes}\n\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm}] {note.Trim()}";
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set adjustments from persistence
    /// </summary>
    public void SetAdjustments(IEnumerable<PackageAdjustment> adjustments)
    {
        _adjustments.Clear();
        if (adjustments != null)
            _adjustments.AddRange(adjustments);
    }

    /// <summary>
    /// Set remediations from persistence
    /// </summary>
    public void SetRemediations(IEnumerable<PackageRemediation> remediations)
    {
        _remediations.Clear();
        if (remediations != null)
            _remediations.AddRange(remediations);
    }

    /// <summary>
    /// Set source package labels from persistence
    /// </summary>
    public void SetSourcePackageLabels(IEnumerable<string> labels)
    {
        _sourcePackageLabels.Clear();
        if (labels != null)
            _sourcePackageLabels.AddRange(labels);
    }

    private void ValidateActiveStatus()
    {
        if (Status == PackageStatus.Finished)
            throw new InvalidOperationException("Cannot modify a finished package");
    }

    private static void ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
    }

    private static void ValidateConstructorArgs(
        Guid siteId,
        PackageLabel packageLabel,
        Guid itemId,
        string itemName,
        decimal quantity,
        string unitOfMeasure,
        Guid createdByUserId)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));

        if (packageLabel == null)
            throw new ArgumentNullException(nameof(packageLabel));

        if (itemId == Guid.Empty)
            throw new ArgumentException("Item ID cannot be empty", nameof(itemId));

        if (string.IsNullOrWhiteSpace(itemName))
            throw new ArgumentException("Item name cannot be empty", nameof(itemName));

        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(quantity));

        if (string.IsNullOrWhiteSpace(unitOfMeasure))
            throw new ArgumentException("Unit of measure cannot be empty", nameof(unitOfMeasure));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty", nameof(createdByUserId));
    }
}



