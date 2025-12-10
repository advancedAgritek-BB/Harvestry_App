using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Harvests.Domain.Entities;

/// <summary>
/// Records calibration events for scale devices
/// </summary>
public sealed class ScaleCalibration : Entity<Guid>
{
    // Private constructor for EF Core
    private ScaleCalibration(Guid id) : base(id) { }

    private ScaleCalibration(
        Guid id,
        Guid scaleDeviceId,
        DateOnly calibrationDate,
        DateOnly calibrationDueDate,
        string calibrationType,
        bool passed,
        Guid recordedByUserId) : base(id)
    {
        ScaleDeviceId = scaleDeviceId;
        CalibrationDate = calibrationDate;
        CalibrationDueDate = calibrationDueDate;
        CalibrationType = calibrationType;
        Passed = passed;
        RecordedByUserId = recordedByUserId;
        RecordedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Scale device this calibration belongs to
    /// </summary>
    public Guid ScaleDeviceId { get; private set; }

    /// <summary>
    /// Date the calibration was performed
    /// </summary>
    public DateOnly CalibrationDate { get; private set; }

    /// <summary>
    /// Date when next calibration is due
    /// </summary>
    public DateOnly CalibrationDueDate { get; private set; }

    /// <summary>
    /// Type of calibration: internal, external, certified
    /// </summary>
    public string CalibrationType { get; private set; } = string.Empty;

    /// <summary>
    /// Name of person who performed the calibration
    /// </summary>
    public string? PerformedBy { get; private set; }

    /// <summary>
    /// Name of certifier (for certified calibrations)
    /// </summary>
    public string? CertifiedBy { get; private set; }

    /// <summary>
    /// Certification number
    /// </summary>
    public string? CertificationNumber { get; private set; }

    /// <summary>
    /// Name of calibration company (for external calibrations)
    /// </summary>
    public string? CalibrationCompany { get; private set; }

    /// <summary>
    /// JSON array of test weights used: [{nominal: 1000, actual: 999.98, measured: 999.97}]
    /// </summary>
    public string? TestWeightsUsedJson { get; private set; }

    /// <summary>
    /// Whether the calibration passed
    /// </summary>
    public bool Passed { get; private set; }

    /// <summary>
    /// Deviation from standard in grams
    /// </summary>
    public decimal? DeviationGrams { get; private set; }

    /// <summary>
    /// Deviation from standard as a percentage
    /// </summary>
    public decimal? DeviationPercent { get; private set; }

    /// <summary>
    /// Notes about the calibration
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// URL to the calibration certificate document
    /// </summary>
    public string? CertificateUrl { get; private set; }

    /// <summary>
    /// Document ID for the certificate in document storage
    /// </summary>
    public Guid? CertificateDocumentId { get; private set; }

    /// <summary>
    /// When this record was created
    /// </summary>
    public DateTime RecordedAt { get; private set; }

    /// <summary>
    /// User who recorded this calibration
    /// </summary>
    public Guid RecordedByUserId { get; private set; }

    /// <summary>
    /// Check if this calibration is currently valid
    /// </summary>
    public bool IsValid()
    {
        return Passed && CalibrationDueDate >= DateOnly.FromDateTime(DateTime.UtcNow);
    }

    /// <summary>
    /// Factory method to create a new calibration record
    /// </summary>
    public static ScaleCalibration Create(
        Guid scaleDeviceId,
        DateOnly calibrationDate,
        DateOnly calibrationDueDate,
        string calibrationType,
        bool passed,
        Guid recordedByUserId)
    {
        if (scaleDeviceId == Guid.Empty)
            throw new ArgumentException("Scale device ID cannot be empty", nameof(scaleDeviceId));

        if (string.IsNullOrWhiteSpace(calibrationType))
            throw new ArgumentException("Calibration type is required", nameof(calibrationType));

        if (recordedByUserId == Guid.Empty)
            throw new ArgumentException("Recorded by user ID cannot be empty", nameof(recordedByUserId));

        return new ScaleCalibration(
            Guid.NewGuid(),
            scaleDeviceId,
            calibrationDate,
            calibrationDueDate,
            calibrationType.Trim(),
            passed,
            recordedByUserId);
    }

    /// <summary>
    /// Set calibration details
    /// </summary>
    public void SetCalibrationDetails(
        string? performedBy,
        string? certifiedBy,
        string? certificationNumber,
        string? calibrationCompany,
        string? testWeightsUsedJson,
        decimal? deviationGrams,
        decimal? deviationPercent,
        string? notes)
    {
        PerformedBy = performedBy?.Trim();
        CertifiedBy = certifiedBy?.Trim();
        CertificationNumber = certificationNumber?.Trim();
        CalibrationCompany = calibrationCompany?.Trim();
        TestWeightsUsedJson = testWeightsUsedJson;
        DeviationGrams = deviationGrams;
        DeviationPercent = deviationPercent;
        Notes = notes?.Trim();
    }

    /// <summary>
    /// Set certificate information
    /// </summary>
    public void SetCertificate(string? certificateUrl, Guid? certificateDocumentId)
    {
        CertificateUrl = certificateUrl?.Trim();
        CertificateDocumentId = certificateDocumentId;
    }

    /// <summary>
    /// Restore from persistence
    /// </summary>
    public static ScaleCalibration Restore(
        Guid id,
        Guid scaleDeviceId,
        DateOnly calibrationDate,
        DateOnly calibrationDueDate,
        string calibrationType,
        string? performedBy,
        string? certifiedBy,
        string? certificationNumber,
        string? calibrationCompany,
        string? testWeightsUsedJson,
        bool passed,
        decimal? deviationGrams,
        decimal? deviationPercent,
        string? notes,
        string? certificateUrl,
        Guid? certificateDocumentId,
        DateTime recordedAt,
        Guid recordedByUserId)
    {
        return new ScaleCalibration(id)
        {
            ScaleDeviceId = scaleDeviceId,
            CalibrationDate = calibrationDate,
            CalibrationDueDate = calibrationDueDate,
            CalibrationType = calibrationType,
            PerformedBy = performedBy,
            CertifiedBy = certifiedBy,
            CertificationNumber = certificationNumber,
            CalibrationCompany = calibrationCompany,
            TestWeightsUsedJson = testWeightsUsedJson,
            Passed = passed,
            DeviationGrams = deviationGrams,
            DeviationPercent = deviationPercent,
            Notes = notes,
            CertificateUrl = certificateUrl,
            CertificateDocumentId = certificateDocumentId,
            RecordedAt = recordedAt,
            RecordedByUserId = recordedByUserId
        };
    }
}




