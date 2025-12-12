namespace Harvestry.AiModels.Domain.Enums;

/// <summary>
/// Canonical node types for the Harvestry knowledge graph.
/// Each type maps to a specific domain entity across bounded contexts.
/// </summary>
public enum GraphNodeType
{
    // === Traceability & Chain-of-Custody Graph ===
    
    /// <summary>Cannabis package with label, quantity, and lineage</summary>
    Package = 1,
    
    /// <summary>Inventory movement event (transfer, adjustment, etc.)</summary>
    InventoryMovement = 2,
    
    /// <summary>Physical or logical location (room, zone, shelf)</summary>
    Location = 3,
    
    /// <summary>Inventory item type/SKU</summary>
    Item = 4,
    
    /// <summary>System user (employee, operator)</summary>
    User = 5,
    
    /// <summary>Sales order</summary>
    SalesOrder = 6,
    
    /// <summary>Outbound transfer manifest</summary>
    Transfer = 7,
    
    /// <summary>Processing job (extraction, packaging, etc.)</summary>
    ProcessingJob = 8,
    
    // === Work Graph ===
    
    /// <summary>Task/work order</summary>
    Task = 20,
    
    /// <summary>Time entry for labor tracking</summary>
    TimeEntry = 21,
    
    /// <summary>Standard operating procedure</summary>
    Sop = 22,
    
    /// <summary>Team of users</summary>
    Team = 23,
    
    // === Telemetry & Irrigation Graph ===
    
    /// <summary>Cultivation zone</summary>
    Zone = 40,
    
    /// <summary>Room containing zones</summary>
    Room = 41,
    
    /// <summary>Equipment (controller, sensor, valve)</summary>
    Equipment = 42,
    
    /// <summary>Sensor stream (VWC, EC, temp, etc.)</summary>
    SensorStream = 43,
    
    /// <summary>Irrigation run execution</summary>
    IrrigationRun = 44,
    
    /// <summary>Alert rule definition</summary>
    AlertRule = 45,
    
    /// <summary>Alert instance (fired alert)</summary>
    AlertInstance = 46,
    
    /// <summary>Zone emitter configuration</summary>
    ZoneEmitterConfig = 47,
    
    /// <summary>Device command sent to equipment</summary>
    DeviceCommand = 48,
    
    // === Genetics & Crop Steering Graph ===
    
    /// <summary>Cannabis strain</summary>
    Strain = 60,
    
    /// <summary>Crop steering profile</summary>
    CropSteeringProfile = 61,
    
    /// <summary>Cultivar response curve</summary>
    ResponseCurve = 62,
    
    /// <summary>Harvest batch</summary>
    Harvest = 63,
    
    /// <summary>Lab test batch</summary>
    LabTestBatch = 64,
    
    /// <summary>Lab test result</summary>
    LabTestResult = 65,
    
    /// <summary>Plant individual</summary>
    Plant = 66,
    
    // === Organization ===
    
    /// <summary>Site/facility</summary>
    Site = 80
}
