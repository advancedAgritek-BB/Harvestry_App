# FRP-02: Spatial Hierarchy & Equipment Registry - Implementation Plan

**Status:** 🎯 READY TO START  
**Estimated Effort:** 16-20 hours  
**Prerequisites:** ✅ FRP-01 Complete (Identity, RLS, ABAC)  
**Blocks:** FRP-05 (Telemetry), FRP-06 (Irrigation), FRP-07 (Inventory)

---

## 📋 OVERVIEW

### Purpose
Establish a site-scoped physical model (Site → Room → Zone → Rack → Bin) plus a first-class Vault location type. Provide an equipment registry (controllers, sensors, actuators, injectors, pumps/valves, meters, EC/pH controllers, mix tanks) with calibration logs and device health/fault tracking.

### Key Features
1. **Sites Management** - Organization, timezone, address, policies
2. **Spatial Hierarchy** - Rooms, Zones, Racks, Bins with RLS
3. **Vault Location** - Distinct compliance-focused location type
4. **Equipment Registry** - Controllers, sensors, actuators with mapping
5. **Calibration Tracking** - Logs and reminders
6. **Device Health** - Heartbeat monitoring and fault logging
7. **Valve-Zone Mapping** - Authoritative map for irrigation

### Acceptance Criteria (from PRD)
- ✅ Map valves to zones implemented and reportable
- ✅ Calibration & faults are logged and reportable
- ✅ RLS blocks cross-site access
- ✅ Equipment health dashboard operational

---

## 📊 IMPLEMENTATION BREAKDOWN

### Phase 1: Database Schema (3-4 hours)

#### Migration 1: Sites & Spatial Model
**File:** `src/database/migrations/frp02/20250930_01_CreateSpatialTables.sql`

**Tables:**
```sql
-- Sites (extends identity sites table if needed)
sites (
    id uuid PRIMARY KEY,
    org_id uuid NOT NULL,
    name varchar(200) NOT NULL,
    timezone varchar(100) NOT NULL,
    address_json jsonb,
    policies_json jsonb,
    created_at timestamptz,
    updated_at timestamptz,
    created_by uuid,
    updated_by uuid
)

-- Rooms (veg, flower, cure, processing, warehouse)
rooms (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    code varchar(50) NOT NULL,
    name varchar(200) NOT NULL,
    purpose varchar(50) CHECK (purpose IN ('veg', 'flower', 'cure', 'processing', 'warehouse', 'vault')),
    area_sqft decimal(10,2),
    status varchar(20) DEFAULT 'active',
    metadata jsonb,
    created_at timestamptz,
    updated_at timestamptz,
    UNIQUE(site_id, code)
)

-- Zones (irrigation, bench, aisle within rooms)
zones (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    room_id uuid NOT NULL REFERENCES rooms(id) ON DELETE RESTRICT,
    code varchar(50) NOT NULL,
    name varchar(200) NOT NULL,
    zone_class varchar(50) CHECK (zone_class IN ('irrigation', 'bench', 'aisle', 'other')),
    area_sqft decimal(10,2),
    capacity_plants int,
    status varchar(20) DEFAULT 'active',
    metadata jsonb,
    created_at timestamptz,
    updated_at timestamptz,
    UNIQUE(site_id, code)
)

-- Racks (within zones or rooms)
racks (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    room_id uuid REFERENCES rooms(id) ON DELETE RESTRICT,
    zone_id uuid REFERENCES zones(id) ON DELETE RESTRICT,
    code varchar(50) NOT NULL,
    name varchar(200) NOT NULL,
    capacity int,
    status varchar(20) DEFAULT 'active',
    metadata jsonb,
    created_at timestamptz,
    updated_at timestamptz,
    UNIQUE(site_id, code),
    CHECK (room_id IS NOT NULL OR zone_id IS NOT NULL)
)

-- Bins (within racks)
bins (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    rack_id uuid NOT NULL REFERENCES racks(id) ON DELETE RESTRICT,
    code varchar(50) NOT NULL,
    name varchar(200) NOT NULL,
    temp_controlled boolean DEFAULT FALSE,
    status varchar(20) DEFAULT 'active',
    metadata jsonb,
    created_at timestamptz,
    updated_at timestamptz,
    UNIQUE(site_id, code)
)

-- Unified Inventory Locations (includes Vault)
inventory_locations (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    type varchar(50) CHECK (type IN ('vault', 'room', 'zone', 'rack', 'bin', 'truck', 'customer', 'lab', 'staging')),
    parent_id uuid REFERENCES inventory_locations(id),
    external_ref varchar(100),
    label_template_id uuid,
    name varchar(200) NOT NULL,
    status varchar(20) DEFAULT 'active',
    metadata jsonb,
    created_at timestamptz,
    updated_at timestamptz,
    UNIQUE(site_id, type, external_ref)
)
```

