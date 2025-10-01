# FRP-02 Completion Plan

**Status:** 🚧 65% Complete (17/26 tasks)  
**Remaining:** 9 tasks, ~19 hours  
**Target Completion:** End of Sprint 2 (W3-W4)

---

## 📊 Current State (As of 2025-09-30)

### ✅ COMPLETE (65%)
- ✅ **Phase 1:** Database schema (3 migrations, ~1,000 lines)
- ✅ **Phase 2:** Domain layer (5 entities, 3 enum files, ~1,500 lines)
- ✅ **Slice 1:** Spatial hierarchy service + repositories + controllers + validators + tests

### 🚧 REMAINING (35%)
- 🚧 **Phase 3:** Application services (Equipment, Calibration, Valve Mapping)
- 🚧 **Phase 4:** Infrastructure repositories (Equipment, Calibration, Valve Mapping)
- 🚧 **Phase 5:** API controllers (Equipment, Calibration)
- 🚧 **Phase 6:** Validators (Equipment, Calibration)
- 🚧 **Phase 7:** Testing (Equipment domain/service/unit + calibration/valve mapping suites)

---

## 🎯 Completion Strategy

### Approach: Build Vertically (Feature Slices)

Instead of building all services → all repos → all controllers, we'll build **feature slices**:

1. **Slice 1: Spatial Hierarchy** (Rooms + Locations)
2. **Slice 2: Equipment Registry** (Equipment + Channels)
3. **Slice 3: Calibration** (Calibration + Alerts)
4. **Slice 4: Valve Mapping** (Valve-Zone routing)

Each slice completes Service → Repository → Controller → Tests for that feature.

**Benefits:**
- ✅ Testable incrementally
- ✅ Demonstrates progress faster
- ✅ Easier to review
- ✅ Follows FRP-01 pattern

---

### Shared Pre-Work (Day 0, ~90 min)

Before the slices, complete one-time prep:

1. **Domain Rehydration Helpers** – add `FromPersistence(...)` factories to rich aggregates so repositories materialize entities without reflection.
2. **DTO Mapping Profile** – create mapper/AutoMapper profile to convert domain entities to `RoomResponse`/`LocationResponse` (controllers stay DTO-first).
3. **Configuration & DI Wiring** – register spatial services/repositories/mappers in `Program.cs`, add the `SPATIAL_DB_CONNECTION` secret to appsettings + deployment manifests, and document it for operations.

---

## 📋 SLICE 1: Spatial Hierarchy (7-8 hours)

**Goal:** Complete CRUD for rooms and hierarchical locations

### 1.1 Service Layer (~90 min)

**Create:** `Application/Services/SpatialHierarchyService.cs` (~300 lines)

