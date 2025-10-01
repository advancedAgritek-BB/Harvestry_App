# FRP-02 Execution Plan - Remaining Work

**Date:** October 1, 2025  
**Status:** Phase 1-2 Complete (36%), Phase 3-7 Remaining (64%)  
**Approach:** Build vertically in 4 feature slices  
**Estimated Time:** 24-29 hours (accounts for shared prep, configuration, and validation work)

---

## 📊 Current State Summary

### ✅ COMPLETE (36%)

- ✅ **Database Schema** - 3 migrations, ~1,000 lines SQL
- ✅ **Domain Entities** - 5 entities, 3 enum files, ~1,500 lines C#
- ✅ **Test Infrastructure** - Automation script for PostgreSQL
- ✅ **Documentation** - 5 comprehensive documents

### 🚧 REMAINING (64%)

- 🚧 **Application Services** - 4 services with interfaces and DTOs
- 🚧 **Infrastructure** - 5 repositories with RLS
- 🚧 **API Controllers** - 3 controllers with OpenAPI
- 🚧 **Validation** - 4 FluentValidation files
- 🚧 **Testing** - 10 test files (unit + integration)

---

## 🎯 Execution Strategy

### Why Vertical Slices?

Instead of building all services → all repos → all controllers, we build **complete vertical slices**:

**Slice = Service + Repository + Controller + Validators + Tests**

**Benefits:**

- ✅ Each slice is independently testable
- ✅ Demonstrates progress incrementally
- ✅ Easier to review and validate
- ✅ Reduces integration risk
- ✅ Can deploy slice-by-slice

**Note:** Total estimate is 28.5 hours over 5 days (accounts for infrastructure setup, implementation, testing, and deployment wiring). This is conservative and includes breaks, context switching, and configuration tasks.

---

## 📋 THE 4 SLICES

```
SLICE 1: Spatial Hierarchy (Rooms + Locations)
├── Service: SpatialHierarchyService
├── Repos: RoomRepository, InventoryLocationRepository
├── Controller: SpatialController
├── Validators: RoomValidators, LocationValidators
└── Tests: Unit + Integration

SLICE 2: Equipment Registry
├── Service: EquipmentRegistryService
├── Repo: EquipmentRepository
├── Controller: EquipmentController
├── Validators: EquipmentValidators
└── Tests: Unit + Integration

SLICE 3: Calibration
├── Service: CalibrationService
├── Repo: EquipmentCalibrationRepository
├── Controller: CalibrationController (or extend EquipmentController)
├── Validators: CalibrationValidators
└── Tests: Unit + Integration

SLICE 4: Valve Mapping
├── Service: ValveZoneMappingService
├── Repo: ValveZoneMappingRepository
├── API: Valve mapping endpoints
└── Tests: Integration
```

---

## 🧰 Pre-Slice Setup (90 min)

Complete these shared tasks before starting the feature slices:

1. **Domain Rehydration Helpers (45 min)**
   - Add static `FromPersistence(...)` factories (or internal constructors) to `Room`, `InventoryLocation`, `Equipment`, `EquipmentChannel`, and `Calibration` so repositories can materialize aggregates without reflection.
   - Keep persistence-specific guardrails inside the factory to centralize validation and audit stamping.

2. **DTO Mapping Profiles (20 min)**
   - Create an AutoMapper profile (or dedicated mapper class) under `Application/Mappers` to convert between domain entities and API DTOs.
   - Ensures controllers return DTOs rather than exposing domain types directly.

3. **Configuration & DI Checklist (25 min)**
   - Register `SpatialDataSourceFactory`, services, repositories, validators, and mappers in the API `Program.cs`.
   - Wire up a dedicated `SPATIAL_DB_CONNECTION` secret across `appsettings.*`, Kubernetes/Helm manifests, and CI secret templates.
   - Document the environment variable in `docs/infra/environment-variables.md` for operations.

---

## 🔧 SLICE 1: SPATIAL HIERARCHY

**Goal:** Complete CRUD for rooms and hierarchical locations  
**Time:** 7-8 hours (after shared pre-work)  
**Owner:** Core Platform/Spatial Squad

### Task 1.1: Create Folder Structure (5 min)