**RLS Policies:**
```sql
-- Enable RLS on all tables
ALTER TABLE rooms ENABLE ROW LEVEL SECURITY;
ALTER TABLE zones ENABLE ROW LEVEL SECURITY;
ALTER TABLE racks ENABLE ROW LEVEL SECURITY;
ALTER TABLE bins ENABLE ROW LEVEL SECURITY;
ALTER TABLE inventory_locations ENABLE ROW LEVEL SECURITY;

-- Policy: Users can only access their site's spatial data
CREATE POLICY rooms_site_access ON rooms
    FOR SELECT
    USING (
        site_id::text = current_setting('app.site_id', TRUE)
        OR current_setting('app.user_role', TRUE) = 'admin'
        OR current_setting('app.user_role', TRUE) = 'service_account'
    );

-- Repeat for zones, racks, bins, inventory_locations
-- Include INSERT/UPDATE/DELETE policies with ABAC checks
```

**Indexes:**
```sql
CREATE INDEX idx_rooms_site_id ON rooms(site_id);
CREATE INDEX idx_zones_site_room ON zones(site_id, room_id);
CREATE INDEX idx_racks_site_zone ON racks(site_id, zone_id);
CREATE INDEX idx_bins_site_rack ON bins(site_id, rack_id);
CREATE INDEX idx_inventory_locations_site_type ON inventory_locations(site_id, type);
```

#### Migration 2: Equipment Registry
**File:** `src/database/migrations/frp02/20250930_02_CreateEquipmentTables.sql`

**Tables:**
```sql
-- Equipment (controllers, sensors, actuators, etc.)
equipment (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    type varchar(50) CHECK (type IN (
        'controller', 'sensor', 'actuator', 'injector', 
        'pump', 'valve', 'meter', 'ecph_controller', 
        'mix_tank', 'relay', 'flow_meter', 'pressure_sensor'
    )),
    vendor varchar(100),
    model varchar(100),
    protocol varchar(50) CHECK (protocol IN (
        'trolmaster', 'agrowtek', 'sdi12', 'modbus', 
        'mqtt', 'http', 'rs485', 'analog'
    )),
    serial varchar(100),
    mac_address varchar(50),
    firmware_version varchar(50),
    capabilities_json jsonb,
    heartbeat_at timestamptz,
    status varchar(20) CHECK (status IN ('online', 'offline', 'commissioning', 'fault', 'maintenance')),
    location_scope varchar(20) CHECK (location_scope IN ('site', 'room', 'zone', 'rack')),
    location_id uuid,
    metadata jsonb,
    created_at timestamptz,
    updated_at timestamptz,
    UNIQUE(site_id, serial)
)

-- Equipment Channels (for multi-channel devices like HSES12, HSEA24)
equipment_channels (
    id uuid PRIMARY KEY,
    equipment_id uuid NOT NULL REFERENCES equipment(id) ON DELETE CASCADE,
    channel_no varchar(20) NOT NULL,
    role varchar(50) CHECK (role IN (
        'substrate_ec', 'substrate_ph', 'vwc', 'air_temp', 'rh', 'co2', 
        'ppfd', 'drain_ec', 'pressure', 'flow', 'level', 'valve', 
        'pump', 'relay', 'estop', 'door'
    )),
    unit varchar(20),
    limits_json jsonb,
    linked_stream_id uuid,
    enabled boolean DEFAULT TRUE,
    metadata jsonb,
    created_at timestamptz,
    UNIQUE(equipment_id, channel_no)
)

-- Equipment Links (maps equipment to spatial entities)
equipment_links (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    equipment_id uuid NOT NULL REFERENCES equipment(id) ON DELETE CASCADE,
    target_type varchar(20) CHECK (target_type IN ('site', 'room', 'zone', 'rack', 'bin')),
    target_id uuid NOT NULL,
    purpose varchar(50) CHECK (purpose IN ('sensor_for', 'actuator_for', 'controller_for')),
    notes text,
    created_at timestamptz,
    UNIQUE(equipment_id, target_type, target_id, purpose)
)

-- Valve-Zone Map (authoritative for irrigation)
valve_zone_map (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    zone_id uuid NOT NULL REFERENCES zones(id) ON DELETE RESTRICT,
    equipment_id uuid NOT NULL REFERENCES equipment(id) ON DELETE RESTRICT,
    channel_no varchar(20) NOT NULL,
    priority int DEFAULT 1,
    enabled boolean DEFAULT TRUE,
    metadata jsonb,
    created_at timestamptz,
    updated_at timestamptz,
    UNIQUE(zone_id, equipment_id, channel_no)
)

-- Calibration Logs
equipment_calibrations (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    equipment_id uuid NOT NULL REFERENCES equipment(id) ON DELETE CASCADE,
    channel_id uuid REFERENCES equipment_channels(id) ON DELETE CASCADE,
    calibration_type varchar(50) CHECK (calibration_type IN (
        'two_point', 'single_point', 'offset', 'slope', 'factory_reset'
    )),
    performed_by uuid NOT NULL REFERENCES users(id),
    performed_at timestamptz NOT NULL,
    reference_value decimal(10,4),
    measured_value decimal(10,4),
    adjustment_value decimal(10,4),
    notes text,
    next_calibration_due_at timestamptz,
    metadata jsonb,
    created_at timestamptz
)

-- Equipment Faults
equipment_faults (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    equipment_id uuid NOT NULL REFERENCES equipment(id) ON DELETE CASCADE,
    channel_id uuid REFERENCES equipment_channels(id) ON DELETE CASCADE,
    fault_code varchar(50),
    fault_message text NOT NULL,
    severity varchar(20) CHECK (severity IN ('info', 'warning', 'error', 'critical')),
    detected_at timestamptz NOT NULL,
    acknowledged_at timestamptz,
    acknowledged_by uuid REFERENCES users(id),
    resolved_at timestamptz,
    resolved_by uuid REFERENCES users(id),
    resolution_notes text,
    metadata jsonb,
    created_at timestamptz
)
```

