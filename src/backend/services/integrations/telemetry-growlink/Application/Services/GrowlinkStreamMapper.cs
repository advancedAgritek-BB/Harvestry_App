using Harvestry.Integration.Growlink.Application.Interfaces;
using Harvestry.Integration.Growlink.Domain.Entities;
using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Domain.Entities;
using Harvestry.Telemetry.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Harvestry.Integration.Growlink.Application.Services;

/// <summary>
/// Maps Growlink sensors to Harvestry SensorStreams with auto-creation support.
/// </summary>
public sealed class GrowlinkStreamMapper : IGrowlinkStreamMapper
{
    private readonly IGrowlinkStreamMappingRepository _mappingRepository;
    private readonly ISensorStreamRepository _sensorStreamRepository;
    private readonly ILogger<GrowlinkStreamMapper> _logger;

    // Maps Growlink sensor types to Harvestry StreamType
    private static readonly Dictionary<string, StreamType> SensorTypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["temperature"] = StreamType.Temperature,
        ["temp"] = StreamType.Temperature,
        ["air_temp"] = StreamType.Temperature,
        ["humidity"] = StreamType.Humidity,
        ["rh"] = StreamType.Humidity,
        ["relative_humidity"] = StreamType.Humidity,
        ["co2"] = StreamType.Co2,
        ["carbon_dioxide"] = StreamType.Co2,
        ["vpd"] = StreamType.Vpd,
        ["vapor_pressure_deficit"] = StreamType.Vpd,
        ["par"] = StreamType.LightPar,
        ["ppfd"] = StreamType.LightPpfd,
        ["light"] = StreamType.LightPar,
        ["ec"] = StreamType.Ec,
        ["electrical_conductivity"] = StreamType.Ec,
        ["ph"] = StreamType.Ph,
        ["dissolved_oxygen"] = StreamType.DissolvedOxygen,
        ["do"] = StreamType.DissolvedOxygen,
        ["water_temp"] = StreamType.WaterTemp,
        ["water_level"] = StreamType.WaterLevel,
        ["soil_moisture"] = StreamType.SoilMoisture,
        ["substrate_moisture"] = StreamType.SoilMoisture,
        ["vwc"] = StreamType.SoilMoisture,
        ["soil_temp"] = StreamType.SoilTemp,
        ["substrate_temp"] = StreamType.SoilTemp,
        ["soil_ec"] = StreamType.SoilEc,
        ["substrate_ec"] = StreamType.SoilEc,
        ["pressure"] = StreamType.Pressure,
        ["flow_rate"] = StreamType.FlowRate,
        ["flow"] = StreamType.FlowRate,
        ["power"] = StreamType.PowerConsumption,
        ["energy"] = StreamType.EnergyConsumption,
        ["airflow"] = StreamType.Airflow
    };

    // Maps Growlink units to Harvestry Unit
    private static readonly Dictionary<string, Unit> UnitMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["°f"] = Unit.DegreesFahrenheit,
        ["°c"] = Unit.DegreesCelsius,
        ["f"] = Unit.DegreesFahrenheit,
        ["c"] = Unit.DegreesCelsius,
        ["%"] = Unit.Percent,
        ["percent"] = Unit.Percent,
        ["ppm"] = Unit.PartsPerMillion,
        ["kpa"] = Unit.Kilopascals,
        ["ms/cm"] = Unit.MillisiemensPerCm,
        ["ec"] = Unit.MillisiemensPerCm,
        ["ph"] = Unit.Ph,
        ["umol/m²/s"] = Unit.Micromoles,
        ["µmol"] = Unit.Micromoles,
        ["lux"] = Unit.Lux,
        ["gpm"] = Unit.GallonsPerMinute,
        ["lpm"] = Unit.LitersPerMinute,
        ["w"] = Unit.Watts,
        ["kw"] = Unit.Kilowatts,
        ["kwh"] = Unit.KilowattHours
    };

    public GrowlinkStreamMapper(
        IGrowlinkStreamMappingRepository mappingRepository,
        ISensorStreamRepository sensorStreamRepository,
        ILogger<GrowlinkStreamMapper> logger)
    {
        _mappingRepository = mappingRepository ?? throw new ArgumentNullException(nameof(mappingRepository));
        _sensorStreamRepository = sensorStreamRepository ?? throw new ArgumentNullException(nameof(sensorStreamRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Guid?> GetHarvestryStreamIdAsync(
        Guid siteId,
        string growlinkDeviceId,
        string growlinkSensorId,
        string growlinkSensorName,
        string growlinkSensorType,
        bool autoCreate,
        CancellationToken cancellationToken = default)
    {
        // Check for existing mapping
        var mapping = await _mappingRepository.GetByGrowlinkSensorAsync(
            siteId,
            growlinkDeviceId,
            growlinkSensorId,
            cancellationToken);

        if (mapping != null && mapping.IsActive)
        {
            return mapping.HarvestryStreamId;
        }

        if (!autoCreate)
        {
            _logger.LogDebug(
                "No mapping found for Growlink sensor {DeviceId}:{SensorId} and auto-create disabled",
                growlinkDeviceId, growlinkSensorId);
            return null;
        }

        // Auto-create sensor stream and mapping
        return await CreateAutoMappingAsync(
            siteId,
            growlinkDeviceId,
            growlinkSensorId,
            growlinkSensorName,
            growlinkSensorType,
            cancellationToken);
    }

    public async Task<Dictionary<string, Guid>> GetMappingsAsync(
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var mappings = await _mappingRepository.GetBySiteIdAsync(siteId, cancellationToken);

        return mappings
            .Where(m => m.IsActive)
            .ToDictionary(
                m => m.GetGrowlinkSensorKey(),
                m => m.HarvestryStreamId);
    }

    public async Task<GrowlinkStreamMapping> CreateMappingAsync(
        Guid siteId,
        string growlinkDeviceId,
        string growlinkSensorId,
        string growlinkSensorName,
        string growlinkSensorType,
        Guid harvestryStreamId,
        CancellationToken cancellationToken = default)
    {
        var mapping = GrowlinkStreamMapping.CreateManual(
            siteId,
            growlinkDeviceId,
            growlinkSensorId,
            growlinkSensorName,
            growlinkSensorType,
            harvestryStreamId);

        await _mappingRepository.CreateAsync(mapping, cancellationToken);

        _logger.LogInformation(
            "Created manual Growlink mapping: {DeviceId}:{SensorId} -> {StreamId}",
            growlinkDeviceId, growlinkSensorId, harvestryStreamId);

        return mapping;
    }

    public async Task DeleteMappingAsync(
        Guid mappingId,
        CancellationToken cancellationToken = default)
    {
        await _mappingRepository.DeleteAsync(mappingId, cancellationToken);
        _logger.LogInformation("Deleted Growlink mapping: {MappingId}", mappingId);
    }

    private async Task<Guid> CreateAutoMappingAsync(
        Guid siteId,
        string growlinkDeviceId,
        string growlinkSensorId,
        string growlinkSensorName,
        string growlinkSensorType,
        CancellationToken cancellationToken)
    {
        // Determine stream type and unit from Growlink sensor type
        var streamType = MapSensorType(growlinkSensorType);
        var unit = GetDefaultUnit(streamType);

        // Create a virtual equipment ID for this Growlink device
        var equipmentId = DeriveEquipmentId(siteId, growlinkDeviceId);

        // Create the sensor stream
        var displayName = $"Growlink: {growlinkSensorName}";
        var sensorStream = SensorStream.Create(
            siteId,
            equipmentId,
            streamType,
            unit,
            displayName);

        await _sensorStreamRepository.CreateAsync(sensorStream, cancellationToken);

        // Create the mapping
        var mapping = GrowlinkStreamMapping.CreateAuto(
            siteId,
            growlinkDeviceId,
            growlinkSensorId,
            growlinkSensorName,
            growlinkSensorType,
            sensorStream.Id);

        await _mappingRepository.CreateAsync(mapping, cancellationToken);

        _logger.LogInformation(
            "Auto-created Growlink mapping: {DeviceId}:{SensorId} -> {StreamId} ({StreamType})",
            growlinkDeviceId, growlinkSensorId, sensorStream.Id, streamType);

        return sensorStream.Id;
    }

    private static StreamType MapSensorType(string growlinkSensorType)
    {
        if (string.IsNullOrWhiteSpace(growlinkSensorType))
            return StreamType.Custom;

        // Try exact match first
        if (SensorTypeMap.TryGetValue(growlinkSensorType, out var streamType))
            return streamType;

        // Try partial match
        var lowerType = growlinkSensorType.ToLowerInvariant();
        foreach (var (key, value) in SensorTypeMap)
        {
            if (lowerType.Contains(key))
                return value;
        }

        return StreamType.Custom;
    }

    private static Unit GetDefaultUnit(StreamType streamType)
    {
        return streamType switch
        {
            StreamType.Temperature => Unit.DegreesFahrenheit,
            StreamType.Humidity => Unit.Percent,
            StreamType.Co2 => Unit.PartsPerMillion,
            StreamType.Vpd => Unit.Kilopascals,
            StreamType.LightPar or StreamType.LightPpfd => Unit.Micromoles,
            StreamType.Ec or StreamType.SoilEc => Unit.MillisiemensPerCm,
            StreamType.Ph => Unit.Ph,
            StreamType.WaterTemp or StreamType.SoilTemp => Unit.DegreesFahrenheit,
            StreamType.SoilMoisture => Unit.Percent,
            StreamType.FlowRate => Unit.GallonsPerMinute,
            StreamType.PowerConsumption => Unit.Watts,
            StreamType.EnergyConsumption => Unit.KilowattHours,
            _ => Unit.Count
        };
    }

    /// <summary>
    /// Derives a deterministic equipment ID from site and Growlink device.
    /// </summary>
    private static Guid DeriveEquipmentId(Guid siteId, string growlinkDeviceId)
    {
        // Create a deterministic GUID based on site and device ID
        var input = $"{siteId}:growlink:{growlinkDeviceId}";
        var hash = System.Security.Cryptography.MD5.HashData(
            System.Text.Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}