```bash
# Create directories
mkdir -p src/backend/services/core-platform/spatial/Application/Interfaces
mkdir -p src/backend/services/core-platform/spatial/Application/DTOs
mkdir -p src/backend/services/core-platform/spatial/Application/Services
mkdir -p src/backend/services/core-platform/spatial/Infrastructure/Persistence
mkdir -p src/backend/services/core-platform/spatial/API/Controllers
mkdir -p src/backend/services/core-platform/spatial/API/Validators
```

### Task 1.2: Create Service Interface (15 min)

**File:** `Application/Interfaces/ISpatialHierarchyService.cs`

```csharp
using Harvestry.Spatial.Application.DTOs;
using Harvestry.Spatial.Domain.Entities;

namespace Harvestry.Spatial.Application.Interfaces;

/// <summary>
/// Service for managing spatial hierarchy (rooms and locations)
/// </summary>
public interface ISpatialHierarchyService
{
    // Room operations
    Task<Room> CreateRoomAsync(Guid siteId, CreateRoomRequest request, Guid userId, CancellationToken ct = default);
    Task<Room?> GetRoomByIdAsync(Guid siteId, Guid roomId, CancellationToken ct = default);
    Task<List<Room>> GetRoomsBySiteAsync(Guid siteId, CancellationToken ct = default);
    Task<RoomWithHierarchyDto> GetRoomWithHierarchyAsync(Guid siteId, Guid roomId, CancellationToken ct = default);
    Task UpdateRoomAsync(Guid siteId, Guid roomId, UpdateRoomRequest request, Guid userId, CancellationToken ct = default);
    Task DeleteRoomAsync(Guid siteId, Guid roomId, Guid userId, CancellationToken ct = default);

    // Location operations
    Task<InventoryLocation> CreateLocationAsync(CreateLocationRequest request, Guid userId, CancellationToken ct = default);
    Task<InventoryLocation?> GetLocationByIdAsync(Guid locationId, CancellationToken ct = default);
    Task<List<InventoryLocation>> GetLocationChildrenAsync(Guid locationId, CancellationToken ct = default);
    Task<List<InventoryLocation>> GetLocationPathAsync(Guid locationId, CancellationToken ct = default);
    Task<List<InventoryLocation>> GetLocationsByRoomAsync(Guid roomId, CancellationToken ct = default);
    Task UpdateLocationAsync(Guid locationId, UpdateLocationRequest request, Guid userId, CancellationToken ct = default);
    Task DeleteLocationAsync(Guid locationId, Guid userId, CancellationToken ct = default);
    
    // Validation helpers
    Task<bool> ValidateLocationHierarchyAsync(LocationType parentType, LocationType childType);
}
```

### Task 1.3: Create DTOs (20 min)

**File:** `Application/DTOs/RoomDtos.cs`

```csharp
using Harvestry.Spatial.Domain.Enums;

namespace Harvestry.Spatial.Application.DTOs;

public record CreateRoomRequest(
    string Code,
    string Name,
    RoomType RoomType,
    string? CustomRoomType,
    string? Description,
    int? FloorLevel,
    decimal? AreaSqft,
    decimal? HeightFt
);

public record UpdateRoomRequest(
    string Name,
    string? Description,
    int? FloorLevel,
    decimal? AreaSqft,
    decimal? HeightFt,
    decimal? TargetTempF,
    decimal? TargetHumidityPct,
    int? TargetCo2Ppm
);

public record RoomWithHierarchyDto(
    Guid Id,
    string Code,
    string Name,
    RoomType RoomType,
    string? CustomRoomType,
    RoomStatus Status,
    List<LocationTreeNode> Locations
);

public record RoomResponse(
    Guid Id,
    Guid SiteId,
    string Code,
    string Name,
    RoomType RoomType,
    string? CustomRoomType,
    RoomStatus Status,
    string? Description,
    int? FloorLevel,
    decimal? AreaSqft,
    decimal? HeightFt,
    decimal? TargetTempF,
    decimal? TargetHumidityPct,
    int? TargetCo2Ppm
);

public record LocationTreeNode(
    Guid Id,
    string Code,
    string Name,
    LocationType LocationType,
    string Path,
    int Depth,
    List<LocationTreeNode> Children
);
```

**File:** `Application/DTOs/LocationDtos.cs`

