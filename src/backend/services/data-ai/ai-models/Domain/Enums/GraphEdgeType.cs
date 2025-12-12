namespace Harvestry.AiModels.Domain.Enums;

/// <summary>
/// Canonical edge types for the Harvestry knowledge graph.
/// Each edge connects two node types with a specific semantic relationship.
/// </summary>
public enum GraphEdgeType
{
    // === Traceability Edges ===
    
    /// <summary>Package moved from location (source)</summary>
    MovedFrom = 1,
    
    /// <summary>Package moved to location (destination)</summary>
    MovedTo = 2,
    
    /// <summary>Movement verified by user</summary>
    VerifiedBy = 3,
    
    /// <summary>Movement approved by user (first approval)</summary>
    ApprovedBy = 4,
    
    /// <summary>Movement approved by user (second approval for dual-control)</summary>
    SecondApprovedBy = 5,
    
    /// <summary>Package is part of batch movement</summary>
    PartOfBatchMovement = 6,
    
    /// <summary>Package derived from ancestor package (lineage)</summary>
    DerivedFrom = 7,
    
    /// <summary>Package created by user</summary>
    CreatedBy = 8,
    
    /// <summary>Movement involves package</summary>
    InvolvesPackage = 9,
    
    /// <summary>Package contains item type</summary>
    ContainsItem = 10,
    
    /// <summary>Movement part of sales order</summary>
    PartOfSalesOrder = 11,
    
    /// <summary>Movement part of transfer</summary>
    PartOfTransfer = 12,
    
    /// <summary>Movement part of processing job</summary>
    PartOfProcessingJob = 13,
    
    // === Work Graph Edges ===
    
    /// <summary>Task depends on another task</summary>
    DependsOn = 20,
    
    /// <summary>Task assigned to user</summary>
    AssignedTo = 21,
    
    /// <summary>Task assigned by user</summary>
    AssignedBy = 22,
    
    /// <summary>Time entry logged by user</summary>
    LoggedBy = 23,
    
    /// <summary>Time entry for task</summary>
    TimeEntryFor = 24,
    
    /// <summary>Task requires SOP completion</summary>
    RequiresSop = 25,
    
    /// <summary>Task watched by user</summary>
    WatchedBy = 26,
    
    /// <summary>User member of team</summary>
    MemberOf = 27,
    
    /// <summary>Task relates to entity (package, harvest, etc.)</summary>
    RelatesTo = 28,
    
    // === Telemetry & Irrigation Edges ===
    
    /// <summary>Irrigation run targets zone</summary>
    TargetsZone = 40,
    
    /// <summary>Zone has emitter configuration</summary>
    HasEmitters = 41,
    
    /// <summary>Sensor stream measures zone</summary>
    MeasuresZone = 42,
    
    /// <summary>Stream attached to equipment</summary>
    AttachedToEquipment = 43,
    
    /// <summary>Equipment located in zone</summary>
    LocatedIn = 44,
    
    /// <summary>Alert rule monitors streams</summary>
    MonitorsStream = 45,
    
    /// <summary>Alert instance fired for rule</summary>
    FiredForRule = 46,
    
    /// <summary>Alert instance triggered by stream</summary>
    TriggeredByStream = 47,
    
    /// <summary>Zone contained in room</summary>
    ContainedInRoom = 48,
    
    /// <summary>Device command sent to equipment</summary>
    CommandSentTo = 49,
    
    /// <summary>Irrigation run issued command</summary>
    IssuedCommand = 50,
    
    /// <summary>Equipment controls zone (e.g., valve controls irrigation)</summary>
    ControlsZone = 51,
    
    // === Genetics & Crop Steering Edges ===
    
    /// <summary>Strain has crop steering profile</summary>
    HasSteeringProfile = 60,
    
    /// <summary>Strain has response curve</summary>
    HasResponseCurve = 61,
    
    /// <summary>Zone grows strain</summary>
    GrowsStrain = 62,
    
    /// <summary>Harvest came from zone</summary>
    HarvestedFrom = 63,
    
    /// <summary>Harvest produced packages</summary>
    ProducedPackage = 64,
    
    /// <summary>Lab test batch for package</summary>
    TestedPackage = 65,
    
    /// <summary>Lab test result part of batch</summary>
    ResultInBatch = 66,
    
    /// <summary>Plant belongs to strain</summary>
    OfStrain = 67,
    
    /// <summary>Plant harvested into harvest batch</summary>
    HarvestedInto = 68,
    
    // === Organization Edges ===
    
    /// <summary>Entity belongs to site</summary>
    BelongsToSite = 80,
    
    /// <summary>User works at site</summary>
    WorksAt = 81
}
