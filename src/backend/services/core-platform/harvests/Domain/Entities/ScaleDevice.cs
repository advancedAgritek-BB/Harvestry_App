using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Harvests.Domain.Entities;

/// <summary>
/// Represents a scale device used for weighing in harvest operations
/// </summary>
public sealed class ScaleDevice : Entity<Guid>
{
    private readonly List<ScaleCalibration> _calibrations = new();

    // Private constructor for EF Core
    private ScaleDevice(Guid id) : base(id) { }

    private ScaleDevice(
        Guid id,
        Guid siteId,
        string deviceName,
        string? deviceSerialNumber,
        string? manufacturer,
        string? model,
        decimal? capacityGrams,
        decimal? readabilityGrams,
        string connectionType,
        Guid createdByUserId) : base(id)
    {
        SiteId = siteId;
        DeviceName = deviceName;
        DeviceSerialNumber = deviceSerialNumber;
        Manufacturer = manufacturer;
        Model = model;
        CapacityGrams = capacityGrams;
        ReadabilityGrams = readabilityGrams;
        ConnectionType = connectionType;
        IsActive = true;
        RequiresCalibration = true;
        CalibrationIntervalDays = 365;
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Site this scale belongs to
    /// </summary>
    public Guid SiteId { get; private set; }

    /// <summary>
    /// User-friendly name for the scale
    /// </summary>
    public string DeviceName { get; private set; } = string.Empty;

    /// <summary>
    /// Serial number of the scale
    /// </summary>
    public string? DeviceSerialNumber { get; private set; }

    /// <summary>
    /// Manufacturer (e.g., Ohaus, Mettler Toledo)
    /// </summary>
    public string? Manufacturer { get; private set; }

    /// <summary>
    /// Model name/number (e.g., Ranger 7000)
    /// </summary>
    public string? Model { get; private set; }

    /// <summary>
    /// Maximum capacity in grams
    /// </summary>
    public decimal? CapacityGrams { get; private set; }

    /// <summary>
    /// Readability/precision in grams (e.g., 0.1g)
    /// </summary>
    public decimal? ReadabilityGrams { get; private set; }

    /// <summary>
    /// Connection type: usb, serial, network, bluetooth
    /// </summary>
    public string ConnectionType { get; private set; } = "usb";

    /// <summary>
    /// JSON configuration for connection (port, IP, baud rate, etc.)
    /// </summary>
    public string? ConnectionConfigJson { get; private set; }

    /// <summary>
    /// Location where this scale is installed
    /// </summary>
    public Guid? LocationId { get; private set; }

    /// <summary>
    /// Location name for display
    /// </summary>
    public string? LocationName { get; private set; }

    /// <summary>
    /// Whether this scale is active and available for use
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Whether this scale requires periodic calibration
    /// </summary>
    public bool RequiresCalibration { get; private set; }

    /// <summary>
    /// Days between required calibrations
    /// </summary>
    public int CalibrationIntervalDays { get; private set; }

    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation
    public IReadOnlyCollection<ScaleCalibration> Calibrations => _calibrations.AsReadOnly();

    /// <summary>
    /// Get the most recent valid calibration
    /// </summary>
    public ScaleCalibration? GetCurrentCalibration()
    {
        return _calibrations
            .Where(c => c.Passed && c.CalibrationDueDate >= DateOnly.FromDateTime(DateTime.UtcNow))
            .OrderByDescending(c => c.CalibrationDate)
            .FirstOrDefault();
    }

    /// <summary>
    /// Check if the scale is currently in calibration
    /// </summary>
    public bool IsCalibrationValid()
    {
        if (!RequiresCalibration)
            return true;

        var currentCal = GetCurrentCalibration();
        return currentCal != null;
    }

    /// <summary>
    /// Factory method to create a new scale device
    /// </summary>
    public static ScaleDevice Create(
        Guid siteId,
        string deviceName,
        string? deviceSerialNumber,
        string? manufacturer,
        string? model,
        decimal? capacityGrams,
        decimal? readabilityGrams,
        string connectionType,
        Guid createdByUserId)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));

        if (string.IsNullOrWhiteSpace(deviceName))
            throw new ArgumentException("Device name cannot be empty", nameof(deviceName));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty", nameof(createdByUserId));

        return new ScaleDevice(
            Guid.NewGuid(),
            siteId,
            deviceName.Trim(),
            deviceSerialNumber?.Trim(),
            manufacturer?.Trim(),
            model?.Trim(),
            capacityGrams,
            readabilityGrams,
            connectionType?.Trim() ?? "usb",
            createdByUserId);
    }

    /// <summary>
    /// Update scale details
    /// </summary>
    public void Update(
        string deviceName,
        string? deviceSerialNumber,
        string? manufacturer,
        string? model,
        decimal? capacityGrams,
        decimal? readabilityGrams,
        string connectionType)
    {
        if (string.IsNullOrWhiteSpace(deviceName))
            throw new ArgumentException("Device name cannot be empty", nameof(deviceName));

        DeviceName = deviceName.Trim();
        DeviceSerialNumber = deviceSerialNumber?.Trim();
        Manufacturer = manufacturer?.Trim();
        Model = model?.Trim();
        CapacityGrams = capacityGrams;
        ReadabilityGrams = readabilityGrams;
        ConnectionType = connectionType?.Trim() ?? "usb";
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update connection configuration
    /// </summary>
    public void UpdateConnectionConfig(string? configJson)
    {
        ConnectionConfigJson = configJson;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update scale location
    /// </summary>
    public void UpdateLocation(Guid? locationId, string? locationName)
    {
        LocationId = locationId;
        LocationName = locationName?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update calibration settings
    /// </summary>
    public void UpdateCalibrationSettings(bool requiresCalibration, int intervalDays)
    {
        RequiresCalibration = requiresCalibration;
        CalibrationIntervalDays = intervalDays;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activate the scale
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivate the scale
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Add a calibration record
    /// </summary>
    public void AddCalibration(ScaleCalibration calibration)
    {
        if (calibration == null)
            throw new ArgumentNullException(nameof(calibration));

        if (calibration.ScaleDeviceId != Id)
            throw new ArgumentException("Calibration does not belong to this scale device");

        _calibrations.Add(calibration);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set calibrations from persistence
    /// </summary>
    public void SetCalibrations(IEnumerable<ScaleCalibration> calibrations)
    {
        _calibrations.Clear();
        if (calibrations != null)
            _calibrations.AddRange(calibrations);
    }

    /// <summary>
    /// Restore from persistence
    /// </summary>
    public static ScaleDevice Restore(
        Guid id,
        Guid siteId,
        string deviceName,
        string? deviceSerialNumber,
        string? manufacturer,
        string? model,
        decimal? capacityGrams,
        decimal? readabilityGrams,
        string connectionType,
        string? connectionConfigJson,
        Guid? locationId,
        string? locationName,
        bool isActive,
        bool requiresCalibration,
        int calibrationIntervalDays,
        DateTime createdAt,
        Guid createdByUserId,
        DateTime updatedAt)
    {
        return new ScaleDevice(id)
        {
            SiteId = siteId,
            DeviceName = deviceName,
            DeviceSerialNumber = deviceSerialNumber,
            Manufacturer = manufacturer,
            Model = model,
            CapacityGrams = capacityGrams,
            ReadabilityGrams = readabilityGrams,
            ConnectionType = connectionType,
            ConnectionConfigJson = connectionConfigJson,
            LocationId = locationId,
            LocationName = locationName,
            IsActive = isActive,
            RequiresCalibration = requiresCalibration,
            CalibrationIntervalDays = calibrationIntervalDays,
            CreatedAt = createdAt,
            CreatedByUserId = createdByUserId,
            UpdatedAt = updatedAt
        };
    }
}