```csharp
using Harvestry.Spatial.Domain.Enums;

namespace Harvestry.Spatial.Application.DTOs;

public record CreateLocationRequest(
    Guid SiteId,
    Guid? RoomId,
    Guid? ParentId,
    LocationType LocationType,
    string Code,
    string Name,
    string? Barcode,
    int? PlantCapacity,
    decimal? WeightCapacityLbs,
    decimal? LengthFt,
    decimal? WidthFt,
    decimal? HeightFt
);

public record UpdateLocationRequest(
    string Name,
    string? Barcode,
    string? Notes,
    decimal? LengthFt,
    decimal? WidthFt,
    decimal? HeightFt,
    int? PlantCapacity,
    decimal? WeightCapacityLbs
);

public record LocationResponse(
    Guid Id,
    Guid SiteId,
    Guid? RoomId,
    Guid? ParentId,
    LocationType LocationType,
    string Code,
    string Name,
    string? Barcode,
    int Depth,
    int? PlantCapacity,
    int CurrentPlantCount,
    decimal? WeightCapacityLbs,
    decimal? CurrentWeightLbs,
    LocationStatus Status
);

public record LocationPathDto(
    List<LocationBreadcrumb> Breadcrumbs
);

public record LocationBreadcrumb(
    Guid Id,
    string Name,
    LocationType LocationType,
    int Depth
);
```

### Task 1.4: Implement Service (60 min)

**File:** `Application/Services/SpatialHierarchyService.cs`

**Pattern to follow:** Look at `identity/Application/Services/BadgeAuthService.cs` for:

- Constructor DI pattern
- Validation approach
- Exception handling
- Async/await usage

**Key implementation notes:**