**Responsibilities:**
- Create room with validation
- Create location with parent hierarchy validation
- Get room with child locations (tree)
- Update room/location
- Change status
- Get location path/breadcrumb
- Validate hierarchy rules (e.g., can't nest Bin under Row)

**Reuse Patterns from FRP-01:**
- Constructor dependency injection
- Async/await throughout
- Result<T> or exceptions for errors
- Comprehensive parameter validation

**Key Methods:**
```csharp
Task<RoomResponse> CreateRoomAsync(CreateRoomRequest request, CancellationToken ct = default);
Task<RoomResponse?> GetRoomWithHierarchyAsync(Guid siteId, Guid roomId, CancellationToken ct = default);
Task<IReadOnlyList<RoomResponse>> GetRoomsBySiteAsync(Guid siteId, CancellationToken ct = default);
Task<RoomResponse> UpdateRoomAsync(Guid siteId, Guid roomId, UpdateRoomRequest request, CancellationToken ct = default);
Task<RoomResponse> ChangeRoomStatusAsync(Guid siteId, Guid roomId, RoomStatus status, Guid requestedByUserId, CancellationToken ct = default);
Task<InventoryLocationNodeResponse> CreateLocationAsync(CreateLocationRequest request, CancellationToken ct = default);
Task<InventoryLocationNodeResponse> UpdateLocationAsync(Guid siteId, Guid locationId, UpdateLocationRequest request, CancellationToken ct = default);
Task<IReadOnlyList<InventoryLocationNodeResponse>> GetLocationChildrenAsync(Guid siteId, Guid locationId, CancellationToken ct = default);
Task<IReadOnlyList<LocationPathSegment>> GetLocationPathAsync(Guid siteId, Guid locationId, CancellationToken ct = default);
Task DeleteLocationAsync(Guid siteId, Guid locationId, Guid requestedByUserId, CancellationToken ct = default);
```

---

### 1.2 Repository Layer (~90 min)

**Create:** 
- `Infrastructure/Persistence/RoomRepository.cs` (~250 lines)
- `Infrastructure/Persistence/InventoryLocationRepository.cs` (~300 lines)

**Reuse from FRP-01:**
- `IdentityDataSourceFactory` pattern (rename to `SpatialDataSourceFactory`)
- NpgsqlDataSource connection pooling
- RLS context setting: `SET LOCAL app.current_user_id = '{userId}'`
- Retry logic for transient PostgreSQL errors
- Async/await with proper disposal

**RoomRepository Methods:**
```csharp
Task<Room?> GetByIdAsync(Guid id, CancellationToken ct);
Task<Room?> GetByCodeAsync(Guid siteId, string code, CancellationToken ct);
Task<List<Room>> GetBySiteAsync(Guid siteId, CancellationToken ct);
Task<Guid> InsertAsync(Room room, CancellationToken ct);
Task UpdateAsync(Room room, CancellationToken ct);
Task DeleteAsync(Guid id, CancellationToken ct);
```

**InventoryLocationRepository Methods:**
```csharp
Task<InventoryLocation?> GetByIdAsync(Guid id, CancellationToken ct);
Task<List<InventoryLocation>> GetChildrenAsync(Guid parentId, CancellationToken ct);
Task<List<InventoryLocation>> GetPathAsync(Guid locationId, CancellationToken ct);
Task<List<InventoryLocation>> GetByRoomAsync(Guid roomId, CancellationToken ct);
Task<Guid> InsertAsync(InventoryLocation location, CancellationToken ct);
Task UpdateAsync(InventoryLocation location, CancellationToken ct);
```

**Key execution notes:**
- Read connection string from the new `SPATIAL_DB_CONNECTION` secret (wire up in configuration step)
- Use `Room.FromPersistence(...)`/`InventoryLocation.FromPersistence(...)` to load aggregates without reflection
- Use CTEs for hierarchy queries (path, ancestors, descendants)
- Leverage materialized path for fast lookups
- RLS automatically filters by site

---

### 1.3 API Layer (~75 min)

**Create:**
- `API/Controllers/RoomsController.cs` (room CRUD + hierarchy)
- `API/Controllers/LocationsController.cs` (non-room inventory nodes)

**Endpoints:**
```yaml
RoomsController
POST   /api/sites/{siteId}/rooms                             # Create room
GET    /api/sites/{siteId}/rooms                             # List rooms
GET    /api/sites/{siteId}/rooms/{roomId}                    # Get room (403 on site mismatch)
GET    /api/sites/{siteId}/rooms/{roomId}/hierarchy          # Get room with hierarchy
PUT    /api/sites/{siteId}/rooms/{roomId}                    # Update room
PATCH  /api/sites/{siteId}/rooms/{roomId}/status             # Change status
DELETE /api/sites/{siteId}/rooms/{roomId}                    # Delete room

LocationsController
POST   /api/sites/{siteId}/locations                         # Create child location
GET    /api/sites/{siteId}/locations/{locationId}/children   # List child locations
GET    /api/sites/{siteId}/locations/{locationId}/path       # Get breadcrumb path
PUT    /api/sites/{siteId}/locations/{locationId}            # Update location
DELETE /api/sites/{siteId}/locations/{locationId}            # Delete location
```

**Reuse from FRP-01:**
- Controller base class (if exists)
- Error handling via ProblemDetails
- OpenAPI documentation attributes
- Async endpoints
- When tenant mismatch occurs, surface `ProblemDetails` with 403 to aid diagnostics (avoid silent 404s)

---

### 1.4 Validation (~30 min)

**Create:** 
- `API/Validators/RoomValidators.cs` (~100 lines)
- `API/Validators/LocationValidators.cs` (~150 lines)

**Reuse from FRP-01:**
- FluentValidation patterns
- ValidationConstants for common patterns
- Async validation where needed (e.g., unique code)

**Example Rules:**
```csharp
RuleFor(x => x.Code)
    .NotEmpty()
    .MaximumLength(50)
    .Matches(@"^[A-Z0-9-]+$");
    
RuleFor(x => x.CustomRoomType)
    .NotEmpty()
    .When(x => x.RoomType == RoomType.Custom);
```

---

### 1.5 Testing (~90 min)

**Create:**
- `Tests/Unit/RoomTests.cs` (~150 lines)
- `Tests/Unit/InventoryLocationTests.cs` (~200 lines)
- `Tests/Integration/SpatialHierarchyIntegrationTests.cs` (~200 lines)

**Test Coverage:**
- **Unit:** Entity business logic (status changes, validations)
- **Unit:** Service logic (hierarchy validation)
- **Integration:** CRUD operations with database
- **Integration:** RLS verification (cross-site access blocked)

---

## 📋 SLICE 2: Equipment Registry (5-6 hours)

### 2.1 DTOs & ViewModels (~30 min)
- Define request DTOs for create/update equipment, heartbeat, network, channel operations
- Add response view models (`EquipmentResponse`, `EquipmentChannelResponse`) including location and telemetry metadata

### 2.2 Service Interface (~15 min)
- Add `IEquipmentRegistryService` covering create/read/update/list, heartbeat, network, status, and channel operations

### 2.3 Service Implementation (~120 min)
- Implement `EquipmentRegistryService`
  - Enforce location requirement for production equipment
  - Validate site alignment using `TenantMismatchException`
  - Handle device twin, heartbeat, and network updates
  - Introduce `EquipmentMapper` for DTO translations

### 2.4 Repository Layer (~90 min)
- Build `EquipmentRepository` and `EquipmentChannelRepository`
  - CRUD with RLS + `FromPersistence`
  - Filtering: status, core type, location, calibration due dates, pagination
  - JSONB helpers for device twin metadata

### 2.5 API Layer (~75 min)
- Implement `EquipmentController`
  - `POST/GET/PUT/PATCH` equipment endpoints scoped to `/api/sites/{siteId}`
  - Heartbeat, network, status, and channel management endpoints
  - Return 403 `ProblemDetails` when entity site ≠ route site; include pagination metadata

### 2.6 Validation (~45 min)
- Add FluentValidation classes for all equipment requests (create/update equipment, heartbeat, network, channel)

### 2.7 Testing (~120 min)
- Add unit tests for equipment entity + service logic
- Add `EquipmentRegistryIntegrationTests` (CRUD, RLS, filters, channels)
- Extend `SpatialTestDataSeeder` with equipment fixtures

### 2.8 Docs & Config (~30 min)
- Update OpenAPI contracts, DI registrations, and ops documentation with new equipment endpoints/telemetry requirements

## 📋 SLICE 3: Calibration (3-4 hours)

### 3.1 DTOs & ViewModels (~20 min)
- `CreateCalibrationRequest` (supports optional per-event interval override)
- `CalibrationResponse`
- Extend `EquipmentResponse` with latest calibration metadata

### 3.2 Service Interface (~10 min)
- Add `ICalibrationService` for record/history/latest/overdue operations

### 3.3 Service Implementation (~75 min)
- Implement `CalibrationService`
  - Validate site alignment via `TenantMismatchException`
  - Calculate next due dates using request overrides (default 30 days)
  - Emit non-critical alerts when calibration is ≥7 days overdue
  - Surface pass/fail/within tolerance outcomes

### 3.4 Repository Layer (~45 min)
- Build `EquipmentCalibrationRepository` with CRUD, history ordering, overdue filtering

### 3.5 API Layer (~45 min)
- Create `CalibrationController`
  - `POST /api/sites/{siteId}/equipment/{equipmentId}/calibrations`
  - `GET /api/sites/{siteId}/equipment/{equipmentId}/calibrations`
  - `GET /api/sites/{siteId}/equipment/{equipmentId}/calibrations/latest`
  - `GET /api/sites/{siteId}/calibrations/overdue`
  - Return 403 `ProblemDetails` on tenant mismatches

### 3.6 Validation (~20 min)
- FluentValidation rules for calibration requests (value ranges, timestamps, immutable records)

### 3.7 Testing (~60 min)
- `CalibrationTests` (unit) + `CalibrationServiceTests`
- `CalibrationIntegrationTests` (CRUD/RLS/overdue)

### 3.8 Docs & Config (~15 min)
- Update OpenAPI specs, DI registrations, and ops docs with calibration endpoints and alert thresholds

---

## 📋 SLICE 4: Valve Mapping (2-3 hours)

### 4.1 Service Layer (~45 min)

**Create:** `Application/Services/ValveZoneMappingService.cs` (~150 lines)

**Responsibilities:**
- Create valve-zone mapping
- Update routing matrix
- Query mappings by valve or zone
- Validate interlock groups

---

### 4.2 Repository Layer (~45 min)

**Create:** `Infrastructure/Persistence/ValveZoneMappingRepository.cs` (~150 lines)

---

### 4.3 API Layer (~30 min)

**Create:** Add valve mapping endpoints to `EquipmentController` or create separate controller

---

### 4.4 Validation (~15 min)

Add validators for valve mapping requests

---

### 4.5 Testing (~45 min)

**Create:** `Tests/Integration/ValveZoneMappingIntegrationTests.cs` (~150 lines)

---

## 📅 Suggested Timeline

### Day 0 (1.5 hours)
- ✅ Domain rehydration factories + mapper profile (45 min)
- ✅ Configuration/DI wiring + `SPATIAL_DB_CONNECTION` secret updates (45 min)

### Day 1 (6 hours)
- ✅ Morning: Slice 1.1 + 1.2 groundwork (folders, interface, DTOs)
- ✅ Afternoon: Start Slice 1.4 (service implementation)

### Day 2 (6 hours)
- ✅ Morning: Finish Slice 1.4 (service logic)
- ✅ Afternoon: Slice 1.5 (SpatialDataSourceFactory + Room/Location repositories)

### Day 3 (5.5 hours)
- ✅ Morning: Slice 1.6 (controller with DTO responses)
- ✅ Midday: Slice 1.7 (validators)
- ✅ Afternoon: Slice 1.8-1.9 (unit + integration tests)

### Day 4 (6 hours)
- ✅ Morning: Slice 2.1 + 2.2 (equipment service + repository)
- ✅ Afternoon: Slice 2.3 + 2.5 (equipment API, validators, tests)

### Day 5 (4.5 hours)
- ✅ Morning: Slice 3 (calibration service, repository, API, tests)
- ✅ Afternoon: Slice 4 (valve mapping service, repository, API/tests)

**Total:** ~29 hours over 5 days (includes prep, configuration, and expanded testing)

---

## 🎯 Definition of Done (Per Slice)

Each slice is "done" when:

- ✅ Service layer implemented with business logic
- ✅ Repository layer with RLS context
- ✅ API endpoints with OpenAPI docs
- ✅ FluentValidation validators
- ✅ Unit tests passing (≥90% coverage)
- ✅ Integration tests passing (including RLS verification)
- ✅ Code reviewed (self-review using FRP-01 patterns)
- ✅ No linter errors
- ✅ Program.cs/appsettings/deployment manifests updated for new dependencies

---

## 🔄 Continuous Integration Checkpoints

After each slice:
1. Run all tests (ensure previous slices still pass)
2. Check code coverage
3. Review OpenAPI documentation
4. Update completion checklist
5. Commit with meaningful message

---

## 📝 Key Patterns to Follow (From FRP-01)

1. **Service Layer:**
   - Interface + implementation
   - Constructor DI
   - Comprehensive validation
   - Proper exception handling
   - Async/await

2. **Repository Layer:**
   - NpgsqlDataSource pooling
   - RLS context setting
   - Retry logic for transient errors
   - Proper async disposal
   - Parameterized queries (SQL injection protection)

3. **API Layer:**
   - RESTful design
   - OpenAPI attributes
   - ProblemDetails for errors
   - Rate limiting
   - Async endpoints

4. **Validation:**
   - FluentValidation
   - Centralized constants
   - Async validators where needed
   - Clear error messages

5. **Testing:**
   - Arrange-Act-Assert pattern
   - Use xUnit
   - Integration tests with real database
   - RLS verification in every integration test
   - Test data seeding/cleanup

---

## ✅ Success Metrics

FRP-02 is complete when:

- ✅ All 23 checklist items marked complete
- ✅ All acceptance criteria met
- ✅ Test coverage ≥90% for services
- ✅ All integration tests passing
- ✅ RLS verification passing (20+ scenarios)
- ✅ OpenAPI documentation complete
- ✅ Zero linter errors
- ✅ Performance acceptable (p95 < 200ms for API calls)

---

**Ready to proceed?** Let's start with **Slice 1: Spatial Hierarchy Service**! 🚀

---

## 🧪 Test Automation Helper

- Added `scripts/test/run-with-local-postgres.sh` to spin up a disposable PostgreSQL container, apply all baseline/FRP migrations, run `dotnet test Harvestry.sln`, and tear everything down automatically.
- Default connection string: `postgresql://postgres:postgres@localhost:6543/harvestry_identity_test` (configurable via `TEST_DB_*` environment variables).
- The script exports `IDENTITY_DB_CONNECTION` for the test run, so integration specs execute against the ephemeral database without touching shared environments.
- You can still point the tests at any other database by setting `IDENTITY_DB_CONNECTION` manually or editing `Tests/appsettings.Test.json`.