**RLS Policies:** (Similar pattern to spatial tables)

**Indexes:**
```sql
CREATE INDEX idx_equipment_site_type ON equipment(site_id, type);
CREATE INDEX idx_equipment_status ON equipment(status) WHERE status IN ('offline', 'fault');
CREATE INDEX idx_equipment_heartbeat ON equipment(heartbeat_at) WHERE status = 'online';
CREATE INDEX idx_equipment_channels_equipment ON equipment_channels(equipment_id);
CREATE INDEX idx_equipment_links_target ON equipment_links(target_type, target_id);
CREATE INDEX idx_valve_zone_map_zone ON valve_zone_map(zone_id) WHERE enabled = TRUE;
CREATE INDEX idx_calibrations_equipment ON equipment_calibrations(equipment_id);
CREATE INDEX idx_calibrations_due ON equipment_calibrations(next_calibration_due_at) WHERE next_calibration_due_at IS NOT NULL;
CREATE INDEX idx_faults_unresolved ON equipment_faults(equipment_id, detected_at) WHERE resolved_at IS NULL;
```

---

### Phase 2: Domain Layer (4-5 hours)

#### Domain Entities

**File Structure:**
```
src/backend/services/core-platform/spatial/Domain/
├── Entities/
│   ├── Site.cs
│   ├── Room.cs
│   ├── Zone.cs
│   ├── Rack.cs
│   ├── Bin.cs
│   ├── InventoryLocation.cs
│   ├── Equipment.cs
│   ├── EquipmentChannel.cs
│   ├── EquipmentLink.cs
│   ├── ValveZoneMapping.cs
│   ├── Calibration.cs
│   └── EquipmentFault.cs
├── ValueObjects/
│   ├── Address.cs
│   ├── Coordinates.cs
│   ├── CalibrationValue.cs
│   └── FaultCode.cs
└── Enums/
    ├── RoomPurpose.cs (Veg, Flower, Cure, Processing, Warehouse, Vault)
    ├── ZoneClass.cs (Irrigation, Bench, Aisle, Other)
    ├── EquipmentType.cs (Controller, Sensor, Actuator, etc.)
    ├── EquipmentProtocol.cs (TrolMaster, Agrowtek, SDI12, Modbus, MQTT)
    ├── EquipmentStatus.cs (Online, Offline, Commissioning, Fault, Maintenance)
    ├── ChannelRole.cs (SubstrateEC, SubstratePH, VWC, AirTemp, etc.)
    ├── CalibrationType.cs (TwoPoint, SinglePoint, Offset, Slope)
    └── FaultSeverity.cs (Info, Warning, Error, Critical)
```