- Validate room code uniqueness (per site)
- Validate location hierarchy rules (e.g., can't nest Bin under Row)
- Build tree structures for GetRoomWithHierarchyAsync
- Use repository methods for data access
- Return domain entities via `FromPersistence` factories to avoid reflection and keep invariants enforced

**Estimated lines:** ~350

### Task 1.5: Create Repositories (105 min)

**File:** `Infrastructure/Persistence/SpatialDataSourceFactory.cs`

**Copy from:** `identity/Infrastructure/Persistence/IdentityDataSourceFactory.cs`  
**Changes:**

- Rename class to `SpatialDataSourceFactory`
- Read connection string from `SPATIAL_DB_CONNECTION` (new secret managed alongside identity connection)

**Estimated lines:** ~150

---

**File:** `Infrastructure/Persistence/RoomRepository.cs`

```csharp
using Npgsql;
using Harvestry.Spatial.Domain.Entities;
using Harvestry.Spatial.Domain.Enums;

namespace Harvestry.Spatial.Infrastructure.Persistence;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Room?> GetByCodeAsync(Guid siteId, string code, CancellationToken ct = default);
    Task<List<Room>> GetBySiteAsync(Guid siteId, CancellationToken ct = default);
    Task<Guid> InsertAsync(Room room, CancellationToken ct = default);
    Task UpdateAsync(Room room, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public class RoomRepository : IRoomRepository
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<RoomRepository> _logger;
    
    public RoomRepository(NpgsqlDataSource dataSource, ILogger<RoomRepository> logger)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task<Room?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT id, site_id, code, name, room_type, custom_room_type, status,
                   description, floor_level, area_sqft, height_ft,
                   target_temp_f, target_humidity_pct, target_co2_ppm,
                   created_at, created_by_user_id, updated_at, updated_by_user_id
            FROM rooms
            WHERE id = @id";
        
        await using var connection = await _dataSource.OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("id", id);
        
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;
        
        return MapRoom(reader);
    }
    
    // ... implement other methods
    
    private Room MapRoom(NpgsqlDataReader reader)
    {
        return Room.FromPersistence(
            id: reader.GetGuid(reader.GetOrdinal("id")),
            siteId: reader.GetGuid(reader.GetOrdinal("site_id")),
            code: reader.GetString(reader.GetOrdinal("code")),
            name: reader.GetString(reader.GetOrdinal("name")),
            roomType: (RoomType)reader.GetInt32(reader.GetOrdinal("room_type")),
            customRoomType: reader.IsDBNull(reader.GetOrdinal("custom_room_type")) ? null : reader.GetString(reader.GetOrdinal("custom_room_type")),
            status: (RoomStatus)reader.GetInt32(reader.GetOrdinal("status")),
            description: reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
            floorLevel: reader.IsDBNull(reader.GetOrdinal("floor_level")) ? null : reader.GetInt32(reader.GetOrdinal("floor_level")),
            areaSqft: reader.IsDBNull(reader.GetOrdinal("area_sqft")) ? null : reader.GetDecimal(reader.GetOrdinal("area_sqft")),
            heightFt: reader.IsDBNull(reader.GetOrdinal("height_ft")) ? null : reader.GetDecimal(reader.GetOrdinal("height_ft")),
            targetTempF: reader.IsDBNull(reader.GetOrdinal("target_temp_f")) ? null : reader.GetDecimal(reader.GetOrdinal("target_temp_f")),
            targetHumidityPct: reader.IsDBNull(reader.GetOrdinal("target_humidity_pct")) ? null : reader.GetDecimal(reader.GetOrdinal("target_humidity_pct")),
            targetCo2Ppm: reader.IsDBNull(reader.GetOrdinal("target_co2_ppm")) ? null : reader.GetInt32(reader.GetOrdinal("target_co2_ppm")),
            createdAt: reader.GetDateTime(reader.GetOrdinal("created_at")),
            createdByUserId: reader.GetGuid(reader.GetOrdinal("created_by_user_id")),
            updatedAt: reader.GetDateTime(reader.GetOrdinal("updated_at")),
            updatedByUserId: reader.GetGuid(reader.GetOrdinal("updated_by_user_id"))
        );
    }
}
```

**Pattern to follow:** `identity/Infrastructure/Persistence/UserRepository.cs`

**Key implementation notes:**

- Set RLS context: `SET LOCAL app.current_user_id = '{userId}'`
- Use parameterized queries (SQL injection protection)
- Implement retry logic for transient errors
- Proper async/await with cancellation tokens

**Estimated lines:** ~250

---

**File:** `Infrastructure/Persistence/InventoryLocationRepository.cs`

**Key methods:**

```csharp
- GetByIdAsync
- GetChildrenAsync (WHERE parent_id = @parentId)
- GetPathAsync (recursive CTE to get ancestors)
- GetByRoomAsync
- InsertAsync
- UpdateAsync
```

**Special query - GetPathAsync:**

```sql
WITH RECURSIVE location_path AS (
    SELECT id, parent_id, name, location_type, depth, path
    FROM inventory_locations
    WHERE id = @locationId
    
    UNION ALL
    
    SELECT l.id, l.parent_id, l.name, l.location_type, l.depth, l.path
    FROM inventory_locations l
    INNER JOIN location_path lp ON l.id = lp.parent_id
)
SELECT * FROM location_path ORDER BY depth;
```

**Estimated lines:** ~300

### Task 1.6: Create Controller (75 min)

**File:** `API/Controllers/SpatialController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Harvestry.Spatial.Application.Interfaces;
using Harvestry.Spatial.Application.DTOs;

namespace Harvestry.Spatial.API.Controllers;

[ApiController]
[Route("api/sites/{siteId:guid}/rooms")]
[Produces("application/json")]
public class SpatialController : ControllerBase
{
    private readonly ISpatialHierarchyService _spatialService;
    private readonly ILogger<SpatialController> _logger;
    
    public SpatialController(
        ISpatialHierarchyService spatialService,
        ILogger<SpatialController> logger)
    {
        _spatialService = spatialService;
        _logger = logger;
    }
    
    /// <summary>
    /// Creates a new room for a site
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RoomResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRoom(
        Guid siteId,
        [FromBody] CreateRoomRequest request,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId(); // From claims
        var room = await _spatialService.CreateRoomAsync(siteId, request, userId, ct);
        var response = RoomMapper.ToResponse(room);
        return CreatedAtAction(nameof(GetRoom), new { siteId, roomId = response.Id }, response);
    }
    
    /// <summary>
    /// Gets a room by ID
    /// </summary>
    [HttpGet("{roomId:guid}")]
    [ProducesResponseType(typeof(RoomResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoom(Guid siteId, Guid roomId, CancellationToken ct)
    {
        var room = await _spatialService.GetRoomByIdAsync(siteId, roomId, ct);
        if (room == null)
            return NotFound();
        return Ok(RoomMapper.ToResponse(room));
    }
    
    // ... implement remaining endpoints
    
    private Guid GetCurrentUserId()
    {
        // Extract from JWT claims or context
        var userIdClaim = User.FindFirst("sub")?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }
}
```

**Pattern to follow:** `identity/API/Controllers/UsersController.cs`

**Endpoints to implement:**

- POST /api/sites/{siteId}/rooms
- GET /api/sites/{siteId}/rooms
- GET /api/sites/{siteId}/rooms/{roomId}
- GET /api/sites/{siteId}/rooms/{roomId}/hierarchy
- PUT /api/sites/{siteId}/rooms/{roomId}
- DELETE /api/sites/{siteId}/rooms/{roomId}
- POST /api/sites/{siteId}/rooms/{roomId}/locations
- GET /api/sites/{siteId}/locations/{locationId}
- GET /api/sites/{siteId}/locations/{locationId}/children
- GET /api/sites/{siteId}/locations/{locationId}/path
- PUT /api/sites/{siteId}/locations/{locationId}
- DELETE /api/sites/{siteId}/locations/{locationId}

**Implementation notes:**

- Maintain site-scoped routes for multi-tenant clarity and guardrails
- Return DTOs (e.g., `RoomResponse`, `LocationResponse`) using the mapper profile
- Register controller routes and Swagger annotations after wiring DI registrations

**Estimated lines:** ~200

### Task 1.7: Create Validators (30 min)

**File:** `API/Validators/RoomValidators.cs`

```csharp
using FluentValidation;
using Harvestry.Spatial.Application.DTOs;
using Harvestry.Spatial.Domain.Enums;

namespace Harvestry.Spatial.API.Validators;

public class CreateRoomRequestValidator : AbstractValidator<CreateRoomRequest>
{
    public CreateRoomRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50)
            .Matches(@"^[A-Z0-9-]+$")
            .WithMessage("Code must contain only uppercase letters, numbers, and hyphens");
        
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
        
        RuleFor(x => x.CustomRoomType)
            .NotEmpty()
            .When(x => x.RoomType == RoomType.Custom)
            .WithMessage("Custom room type must be specified when RoomType is Custom");
        
        RuleFor(x => x.CustomRoomType)
            .Empty()
            .When(x => x.RoomType != RoomType.Custom)
            .WithMessage("Custom room type should only be specified when RoomType is Custom");
        
        RuleFor(x => x.AreaSqft)
            .GreaterThan(0)
            .When(x => x.AreaSqft.HasValue);
        
        RuleFor(x => x.HeightFt)
            .GreaterThan(0)
            .When(x => x.HeightFt.HasValue);
    }
}

public class UpdateRoomRequestValidator : AbstractValidator<UpdateRoomRequest>
{
    public UpdateRoomRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
        
        RuleFor(x => x.TargetHumidityPct)
            .InclusiveBetween(0, 100)
            .When(x => x.TargetHumidityPct.HasValue);
        
        RuleFor(x => x.TargetCo2Ppm)
            .GreaterThan(0)
            .When(x => x.TargetCo2Ppm.HasValue);
    }
}
```

**File:** `API/Validators/LocationValidators.cs` (similar pattern)

**Pattern to follow:** `identity/API/Validators/UserRequestValidators.cs`

**Estimated lines:** ~150 total

### Task 1.8: Unit Tests (60 min)

**File:** `Tests/Unit/Domain/RoomTests.cs`

```csharp
using Xunit;
using Harvestry.Spatial.Domain.Entities;
using Harvestry.Spatial.Domain.Enums;

namespace Harvestry.Spatial.Tests.Unit.Domain;

public class RoomTests
{
    private readonly Guid _siteId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    
    [Fact]
    public void Constructor_ValidInputs_CreatesRoom()
    {
        // Arrange & Act
        var room = new Room(_siteId, "VEG-1", "Veg Room 1", RoomType.Veg, _userId);
        
        // Assert
        Assert.NotEqual(Guid.Empty, room.Id);
        Assert.Equal(_siteId, room.SiteId);
        Assert.Equal("VEG-1", room.Code);
        Assert.Equal("Veg Room 1", room.Name);
        Assert.Equal(RoomType.Veg, room.RoomType);
        Assert.Equal(RoomStatus.Active, room.Status);
    }
    
    [Fact]
    public void Constructor_CustomRoomType_RequiresCustomTypeString()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => 
            new Room(_siteId, "CUSTOM-1", "Custom Room", RoomType.Custom, _userId));
        Assert.Contains("Custom room type must be specified", ex.Message);
    }
    
    [Fact]
    public void Quarantine_ChangesStatusToQuarantine()
    {
        // Arrange
        var room = new Room(_siteId, "VEG-1", "Veg Room 1", RoomType.Veg, _userId);
        
        // Act
        room.Quarantine(_userId);
        
        // Assert
        Assert.Equal(RoomStatus.Quarantine, room.Status);
    }
    
    // ... more tests
}
```

**Tests needed:**

- Constructor validation tests (empty params, custom room type)
- Status change tests (Activate, Deactivate, SetMaintenance, Quarantine)
- UpdateInfo tests
- UpdateDimensions tests
- UpdateEnvironmentTargets tests
- IsOperational tests
- GetDisplayRoomType tests

**Estimated lines:** ~200

---

**File:** `Tests/Unit/Domain/InventoryLocationTests.cs`

**Tests needed:**

- Constructor validation (hierarchy rules)
- Capacity tracking (AddPlants, RemovePlants, AddWeight, RemoveWeight)
- Auto-status management (Full when at capacity)
- Helper methods (IsCultivationLocation, IsWarehouseLocation, HasCapacity)

**Estimated lines:** ~250

---

**File:** `Tests/Unit/Services/SpatialHierarchyServiceTests.cs`

**Tests needed:**

- CreateRoom success and failure cases
- CreateLocation with hierarchy validation
- GetRoomWithHierarchy tree building
- Update operations
- Delete operations with cascading

**Estimated lines:** ~200

### Task 1.9: Integration Tests (90 min)

**File:** `Tests/Integration/SpatialHierarchyIntegrationTests.cs`

```csharp
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Harvestry.Spatial.Application.Interfaces;
using Harvestry.Spatial.Application.DTOs;
using Harvestry.Spatial.Domain.Enums;

namespace Harvestry.Spatial.Tests.Integration;

[Collection("Integration")]
public class SpatialHierarchyIntegrationTests : IntegrationTestBase
{
    private readonly ISpatialHierarchyService _spatialService;
    
    public SpatialHierarchyIntegrationTests(IntegrationTestFixture fixture) : base(fixture)
    {
        _spatialService = ServiceProvider.GetRequiredService<ISpatialHierarchyService>();
    }
    
    [Fact]
    public async Task CreateRoom_ValidRequest_ReturnsRoomWithId()
    {
        // Arrange
        var request = new CreateRoomRequest(
            Code: "VEG-TEST",
            Name: "Test Veg Room",
            RoomType: RoomType.Veg,
            CustomRoomType: null,
            Description: "Integration test room",
            FloorLevel: 1,
            AreaSqft: 1000m,
            HeightFt: 12m
        );
        
        // Act
        var room = await _spatialService.CreateRoomAsync(TestSiteId, request, TestUserId);
        
        // Assert
        Assert.NotEqual(Guid.Empty, room.Id);
        Assert.Equal("VEG-TEST", room.Code);
        Assert.Equal(TestSiteId, room.SiteId);
    }
    
    [Fact]
    public async Task CreateLocation_WithHierarchy_BuildsCorrectPath()
    {
        // Arrange - Create room first
        var room = await CreateTestRoomAsync();
        
        // Create zone
        var zoneRequest = new CreateLocationRequest(
            SiteId: TestSiteId,
            RoomId: room.Id,
            ParentId: null,
            LocationType: LocationType.Zone,
            Code: "ZONE-A",
            Name: "Zone A",
            Barcode: null,
            PlantCapacity: 100,
            WeightCapacityLbs: null,
            LengthFt: null,
            WidthFt: null,
            HeightFt: null
        );
        
        var zone = await _spatialService.CreateLocationAsync(zoneRequest, TestUserId);
        
        // Assert
        Assert.NotEqual(Guid.Empty, zone.Id);
        Assert.Equal("Zone A", zone.Path); // Path should be just the name for top level
        Assert.Equal(0, zone.Depth);
    }
    
    // ... more tests
}
```

**Tests needed:**

- CRUD operations for rooms
- CRUD operations for locations
- Hierarchy path building
- RLS verification (cross-site access blocked)
- Capacity tracking
- Status management

**Estimated lines:** ~300

---

**File:** `Tests/Integration/RlsSpatialTests.cs`

**Purpose:** Verify RLS policies block cross-site access

**Tests needed:**

- User can see rooms for their assigned sites only
- User cannot see rooms for other sites
- User can see locations in their assigned sites only
- Updates blocked for other sites
- Deletes blocked for other sites

**Estimated lines:** ~150

---

### Slice 1 Summary

**Total Time:** 5-6 hours  
**Total Files:** 15 files  
**Total Lines:** ~2,500 lines

**Deliverables:**

- ✅ Service interface + implementation
- ✅ 2 repositories with RLS
- ✅ 1 controller with 12 endpoints
- ✅ 2 validator files
- ✅ 5 test files (unit + integration)

---

## 🔧 SLICE 2: EQUIPMENT REGISTRY

**Goal:** Complete equipment CRUD with device twin and health tracking  
**Time:** 5-6 hours  
**Dependencies:** None (can run in parallel with Slice 1 if needed)

### Quick Overview (Detailed steps similar to Slice 1)

**Service:** `EquipmentRegistryService.cs` (~400 lines)

- CreateEquipment, GetEquipment, UpdateEquipment
- RecordHeartbeat, UpdateNetworkConfig, UpdateDeviceTwin
- AssignToLocation, ChangeStatus
- CreateChannel, GetChannels, UpdateChannel

**Repository:** `EquipmentRepository.cs` (~350 lines)

- Standard CRUD with RLS
- JSONB queries for device twin
- Online status filtering (computed column)
- Equipment with channels JOIN queries

**Controller:** `EquipmentController.cs` (~250 lines)

- POST /api/sites/{siteId}/equipment
- GET /api/sites/{siteId}/equipment (with filters)
- GET /api/equipment/{id}
- PUT /api/equipment/{id}
- POST /api/equipment/{id}/heartbeat
- PUT /api/equipment/{id}/network
- POST /api/equipment/{id}/channels
- GET /api/equipment/{id}/channels

**Validators:** `EquipmentValidators.cs` (~150 lines)

**Tests:** 3 files (~450 lines total)

- EquipmentTests.cs (unit)
- EquipmentRegistryServiceTests.cs (unit)
- EquipmentIntegrationTests.cs (integration with RLS)

---

## 🔧 SLICE 3: CALIBRATION

**Goal:** Calibration recording and tracking  
**Time:** 3-4 hours  
**Dependencies:** Slice 2 (Equipment must exist)

### Quick Overview

**Service:** `CalibrationService.cs` (~250 lines)

- RecordCalibration
- GetCalibrationHistory
- GetLatestCalibration
- CheckOverdueCalibrations
- CalculateNextDueDate

**Repository:** `EquipmentCalibrationRepository.cs` (~200 lines)

- Standard CRUD with RLS (via equipment FK)
- History queries (ORDER BY performed_at DESC)
- Overdue queries (WHERE next_due_at < CURRENT_DATE)

**Controller:** `CalibrationController.cs` (~150 lines) or extend EquipmentController

- POST /api/equipment/{id}/calibrations
- GET /api/equipment/{id}/calibrations
- GET /api/equipment/{id}/calibrations/latest
- GET /api/calibrations/overdue

**Validators:** `CalibrationValidators.cs` (~100 lines)

**Tests:** 2 files (~250 lines total)

- CalibrationTests.cs (unit - deviation calculations)
- CalibrationIntegrationTests.cs (integration)

---

## 🔧 SLICE 4: VALVE MAPPING

**Goal:** Valve-to-zone routing matrix  
**Time:** 2-3 hours  
**Dependencies:** Slice 1 (Locations) + Slice 2 (Equipment)

### Quick Overview

**Service:** `ValveZoneMappingService.cs` (~200 lines)

- CreateMapping
- GetMappingsForValve
- GetMappingsForZone
- UpdateMapping
- DeleteMapping
- ValidateInterlockGroups

**Repository:** `ValveZoneMappingRepository.cs` (~150 lines)

- Standard CRUD with RLS
- Query by valve (WHERE valve_equipment_id = @id)
- Query by zone (WHERE zone_location_id = @id)
- Interlock group queries

**API:** Add endpoints to EquipmentController or SpatialController (~100 lines)

- POST /api/valve-mappings
- GET /api/equipment/{id}/valve-mappings
- GET /api/locations/{id}/valve-mappings
- DELETE /api/valve-mappings/{id}

**Tests:** 1 file (~150 lines)

- ValveZoneMappingIntegrationTests.cs (integration, many-to-many scenarios)

---

## 📅 RECOMMENDED TIMELINE

### Day 1 (6.5 hours) - Foundation + Slice 1 Part 1

- Morning (2.5h): Pre-slice setup (rehydration factories, mapper profile, configuration checklist)
- Midday (2h): Tasks 1.1-1.3 (folder structure, service interface, DTOs)
- Afternoon (2h): Begin Task 1.4 (service implementation)

### Day 2 (6 hours) - Slice 1 Part 2

- Morning (3h): Complete Task 1.4 and build `SpatialDataSourceFactory`
- Afternoon (3h): RoomRepository + InventoryLocationRepository (Task 1.5)

### Day 3 (5.5 hours) - Slice 1 Part 3

- Morning (2h): Task 1.6 (controller with explicit routing + DTO responses)
- Midday (1h): Task 1.7 (validators)
- Afternoon (2.5h): Tasks 1.8-1.9 (unit + integration tests)

### Day 4 (6 hours) - Slice 2

- Morning (3h): Equipment service + repository (with mapper + rehydration helpers)
- Afternoon (3h): Equipment controller + validators + tests

### Day 5 (4.5 hours) - Slices 3 + 4

- Morning (3h): Calibration (all layers, ensure configuration/DI updates)
- Afternoon (1.5h): Valve mapping (all layers + tests)

**Total:** 28.5 hours over 5 days (includes pre-slice setup, implementation, testing, and configuration/DI wiring)

---

## 🎯 Definition of Done

Each slice is complete when:

- ✅ Service implemented with all methods
- ✅ Repository implemented with RLS context
- ✅ Controller with all endpoints and OpenAPI docs
- ✅ FluentValidation validators
- ✅ Unit tests passing (≥90% coverage)
- ✅ Integration tests passing (including RLS verification)
- ✅ No linter errors
- ✅ Manual API testing via Swagger
- ✅ Program.cs, appsettings, and deployment secrets updated/validated for new components

---

## 🔧 Helper Commands

### Run Tests

```bash
# Run all tests with coverage
dotnet test /p:CollectCoverage=true

# Run integration tests only
dotnet test --filter Category=Integration

# Run with test automation script
./scripts/test/run-with-local-postgres.sh
```

### Check Coverage

```bash
# Generate coverage report
dotnet test /p:CollectCoverage=true /p:CoverageReporter=html
```

### Start API locally

```bash
cd src/backend/services/core-platform/spatial/API
dotnet run
```

---

## 📋 Progress Tracking

After each task, update:

1. `docs/TRACK_B_COMPLETION_CHECKLIST.md` - Mark checkboxes
2. `docs/FRP02_CURRENT_STATUS.md` - Update % complete
3. Git commit with meaningful message

Example commit messages:

```
feat(frp02): implement spatial hierarchy service
feat(frp02): add room and location repositories with RLS
feat(frp02): add spatial controller with 12 endpoints
test(frp02): add integration tests for spatial hierarchy
```

---

## 🚦 Quality Gates

Before marking a slice complete, verify:

1. ✅ All tests passing
2. ✅ Code coverage ≥90% for services
3. ✅ RLS verification tests passing
4. ✅ Swagger UI loads without errors
5. ✅ Manual testing of happy path works
6. ✅ No linter warnings
7. ✅ Follows FRP-01 patterns

---

## 🎯 Success Metrics

FRP-02 is successfully complete when:

- ✅ All 4 slices delivered and tested
- ✅ 23/23 checklist items marked complete
- ✅ All acceptance criteria met
- ✅ Performance targets met (p95 < 200ms)
- ✅ Documentation updated
- ✅ Ready for FRP-03

**Note on estimates:** The 28.5-hour total (spread over 5 days) accounts for all work including setup, coding, testing, breaks, and deployment configuration. Pure execution time (focused coding without setup/breaks) is estimated at 18-22 hours.

---

**Ready to start?** Begin with **Slice 1, Task 1.1** (Folder Structure) 🚀
