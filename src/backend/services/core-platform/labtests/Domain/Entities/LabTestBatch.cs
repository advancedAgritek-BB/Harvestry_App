using Harvestry.LabTests.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.LabTests.Domain.Entities;

/// <summary>
/// Lab test batch aggregate root - represents a set of lab tests for a sample
/// </summary>
public sealed partial class LabTestBatch : AggregateRoot<Guid>
{
    private readonly List<LabTestResult> _results = new();

    // Private constructor for EF Core
    private LabTestBatch(Guid id) : base(id) { }

    private LabTestBatch(
        Guid id,
        Guid siteId,
        string packageLabel,
        string labFacilityLicenseNumber,
        string labFacilityName,
        Guid createdByUserId) : base(id)
    {
        ValidateConstructorArgs(siteId, packageLabel, labFacilityLicenseNumber, labFacilityName, createdByUserId);

        SiteId = siteId;
        PackageLabel = packageLabel.Trim().ToUpperInvariant();
        LabFacilityLicenseNumber = labFacilityLicenseNumber.Trim();
        LabFacilityName = labFacilityName.Trim();
        Status = LabTestStatus.Pending;
        CreatedByUserId = createdByUserId;
        UpdatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Core identification
    public Guid SiteId { get; private set; }
    public string PackageLabel { get; private set; } = string.Empty;

    // Lab facility information
    public string LabFacilityLicenseNumber { get; private set; } = string.Empty;
    public string LabFacilityName { get; private set; } = string.Empty;

    // Dates
    public DateOnly? CollectedDate { get; private set; }
    public DateOnly? ReceivedDate { get; private set; }
    public DateOnly? TestCompletedDate { get; private set; }

    // Status
    public LabTestStatus Status { get; private set; }
    public string? Notes { get; private set; }

    // METRC sync tracking
    public long? MetrcLabTestId { get; private set; }
    public DateTime? MetrcLastSyncAt { get; private set; }
    public string? MetrcSyncStatus { get; private set; }

    // Document reference
    public string? DocumentUrl { get; private set; }
    public string? DocumentFileName { get; private set; }

    // Metadata
    public Dictionary<string, object> Metadata { get; private set; } = new();

    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    // Navigation
    public IReadOnlyCollection<LabTestResult> Results => _results.AsReadOnly();

    /// <summary>
    /// Factory method to create a new lab test batch
    /// </summary>
    public static LabTestBatch Create(
        Guid siteId,
        string packageLabel,
        string labFacilityLicenseNumber,
        string labFacilityName,
        Guid createdByUserId)
    {
        return new LabTestBatch(
            Guid.NewGuid(),
            siteId,
            packageLabel,
            labFacilityLicenseNumber,
            labFacilityName,
            createdByUserId);
    }

    /// <summary>
    /// Record sample collection date
    /// </summary>
    public void RecordCollected(DateOnly collectedDate, Guid userId)
    {
        ValidateUserId(userId);

        CollectedDate = collectedDate;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Record sample received by lab
    /// </summary>
    public void RecordReceived(DateOnly receivedDate, Guid userId)
    {
        ValidateUserId(userId);

        ReceivedDate = receivedDate;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Add a test result
    /// </summary>
    public LabTestResult AddResult(
        LabTestType testType,
        string testTypeName,
        bool passed,
        Guid userId,
        string? analyteName = null,
        decimal? resultValue = null,
        string? resultUnit = null,
        decimal? limitValue = null,
        string? notes = null)
    {
        ValidateUserId(userId);

        var result = LabTestResult.Create(
            Id,
            testType,
            testTypeName,
            passed,
            analyteName,
            resultValue,
            resultUnit,
            limitValue,
            notes);

        _results.Add(result);
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;

        // Update overall status based on results
        UpdateOverallStatus();

        return result;
    }

    /// <summary>
    /// Complete the test batch
    /// </summary>
    public void Complete(DateOnly completedDate, LabTestStatus overallStatus, Guid userId)
    {
        ValidateUserId(userId);

        TestCompletedDate = completedDate;
        Status = overallStatus;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Release test results (make available to licensee)
    /// </summary>
    public void ReleaseResults(Guid userId)
    {
        ValidateUserId(userId);

        if (Status == LabTestStatus.Pending)
            throw new InvalidOperationException("Cannot release results for pending tests");

        AddNote("Results released", userId);
    }

    /// <summary>
    /// Attach test document
    /// </summary>
    public void AttachDocument(string documentUrl, string fileName, Guid userId)
    {
        ValidateUserId(userId);

        if (string.IsNullOrWhiteSpace(documentUrl))
            throw new ArgumentException("Document URL cannot be empty", nameof(documentUrl));

        DocumentUrl = documentUrl.Trim();
        DocumentFileName = fileName?.Trim();
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update METRC sync information
    /// </summary>
    public void UpdateMetrcSync(long metrcLabTestId, string? syncStatus = null)
    {
        MetrcLabTestId = metrcLabTestId;
        MetrcLastSyncAt = DateTime.UtcNow;
        MetrcSyncStatus = syncStatus ?? "Synced";
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Add notes
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
    /// Set results from persistence
    /// </summary>
    public void SetResults(IEnumerable<LabTestResult> results)
    {
        _results.Clear();
        if (results != null)
            _results.AddRange(results);
    }

    private void UpdateOverallStatus()
    {
        if (!_results.Any())
        {
            Status = LabTestStatus.Pending;
            return;
        }

        if (_results.All(r => r.Passed))
        {
            Status = LabTestStatus.Passed;
        }
        else if (_results.Any(r => !r.Passed))
        {
            Status = LabTestStatus.Failed;
        }
    }

    private static void ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
    }

    private static void ValidateConstructorArgs(
        Guid siteId,
        string packageLabel,
        string labFacilityLicenseNumber,
        string labFacilityName,
        Guid createdByUserId)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));

        if (string.IsNullOrWhiteSpace(packageLabel))
            throw new ArgumentException("Package label cannot be empty", nameof(packageLabel));

        if (string.IsNullOrWhiteSpace(labFacilityLicenseNumber))
            throw new ArgumentException("Lab facility license number cannot be empty", nameof(labFacilityLicenseNumber));

        if (string.IsNullOrWhiteSpace(labFacilityName))
            throw new ArgumentException("Lab facility name cannot be empty", nameof(labFacilityName));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty", nameof(createdByUserId));
    }
}