**Key Domain Methods:**

**Room.cs:**
```csharp
public class Room : AggregateRoot<Guid>
{
    public Guid SiteId { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public RoomPurpose Purpose { get; private set; }
    public decimal? AreaSqft { get; private set; }
    public EntityStatus Status { get; private set; }
    
    // Methods
    public void UpdateDetails(string name, decimal? areaSqft);
    public void ChangePurpose(RoomPurpose newPurpose);
    public void Deactivate(string reason);
    public void Reactivate();
    public bool CanDelete(); // Check for zones/equipment
}
```

**Equipment.cs:**
```csharp
public class Equipment : AggregateRoot<Guid>
{
    public Guid SiteId { get; private set; }
    public EquipmentType Type { get; private set; }
    public string Vendor { get; private set; }
    public string Model { get; private set; }
    public EquipmentProtocol Protocol { get; private set; }
    public string Serial { get; private set; }
    public string MacAddress { get; private set; }
    public string FirmwareVersion { get; private set; }
    public DateTime? HeartbeatAt { get; private set; }
    public EquipmentStatus Status { get; private set; }
    
    private readonly List<EquipmentChannel> _channels = new();
    public IReadOnlyCollection<EquipmentChannel> Channels => _channels.AsReadOnly();
    
    // Methods
    public void RecordHeartbeat();
    public void UpdateFirmware(string version);
    public void MarkOffline(string reason);
    public void MarkOnline();
    public void ReportFault(FaultCode code, FaultSeverity severity, string message);
    public EquipmentChannel AddChannel(string channelNo, ChannelRole role, string unit);
    public void LinkToLocation(LocationScope scope, Guid locationId, LinkPurpose purpose);
}
```

**ValveZoneMapping.cs:**
```csharp
public class ValveZoneMapping : Entity<Guid>
{
    public Guid SiteId { get; private set; }
    public Guid ZoneId { get; private set; }
    public Guid EquipmentId { get; private set; }
    public string ChannelNo { get; private set; }
    public int Priority { get; private set; }
    public bool Enabled { get; private set; }
    
    // Methods
    public static ValveZoneMapping Create(Guid siteId, Guid zoneId, Guid equipmentId, string channelNo, int priority);
    public void Enable();
    public void Disable(string reason);
    public void UpdatePriority(int newPriority);
}
```

---

### Phase 3: Application Layer (3-4 hours)

#### Application Services

**Files:**
```
src/backend/services/core-platform/spatial/Application/
├── Services/
│   ├── SpatialManagementService.cs
│   ├── EquipmentRegistryService.cs
│   ├── CalibrationService.cs
│   └── DeviceHealthService.cs
├── DTOs/
│   ├── CreateRoomRequest.cs
│   ├── CreateZoneRequest.cs
│   ├── RegisterEquipmentRequest.cs
│   ├── CreateValveMappingRequest.cs
│   ├── RecordCalibrationRequest.cs
│   ├── EquipmentHealthResponse.cs
│   └── ValveZoneMappingResponse.cs
└── Interfaces/
    ├── ISpatialManagementService.cs
    ├── IEquipmentRegistryService.cs
    ├── ICalibrationService.cs
    └── IDeviceHealthService.cs
```

**Key Service Methods:**

**ISpatialManagementService:**
```csharp
public interface ISpatialManagementService
{
    Task<RoomResponse> CreateRoomAsync(CreateRoomRequest request, CancellationToken ct);
    Task<ZoneResponse> CreateZoneAsync(CreateZoneRequest request, CancellationToken ct);
    Task<RackResponse> CreateRackAsync(CreateRackRequest request, CancellationToken ct);
    Task<BinResponse> CreateBinAsync(CreateBinRequest request, CancellationToken ct);
    Task<IReadOnlyList<RoomResponse>> GetRoomsBySiteAsync(Guid siteId, CancellationToken ct);
    Task<SpatialHierarchyResponse> GetSpatialHierarchyAsync(Guid siteId, CancellationToken ct);
    Task<bool> CanDeleteRoomAsync(Guid roomId, CancellationToken ct);
}
```

