using System;
using Harvestry.Spatial.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Spatial.Domain.Entities;

/// <summary>
/// Equipment calibration record with multi-point support
/// Tracks calibration history, deviations, and next due dates
/// </summary>
public partial class Calibration : Entity<Guid>
{
    private Calibration(Guid id) : base(id) { } // EF Core

    private Calibration(
        Guid id,
        Guid equipmentId,
        CalibrationMethod method,
        decimal referenceValue,
        decimal measuredValue,
        CalibrationResult result,
        Guid performedByUserId,
        string? channelCode = null,
        string? coefficientsJson = null,
        string? firmwareVersionAtCalibration = null) : base(id)
    {
        if (equipmentId == Guid.Empty)
            throw new ArgumentException("Equipment ID cannot be empty", nameof(equipmentId));
        
        if (performedByUserId == Guid.Empty)
            throw new ArgumentException("Performed by user ID cannot be empty", nameof(performedByUserId));
        
        EquipmentId = equipmentId;
        ChannelCode = channelCode?.Trim();
        Method = method;
        ReferenceValue = referenceValue;
        MeasuredValue = measuredValue;
        Result = result;
        PerformedByUserId = performedByUserId;
        CoefficientsJson = coefficientsJson?.Trim();
        FirmwareVersionAtCalibration = firmwareVersionAtCalibration?.Trim();
        PerformedAt = DateTime.UtcNow;
        
        // Calculate deviation
        Deviation = Math.Abs(measuredValue - referenceValue);
        
        // Calculate deviation percentage (avoid divide by zero)
        if (referenceValue != 0)
        {
            DeviationPct = (Deviation / Math.Abs(referenceValue)) * 100;
        }
        else
        {
            DeviationPct = 0;
        }
    }

    public Guid EquipmentId { get; private set; }
    public string? ChannelCode { get; private set; }
    
    // Calibration details
    public CalibrationMethod Method { get; private set; }
    public decimal ReferenceValue { get; private set; }
    public decimal MeasuredValue { get; private set; }
    
    // Calibration coefficients (for multi-point or correction curves)
    public string? CoefficientsJson { get; private set; }
    
    // Result
    public CalibrationResult Result { get; private set; }
    public decimal Deviation { get; private set; }
    public decimal DeviationPct { get; private set; }
    
    // Metadata
    public DateTime PerformedAt { get; private set; }
    public Guid PerformedByUserId { get; private set; }
    public DateTime? NextDueAt { get; private set; }
    
    // Documentation
    public string? Notes { get; private set; }
    public string? AttachmentUrl { get; private set; }
    
    // Equipment state at time of calibration
    public string? FirmwareVersionAtCalibration { get; private set; }

    /// <summary>
    /// Sets the next calibration due date
    /// </summary>
    public void SetNextDueDate(DateTime nextDueAt)
    {
        if (nextDueAt <= PerformedAt)
            throw new ArgumentException("Next due date must be after calibration date", nameof(nextDueAt));
        
        NextDueAt = nextDueAt;
    }

    /// <summary>
    /// Adds notes to calibration record
    /// </summary>
    public void AddNotes(string notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            throw new ArgumentException("Notes cannot be empty", nameof(notes));
        
        Notes = notes.Trim();
    }

    /// <summary>
    /// Attaches documentation URL (certificate, report, etc.)
    /// </summary>
    public void AttachDocumentation(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be empty", nameof(url));
        
        AttachmentUrl = url.Trim();
    }

    /// <summary>
    /// Checks if calibration passed
    /// </summary>
    public bool Passed()
    {
        return Result == CalibrationResult.Pass || Result == CalibrationResult.WithinTolerance;
    }

    /// <summary>
    /// Checks if calibration failed
    /// </summary>
    public bool Failed()
    {
        return Result == CalibrationResult.Fail || Result == CalibrationResult.OutOfTolerance;
    }

    /// <summary>
    /// Gets a human-readable calibration summary
    /// </summary>
    public string GetSummary()
    {
        return $"{Method} calibration: Reference={ReferenceValue}, Measured={MeasuredValue}, " +
               $"Deviation={Deviation:F4} ({DeviationPct:F2}%), Result={Result}";
    }

    public Calibration(
        Guid equipmentId,
        CalibrationMethod method,
        decimal referenceValue,
        decimal measuredValue,
        CalibrationResult result,
        Guid performedByUserId,
        string? channelCode = null,
        string? coefficientsJson = null,
        string? firmwareVersionAtCalibration = null)
        : this(Guid.NewGuid(), equipmentId, method, referenceValue, measuredValue, result, performedByUserId, channelCode, coefficientsJson, firmwareVersionAtCalibration)
    {
    }
}