**IEquipmentRegistryService:**
```csharp
public interface IEquipmentRegistryService
{
    Task<EquipmentResponse> RegisterEquipmentAsync(RegisterEquipmentRequest request, CancellationToken ct);
    Task<ValveZoneMappingResponse> MapValveToZoneAsync(CreateValveMappingRequest request, CancellationToken ct);
    Task<IReadOnlyList<EquipmentResponse>> GetEquipmentBySiteAsync(Guid siteId, EquipmentType? type, CancellationToken ct);
    Task<IReadOnlyList<ValveZoneMappingResponse>> GetValveMappingsBySiteAsync(Guid siteId, CancellationToken ct);
    Task RecordHeartbeatAsync(Guid equipmentId, CancellationToken ct);
}
```

**ICalibrationService:**
```csharp
public interface ICalibrationService
{
    Task<CalibrationResponse> RecordCalibrationAsync(RecordCalibrationRequest request, CancellationToken ct);
    Task<IReadOnlyList<CalibrationResponse>> GetCalibrationHistoryAsync(Guid equipmentId, CancellationToken ct);
    Task<IReadOnlyList<CalibrationDueResponse>> GetCalibrationsDueAsync(Guid siteId, int daysAhead, CancellationToken ct);
}
```

**IDeviceHealthService:**
```csharp
public interface IDeviceHealthService
{
    Task<EquipmentFaultResponse> ReportFaultAsync(ReportFaultRequest request, CancellationToken ct);
    Task AcknowledgeFaultAsync(Guid faultId, Guid userId, CancellationToken ct);
    Task ResolveFaultAsync(Guid faultId, Guid userId, string notes, CancellationToken ct);
    Task<IReadOnlyList<EquipmentFaultResponse>> GetActiveFaultsAsync(Guid siteId, CancellationToken ct);
    Task<DeviceHealthSummaryResponse> GetDeviceHealthSummaryAsync(Guid siteId, CancellationToken ct);
}
```

---

### Phase 4: Infrastructure Layer (3-4 hours)

#### Repositories

**Files:**
```
src/backend/services/core-platform/spatial/Infrastructure/Persistence/
├── SpatialDbContext.cs
├── RoomRepository.cs
├── ZoneRepository.cs
├── RackRepository.cs
├── BinRepository.cs
├── InventoryLocationRepository.cs
├── EquipmentRepository.cs
├── ValveZoneMappingRepository.cs
├── CalibrationRepository.cs
└── EquipmentFaultRepository.cs
```

**Key Repository Methods:**

**IEquipmentRepository:**
```csharp
public interface IEquipmentRepository : IRepository<Equipment, Guid>
{
    Task<Equipment?> GetBySerialAsync(string serial, Guid siteId, CancellationToken ct);
    Task<IReadOnlyList<Equipment>> GetBySiteAndTypeAsync(Guid siteId, EquipmentType type, CancellationToken ct);
    Task<IReadOnlyList<Equipment>> GetOfflineEquipmentAsync(Guid siteId, TimeSpan threshold, CancellationToken ct);
    Task UpdateHeartbeatAsync(Guid equipmentId, DateTime timestamp, CancellationToken ct);
}
```

**IValveZoneMappingRepository:**
```csharp
public interface IValveZoneMappingRepository : IRepository<ValveZoneMapping, Guid>
{
    Task<IReadOnlyList<ValveZoneMapping>> GetBySiteAsync(Guid siteId, CancellationToken ct);
    Task<IReadOnlyList<ValveZoneMapping>> GetByZoneAsync(Guid zoneId, CancellationToken ct);
    Task<ValveZoneMapping?> GetByZoneAndEquipmentAsync(Guid zoneId, Guid equipmentId, string channelNo, CancellationToken ct);
}
```

---

### Phase 5: API Layer (2-3 hours)

#### Controllers

**Files:**
```
src/backend/services/core-platform/spatial/API/Controllers/
├── RoomsController.cs
├── ZonesController.cs
├── RacksController.cs
├── BinsController.cs
├── EquipmentController.cs
├── ValveMappingsController.cs
└── CalibrationsController.cs
```

**Key Endpoints:**

**RoomsController.cs:**
```csharp
[ApiController]
[Route("api/sites/{siteId}/rooms")]
public class RoomsController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RoomResponse>> CreateRoom(Guid siteId, CreateRoomRequest request);
    
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoomResponse>>> GetRooms(Guid siteId);
    
    [HttpGet("{roomId}")]
    public async Task<ActionResult<RoomResponse>> GetRoom(Guid siteId, Guid roomId);
    
    [HttpPut("{roomId}")]
    public async Task<ActionResult<RoomResponse>> UpdateRoom(Guid siteId, Guid roomId, UpdateRoomRequest request);
    
    [HttpDelete("{roomId}")]
    public async Task<IActionResult> DeleteRoom(Guid siteId, Guid roomId);
}
```

**EquipmentController.cs:**
```csharp
[ApiController]
[Route("api/sites/{siteId}/equipment")]
public class EquipmentController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<EquipmentResponse>> RegisterEquipment(Guid siteId, RegisterEquipmentRequest request);
    
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EquipmentResponse>>> GetEquipment(Guid siteId, [FromQuery] EquipmentType? type);
    
    [HttpGet("{equipmentId}")]
    public async Task<ActionResult<EquipmentResponse>> GetEquipmentById(Guid siteId, Guid equipmentId);
    
    [HttpPost("{equipmentId}/heartbeat")]
    public async Task<IActionResult> RecordHeartbeat(Guid siteId, Guid equipmentId);
    
    [HttpGet("{equipmentId}/calibrations")]
    public async Task<ActionResult<IReadOnlyList<CalibrationResponse>>> GetCalibrations(Guid siteId, Guid equipmentId);
    
    [HttpPost("{equipmentId}/calibrations")]
    public async Task<ActionResult<CalibrationResponse>> RecordCalibration(Guid siteId, Guid equipmentId, RecordCalibrationRequest request);
    
    [HttpGet("{equipmentId}/faults")]
    public async Task<ActionResult<IReadOnlyList<EquipmentFaultResponse>>> GetFaults(Guid siteId, Guid equipmentId);
    
    [HttpPost("{equipmentId}/faults")]
    public async Task<ActionResult<EquipmentFaultResponse>> ReportFault(Guid siteId, Guid equipmentId, ReportFaultRequest request);
}
```

**ValveMappingsController.cs:**
```csharp
[ApiController]
[Route("api/sites/{siteId}/valve-mappings")]
public class ValveMappingsController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ValveZoneMappingResponse>> CreateMapping(Guid siteId, CreateValveMappingRequest request);
    
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ValveZoneMappingResponse>>> GetMappings(Guid siteId);
    
    [HttpGet("zones/{zoneId}")]
    public async Task<ActionResult<IReadOnlyList<ValveZoneMappingResponse>>> GetMappingsByZone(Guid siteId, Guid zoneId);
    
    [HttpPut("{mappingId}")]
    public async Task<ActionResult<ValveZoneMappingResponse>> UpdateMapping(Guid siteId, Guid mappingId, UpdateValveMappingRequest request);
    
    [HttpDelete("{mappingId}")]
    public async Task<IActionResult> DeleteMapping(Guid siteId, Guid mappingId);
}
```

---

### Phase 6: Validators (1 hour)

**Files:**
```
src/backend/services/core-platform/spatial/API/Validators/
├── CreateRoomRequestValidator.cs
├── CreateZoneRequestValidator.cs
├── RegisterEquipmentRequestValidator.cs
├── CreateValveMappingRequestValidator.cs
└── RecordCalibrationRequestValidator.cs
```

---

### Phase 7: Unit Tests (2-3 hours)

**Files:**
```
src/backend/services/core-platform/spatial/Tests/Unit/
├── Domain/
│   ├── RoomTests.cs
│   ├── ZoneTests.cs
│   ├── EquipmentTests.cs
│   └── ValveZoneMappingTests.cs
└── Services/
    ├── SpatialManagementServiceTests.cs
    ├── EquipmentRegistryServiceTests.cs
    └── CalibrationServiceTests.cs
```

**Test Scenarios:**
- Room/Zone/Rack/Bin creation with validation
- Equipment registration and heartbeat tracking
- Valve-zone mapping creation and validation
- Calibration recording and due date calculation
- Fault reporting and resolution workflow
- RLS policy enforcement

---

### Phase 8: Integration Tests (2-3 hours)

**Files:**
```
src/backend/services/core-platform/spatial/Tests/Integration/
├── SpatialHierarchyTests.cs
├── EquipmentRegistryTests.cs
├── ValveZoneMappingTests.cs
└── RlsFuzzTests.cs
```

**Test Scenarios:**
- Create room → zones → equipment → valve mapping (E2E)
- RLS: Cross-site room access blocked
- RLS: Cross-site equipment access blocked
- Valve mapping prevents irrigation start if missing
- Calibration due date triggers notifications
- Equipment offline detection

---

## 📊 TASK BREAKDOWN WITH ESTIMATES

| Phase | Task | Est. Hours | Owner |
|-------|------|------------|-------|
| **1. Database** | Migration 1: Spatial tables | 1.5-2 | Backend |
| | Migration 2: Equipment tables | 1.5-2 | Backend |
| **2. Domain** | 12 entity files | 2-2.5 | Backend |
| | 4 value object files | 0.5-1 | Backend |
| | 8 enum files | 0.5-1 | Backend |
| | Domain logic methods | 1-1.5 | Backend |
| **3. Application** | 4 service implementations | 1.5-2 | Backend |
| | 12 DTO files | 1-1.5 | Backend |
| | 4 interface files | 0.5-1 | Backend |
| **4. Infrastructure** | DbContext + 10 repositories | 2-2.5 | Backend |
| | RLS context integration | 0.5-1 | Backend |
| | Connection/retry logic | 0.5-1 | Backend |
| **5. API** | 7 controllers (~800 lines) | 2-2.5 | Backend |
| | Program.cs DI registration | 0.5 | Backend |
| **6. Validators** | 5 validator files | 1 | Backend |
| **7. Unit Tests** | 7 test files | 2-2.5 | Backend |
| **8. Integration Tests** | 4 test files | 2-2.5 | Backend |
| **TOTAL** | | **16-20** | |

---

## ✅ QUALITY GATES (Same as FRP-01)

1. ✅ All repositories with RLS
2. ✅ Unit test coverage ≥90%
3. ✅ API endpoints operational
4. ✅ Integration tests passing
5. ✅ Health checks configured
6. ✅ Swagger documentation
7. ✅ Production polish (CORS, validators, logging)
8. ✅ Acceptance criteria met

---

## 🎯 ACCEPTANCE CRITERIA VALIDATION

### From PRD:
- ✅ **Map valves to zones** - Implemented via `valve_zone_map` table + API
- ✅ **Calibration & faults reportable** - `equipment_calibrations` + `equipment_faults` tables + reporting endpoints
- ✅ **RLS blocks cross-site access** - Integration tests validate
- ✅ **Equipment health dashboard** - `/equipment` endpoints with status/heartbeat

---

## 🚀 DEPENDENCIES & BLOCKING

### Prerequisites (All Met ✅)
- ✅ FRP-01 Complete (Identity, RLS, ABAC)
- ✅ Database infrastructure (Supabase)
- ✅ API infrastructure (ASP.NET Core)

### Blocks (After FRP-02 Complete)
- **FRP-05: Telemetry** - Needs equipment registry for sensor mapping
- **FRP-06: Irrigation** - Needs valve-zone mappings
- **FRP-07: Inventory** - Needs spatial hierarchy for lot locations

---

## 📝 OPEN QUESTIONS FOR REVIEW

1. **Hardware Templates:** Should we pre-seed HSES12 and HSEA24 templates?
2. **Vault Implementation:** Should Vault be a Room type or separate table?
3. **Equipment Deletion:** Hard delete or soft delete (status = 'decommissioned')?
4. **Calibration Reminders:** Build notification system now or later?
5. **Device Health Dashboard:** Build in FRP-02 or defer to FRP-15 (Notifications)?

---

## 🎯 SUCCESS CRITERIA

**Definition of Done:**
- ✅ All 8 quality gates passed
- ✅ Valve-zone mapping API operational
- ✅ Calibration tracking with due dates
- ✅ Equipment health monitoring
- ✅ RLS validated (cross-site blocked)
- ✅ Integration tests passing
- ✅ Swagger docs published
- ✅ Ready for FRP-05 (Telemetry) handoff

**Expected Outcome:**
- 50-60 C# files created
- ~6,000-8,000 lines of code
- Complete spatial + equipment foundation
- Production-ready API
- FRP-05, FRP-06, FRP-07 unblocked

---

**Status:** 🎯 READY FOR REVIEW & APPROVAL  
**Next Step:** Review plan → Get approval → Begin implementation  
**Estimated Completion:** 16-20 hours from start
