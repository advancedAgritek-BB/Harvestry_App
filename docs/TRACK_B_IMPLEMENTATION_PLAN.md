# Track B Implementation Plan — Core MVP (12 Weeks)

**Version:** 1.1  
**Date:** 2025-10-07  
**Status:** 🚧 In Progress (FRP-01-04 Complete)  
**Pilot Site:** Denver Grow Co., Colorado (METRC)

---

## Executive Summary

Track B delivers the **Core MVP** with 12 FRPs across 12 weeks, incorporating three critical adjustments:

- **(A1)** W6 HIL interlock chaos drills with firmware sign-off gate before irrigation go-live
- **(A2)** QBO recon SLO ≤0.5% daily + minimal dashboard (variance KPI, alert, DLQ view)
- **(A3)** UI performance budgets tied to SciChart (chart mount p95 < 1.5s, realtime push p95 < 1.5s) — **deferred to future frontend implementation**

**Critical Path:** FRP-01 → FRP-02 → FRP-05 → FRP-06 → FRP-09 → FRP-10 → Pilot Cutover

---

## Implementation Progress (as of 2025-10-07)

### Overall Status: 🚧 **31.7% Complete** (114/360 items)

| Sprint | Weeks | FRPs | Status | Completion |
|--------|-------|------|--------|------------|
| **S1** | W0-W2 | FRP-01, W0 Foundation | 🚧 Partially Complete | **FRP-01:** 100% ✅ **W0:** 13.6% 🚧 |
| **S2** | W3-W4 | FRP-02, FRP-03 | ✅ **COMPLETE** | **FRP-02:** 100% ✅ **FRP-03:** 100% ✅ |
| **S3** | W5-W6 | FRP-04, FRP-05 | 🚧 In Progress | **FRP-04:** 100% ✅ **FRP-05:** 0% ⏳ |
| **S4** | W7-W8 | FRP-06, FRP-07 | ⏳ Not Started | 0% |
| **S5** | W9-W10 | FRP-08, FRP-09 | ⏳ Not Started | 0% |
| **S6** | W11-W12 | FRP-10, FRP-15 + Pilot | ⏳ Not Started | 0% |

### FRP Completion Summary

| FRP | Feature | Owner | Status | Items | Completion |
|-----|---------|-------|--------|-------|------------|
| **FRP-01** | Identity/RLS/ABAC | Core Platform | ✅ **COMPLETE** | 32/32 | **100%** |
| **FRP-02** | Spatial/Equipment | Core Platform | ✅ **COMPLETE** | 26/26 | **100%** |
| **FRP-03** | Genetics/Batches | Core Platform | ✅ **COMPLETE** | 28/28 | **100%** (All requirements met) |
| **FRP-04** | Tasks/Slack | Workflow & Messaging | ✅ **COMPLETE** | 28/28 | **100%** |
| **FRP-05** | Telemetry | Telemetry & Controls | ⏳ Not Started | 0/30 | 0% |
| **FRP-06** | Irrigation/HIL | Controls | ⏳ Not Started | 0/46 | 0% |
| **FRP-07** | Inventory/GS1 | Core Platform | ⏳ Not Started | 0/30 | 0% |
| **FRP-08** | Processing | Core Platform | ⏳ Not Started | 0/21 | 0% |
| **FRP-09** | Compliance/COA | Integrations | ⏳ Not Started | 0/27 | 0% |
| **FRP-10** | QBO | Integrations | ⏳ Not Started | 0/32 | 0% |
| **FRP-15** | Notifications | Integrations | ⏳ Not Started | 0/22 | 0% |

### Key Milestones

- ✅ **FRP-01 Complete:** September 29, 2025 (38% ahead of schedule!)
- 🚧 **W0 Foundation:** In progress (golden harness, seed data, OTel)
- ✅ **FRP-04 Complete:** October 7, 2025 (task workflows, messaging, Slack notifications)
- 🎯 **Next:** FRP-05 telemetry ingest slice kickoff (W6)
- 🎯 **Critical:** FRP-06 HIL drills + firmware sign-off (W7-W8)
- 🎯 **Pilot Go-Live:** Week 12 (target)

### Critical Path Health: 🟢 **ON TRACK**

- ✅ FRP-01 delivered ahead of schedule
- 🚧 W0 foundation work in progress
- 🟢 No blockers identified
- 🟢 Team capacity confirmed
- ⚠️ Golden harness build needs to start (W0-W5 target)

---

## Team Composition

| Squad | Developers | Responsibilities |
|-------|-----------|------------------|
| **Backend/.NET** | 4 | Core Platform (Identity, Spatial, Inventory, Processing) |
| **Frontend/React** | 3 | **ON HOLD** — Design in progress; APIs will be ready |
| **Hardware/Firmware** | 2 | Golden harness build, HIL drills, firmware sign-off, device simulation |
| **DevOps/SRE** | 3 | CI/CD, observability, DR, security hardening, monitoring |

**Active Track A Squads (Reuse):**

- Core Platform
- Telemetry & Controls
- Integrations
- Workflow & Messaging
- Hardware/Firmware
- SRE/Security

---

## 12-Week Sprint Breakdown

| Sprint | Weeks | FRPs | Goal | Owners |
|--------|-------|------|------|--------|
| **S1** | W0-W2 | FRP-01, W0 Foundation | Identity/RLS/ABAC, golden harness readiness | Core Platform + Hardware/Firmware |
| **S2** | W3-W4 | FRP-02, FRP-03 | Spatial model, batch/strain management | Core Platform |
| **S3** | W5-W6 | FRP-04, FRP-05 | Task workflows, telemetry load readiness | Workflow & Telemetry |
| **S4** | W7-W8 | FRP-06, FRP-07 | Irrigation (open-loop + HIL), Inventory | Controls & Core |
| **S5** | W9-W10 | FRP-08, FRP-09 | Processing, Compliance/COA | Core & Integrations |
| **S6** | W11-W12 | FRP-10, FRP-15 + Pilot | QBO sync, Notifications, Pilot cutover | Integrations + All squads |

---

## Hard Dependencies (Critical Path)

```
FRP-01 (Identity/RLS)
    ├── FRP-02 (Spatial)
    │   ├── FRP-05 (Telemetry)
    │   ├── FRP-06 (Irrigation) ← **CRITICAL**
    │   └── FRP-07 (Inventory)
    ├── FRP-04 (Tasks)
    │   ├── FRP-06 (Irrigation)
    │   └── FRP-15 (Notifications)
    └── FRP-07 (Inventory) + FRP-08 (Processing)
        ├── FRP-09 (Compliance) ← **CRITICAL**
        └── FRP-10 (QBO) ← **CRITICAL**
            └── W12 Pilot Cutover
```

**Critical Path Risks:**

1. **FRP-01 delay** → All downstream FRPs blocked
2. **FRP-05 telemetry load/alert gate fails** → Irrigation start blocked, hardware idle
3. **FRP-06 HIL failure** → Irrigation cannot enable → Pilot blocked
4. **FRP-10 QBO variance > 0.5%** → Financial integrity issue → Go-Live blocked

---

## W0 Foundation Setup (Pre-FRP Work)

### Golden Harness Build (A1 Requirement)

**Owner:** Hardware/Firmware + DevOps  
**Duration:** W0-W1 (target 80 engineer-hours)  
**Deliverable:** Physical test rig with:

- Relay boards for E-STOP/door simulation
- PoE↔AC failover injection
- MQTT broker kill switch
- VLAN flap simulator
- Logging infrastructure (FRAM + timeseries DB)

**Acceptance:** Can inject faults remotely; logs persist; device recovers autonomously  
**Exit Criteria:** 12 scripted chaos scenarios executed, firmware sign-off checklist drafted, runbook published

---

### Seed Data Script (R3)

**Owner:** Core Platform  
**Duration:** W0 (16 engineer-hours)  
**Deliverable:** `scripts/seed/seed-pilot-site.sql`

**Contents:**

```sql
-- 1 Organization: Denver Grow Co. (Colorado, METRC)
-- 1 Site: Denver Main Facility
-- 2 Rooms: Veg (4 zones), Flower (6 zones)
-- 10 Users: 4 Operators (badges), 3 Managers, 2 Compliance, 1 Admin
-- 3 Strains: Blue Dream, OG Kush, Gorilla Glue
-- 2 Batches: BD-V-001 (Veg), OG-F-002 (Flower)
-- Equipment: 10 sensors (temp/humid/EC/pH/moisture), 5 valves, 2 pumps
-- 20 Inventory Lots: 10 inputs (nutrients, media), 5 WIP, 5 FG
```

**Integration:** Run in CI before every test suite; idempotent (DROP CASCADE + INSERT)  
**Exit Criteria:** CI job green, pilot personas validated with Product, reseed instructions documented

---

### Track A Gap Closure

**Owner:** SRE + Integrations  
**Duration:** W0-W1 (40 engineer-hours)  

| Gap | Action | Deliverable |
|-----|--------|-------------|
| Alert routing | Wire Slack/PagerDuty webhooks | Test alert in #alerts-test |
| OpenTelemetry | Instrument services with OTel SDK | Traces in Jaeger |
| Unit test coverage | Add tests to reach 80%+ | CI fails if < 80% |

**Exit Criteria:** Slack alert observed in #alerts-test, Jaeger spans recorded for identity service, coverage gate enforced in CI pipeline

---

## FRP-by-FRP Implementation Guide

### FRP-01: Identity, Roles, RLS/ABAC (W0-W1) ✅ **COMPLETE**

**Status:** ✅ **100% COMPLETE** (September 29, 2025)  
**Owner:** Core Platform/Identity Squad  
**Actual Effort:** ~32 hours (38% ahead of estimate)

**Goal:** Multi-tenant security with site-scoped RLS and ABAC overlays for high-risk actions.

**Delivered:**

**Migrations:** ✅

- `users`, `roles`, `user_sites`, `user_roles` with RLS policies
- `badges`, `badge_credentials`, `device_sessions`
- `sops`, `trainings`, `assignments`, `quizzes`, `signoffs`
- `audit_logs` with `row_hash`, `prev_hash` for tamper-evident chain
- `authorization_audit_entries` for high-risk action tracking

**Services:** ✅

```
Core Platform/Identity/ (75 files, 10,563 lines)
├── Application/Services/
│   ├── PolicyEvaluationService.cs       # ABAC engine (416 lines)
│   ├── BadgeAuthService.cs              # Badge login/revoke (340 lines)
│   ├── TaskGatingService.cs             # Check SOPs/training
│   └── [12 additional services]
├── Domain/Entities/
│   ├── User.cs, Role.cs, Badge.cs, Session.cs
│   ├── Site.cs, UserSite.cs
│   ├── Sop.cs, Training.cs, Assignment.cs
│   ├── AuditLog.cs, AuthorizationAuditEntry.cs
│   └── [15 additional entities]
├── Infrastructure/Persistence/
│   ├── 12 repository classes with RLS
│   ├── IdentityDataSourceFactory.cs (NpgsqlDataSource)
│   ├── JsonUtilities.cs (canonical JSON serialization)
│   ├── DatabaseRepository.cs (retry with backoff)
│   └── [8 additional infrastructure files]
├── Infrastructure/Jobs/
│   ├── AuditChainVerificationJob.cs     # Nightly hash verification
│   ├── SessionCleanupJob.cs             # Hourly session cleanup
│   └── BadgeExpirationNotificationJob.cs # Daily notifications
└── API/
    ├── Controllers/ (4 files)
    │   ├── AuthController.cs            # Badge login endpoints
    │   ├── UsersController.cs           # User CRUD with ABAC
    │   ├── BadgesController.cs          # Badge provisioning/revocation
    │   └── PermissionsController.cs     # Two-person approval
    ├── Validators/ (8 files)
    │   ├── ValidationConstants.cs       # Centralized patterns
    │   └── [7 FluentValidation validators]
    ├── Middleware/
    │   ├── RlsContextMiddleware.cs
    │   └── ErrorHandlingMiddleware.cs
    └── Program.cs (263 lines, full DI setup)
```

**Shared Kernel Enhancements:** ✅

```
src/shared/kernel/
├── Serialization/
│   └── CanonicalJsonSerializer.cs       # Deterministic JSON (273 lines)
└── Domain/
    └── Entity.cs                        # Updated with canonical hashing
```

**API Contracts:** ✅

```yaml
# POST /auth/badge-login
request:
  badgeId: "string"
  deviceId: "string"
response:
  token: "jwt"
  sessionId: "uuid"
  user: { id, name, roles[], sites[] }

# GET /auth/sessions
response:
  sessions: [{ id, userId, deviceId, startedAt, lastActivity }]

# POST /auth/badge-revoke
request:
  badgeId: "string"
response:
  revokedSessionIds: ["uuid"]

# POST /users/{id}/permissions/request-approval
request:
  action: "string"
  resourceId: "uuid"
  reason: "string"
response:
  approvalRequestId: "uuid"
  status: "Pending"
```

**Testing:** ✅ **Comprehensive**

- **Unit Tests:** 6 files, ~800 lines, ≥90% coverage
  - PolicyEvaluationService (20+ ABAC scenarios)
  - TaskGatingService
  - BadgeAuthService
  - Domain entities (User, Badge, Session)
- **Integration Tests:** 7 files, ~490 lines
  - RLS fuzz tests (cross-site access → 403, 20+ scenarios)
  - Badge login flow E2E
  - Two-person approval workflow
  - Test infrastructure with seeding
- **Canonical JSON Tests:** 53 tests
  - CanonicalJsonSerializerTests (20 tests)
  - JsonUtilitiesTests (11 tests)
  - AuditChainVerificationTests (8 tests)
  - Integration tests (5 tests)
  - Backward compatibility tests (9 tests)

**Production Polish:** ✅

- CORS policy configured
- Error handling middleware (ProblemDetails RFC)
- FluentValidation (8 validators)
- Serilog structured logging (JSON)
- Rate limiting (sliding window)
- ValidationConstants centralization
- Health checks (database + migrations)
- OpenAPI/Swagger documentation

**Key Innovations:**

1. **Canonical JSON Serialization** - Eliminates audit hash false positives
2. **NpgsqlDataSource** - Modern connection pooling
3. **Retry Logic** - 7 PostgreSQL error codes with exponential backoff
4. **ValidationConstants** - Centralized patterns for reuse

**Acceptance Criteria Met:** ✅

- ✅ Cross-site read/write blocked (RLS) - 20+ test scenarios passing
- ✅ Gated task shows explicit reason - E2E tests passing
- ✅ Badge revoke ends sessions within 5s - Integration tests passing
- ✅ Audit chain verifies nightly - Background job scheduled
- ✅ Canonical JSON hashing - 100% backward compatible, 0% false positives
- ✅ Two-person approval workflow - Complete with audit trail
- ✅ API p95 response time < 200ms - Performance validated

**Documentation:** ✅

- FRP-01 Completion Certificate
- Canonical JSON Implementation Guide
- Canonical JSON Migration Guide
- Polish Session Summary
- API documentation (OpenAPI/Swagger)

**Artifacts:**

- `docs/FRP01_COMPLETION_CERTIFICATE.md`
- `docs/FRP01_POLISH_SESSION_SUMMARY.md`
- `docs/CANONICAL_JSON_COMPLETE.md`
- `docs/architecture/canonical-json-hashing.md`
- `docs/migrations/canonical-json-migration-guide.md`

**Ready For:** FRP-02 (Spatial Model)

---

### FRP-02: Spatial Model & Equipment Registry (W2-W3) 🚧 **IN PROGRESS**

**Status:** 🚧 **36% Complete** (8/26 items, ~4.5 hours invested)  
**Started:** September 30, 2025  
**Owner:** Core Platform/Spatial Squad  
**Estimated Remaining:** 24 hours (revised plan)

**Goal:** Model sites→rooms→zones→racks→bins and register equipment.

**Progress Summary:**

- ✅ **Phase 1-2 Complete:** Database schema (3 migrations) + Domain layer (5 entities)
- 🚧 **Phase 2.5 Added:** Pre-slice foundation work (FromPersistence + DTOs + Config)
- 🚧 **Phase 3-7 Remaining:** Services, Repositories, API, Validation, Testing

**Key Achievements:**

- ✅ Universal location hierarchy (cultivation AND warehouse paths)
- ✅ Equipment type registry with 40+ fault codes
- ✅ 5 built-in templates (HSES12, HSEA24, HydroCore, RoomHub, EdgePods)
- ✅ Device twin pattern for discovered capabilities
- ✅ Many-to-many valve-zone routing matrix
- ✅ All schema fixes applied (proper FK alignment with FRP-01)

**Detailed Status:** See `docs/FRP02_CURRENT_STATUS.md`  
**Completion Plan:** See `docs/FRP02_COMPLETION_PLAN.md`  
**Execution Plan:** See `docs/FRP02_EXECUTION_PLAN.md` (UPDATED)  
**Status Update:** See `docs/FRP02_STATUS_UPDATE.md` (NEW)

**Execution Plan Refined (2025-09-30):**

- ✅ Added Pre-Slice Setup: FromPersistence factories, DTO mappers, DI configuration (90 min)
- ✅ Strengthened Multi-Tenancy: Site-scoped routes (`/api/sites/{siteId}/rooms/{roomId}`)
- ✅ Added Response DTOs: Proper API/domain separation (`RoomResponse`, `LocationResponse`)
- ✅ Realistic Time Estimates: 21h → 28.5h (accounts for all work, not just features)
- ✅ Timeline: 4 days → 5 days (includes foundation work + testing)

**Latest Review (2025-09-30):**

- ⭐⭐⭐⭐⭐ Quality Grade: EXCELLENT (5/5)
- ✅ All Quality Gates PASSED (40/40 points)
- 🟢 Velocity: 61% faster than FRP-01 at same completion %
- ✅ Test automation infrastructure added
- ✅ Execution plan refined with production-ready patterns
- ✅ **APPROVED TO PROCEED with Pre-Slice Setup → Slice 1**

**Reuse:** Track A facility bootstrap scripts, validation middleware, shared Terraform modules for Timescale Cloud

**Migrations:**

- `rooms`, `zones`, `racks`, `bins`, `inventory_locations`
- `equipment_registry`, `equipment_calibrations`, `device_health`
- `valve_zone_mappings`, `fault_reason_codes`

**Services:**

```
Core Platform/Spatial/
├── Application/Services/
│   ├── SpatialHierarchyService.cs       # Room→zone tree
│   ├── EquipmentRegistryService.cs      # CRUD equipment
│   ├── CalibrationService.cs            # Track calibrations
│   └── ValveZoneMappingService.cs       # Map valves→zones
├── Domain/Entities/
│   ├── Room.cs, Zone.cs, Rack.cs, Bin.cs
│   └── EquipmentItem.cs, Calibration.cs
└── API/Controllers/
    ├── SpatialController.cs             # Hierarchy CRUD
    └── EquipmentController.cs           # Equipment endpoints
```

**API Contracts:**

```yaml
# POST /sites/{siteId}/rooms
request:
  code: "VEG-1"
  name: "Veg Room 1"
  roomType: "Veg"
response:
  id: "uuid"

# POST /equipment
request:
  code: "TEMP-001"
  type: "Temperature Sensor"
  zoneId: "uuid"
  calibrationDueDate: "2025-12-31"
response:
  id: "uuid"

# GET /equipment/{id}/calibrations
response:
  calibrations: [{ id, date, result, nextDue }]
```

**Testing:**

- **Unit:** Spatial tree traversal
- **Integration:** Equipment linkage (create room → zones → equipment)
- **E2E:** Calibration tracking

**Acceptance:**

- ✅ Device heartbeat visible
- ✅ Calibration logs retrievable
- ✅ Valve→zone mapping correct
- ✅ RLS blocks cross-site access

---

### FRP-05: Telemetry Ingest & Rollups (W5-W6)

**Goal:** Ingest climate/substrate streams with 1m/5m/1h rollups; realtime fan-out.

**Reuse:** Track A MQTT ingestion service, WAL fan-out prototype, existing Grafana telemetry dashboards

**Migrations:**

- `sensor_streams`, `sensor_readings` (TimescaleDB hypertable)
- Continuous aggregates: `sensor_readings_1m`, `sensor_readings_5m`, `sensor_readings_1h`
- Compression policies (7d), retention policies (90d raw, 730d rollups)
- `alert_rules`, `alert_instances`

**Services:**

```
Telemetry & Controls/
├── Sensors/
│   ├── Application/Services/
│   │   ├── TelemetryIngestService.cs      # MQTT/HTTP/SDI-12 adapters
│   │   ├── NormalizationService.cs        # Unit coercion
│   │   └── IdempotencyService.cs          # Dedupe
│   └── Infrastructure/DeviceAdapters/
│       ├── MqttAdapter.cs
│       ├── HttpAdapter.cs
│       └── Sdi12Adapter.cs
├── Environment/
│   ├── Application/Services/
│   │   ├── AlertEvaluationService.cs      # Rule evaluation
│   │   └── RollupFreshnessMonitor.cs      # Lag tracking
│   └── WalFanoutService.cs                # Realtime push (WS/SSE)
```

**API Contracts:**

```yaml
# POST /telemetry/ingest
request:
  deviceId: "string"
  readings: [
    { streamId, value, unit, timestamp }
  ]
response:
  accepted: 42
  duplicates: 0

# WebSocket /realtime/subscribe
message:
  siteId: "uuid"
  streamIds: ["uuid", ...]
push:
  { streamId, value, unit, timestamp }
```

**Testing:**

- **Unit:** Normalization logic
- **Integration:** Ingest → rollup → query
- **Load:** k6 ingest (10k msg/s, p95 < 1.0s)
- **Contract:** WebSocket deterministic scenarios

**Exit Gate:** k6 run sustained for 15 minutes, burn-rate alerts verified in staging, telemetry runbook updated

**Acceptance:**

- ✅ Ingest p95 < 1.0s
- ✅ Rollup freshness < 60s
- ✅ Realtime push p95 < 1.5s
- ✅ Deviation alerts fire correctly

---

### FRP-04: Tasks, Messaging & Slack (W5-W6)

**Goal:** Core task engine with Slack notify-only integration.

**Reuse:** Track A workflow engine primitives, Slack webhook adapters, existing outbox infrastructure

**Migrations:**

- `tasks`, `task_dependencies`, `task_watchers`
- `conversations`, `messages`, `message_attachments`
- `slack_message_bridge_log` (idempotent mapping)

**Services:**

```
Workflow & Messaging/
├── Tasks/
│   ├── Application/Services/
│   │   ├── TaskLifecycleService.cs        # ✅ Create/start/complete
│   │   └── TaskGatingResolver.cs          # ✅ Check SOPs
│   └── Domain/Entities/
│       ├── Task.cs, TaskDependency.cs     # ✅ Implemented
│       └── Conversation.cs, Message.cs
├── Slack-Bridge/
│   ├── Application/Services/
│   │   ├── SlackNotificationService.cs    # Notify-only
│   │   └── SlackOutboxWorker.cs           # Retry failed
│   └── Infrastructure/External/
│       └── SlackApiClient.cs
```

**API Contracts:**

```yaml
# POST /tasks
request:
  title: "string"
  assigneeId: "uuid"
  dueDate: "iso-date"
  requiredSopIds: ["uuid"]
response:
  id: "uuid"

# PUT /tasks/{id}/start
response:
  status: "InProgress" | "Blocked"
  blockReason: "Missing SOP: XYZ"

# PUT /tasks/{id}/complete
response:
  status: "Completed"
```

**Testing:**

- **Unit:** Task lifecycle state machine
- **Contract:** Slack API mock (verify idempotency)
- **E2E:** Blocked task → Slack notify

**Acceptance:**

- ✅ Task events notify Slack p95 < 2s
- ✅ Blocked reasons explicit
- ✅ Gating works E2E

---

### FRP-06: Irrigation Orchestrator (Open-Loop) + HIL (W7-W8) ⚠️ **CRITICAL**

**Goal:** Groups/programs/schedules, interlocks, safe aborts, HIL validation.

**Reuse:** Track A device command topics, command queue infrastructure, existing interlock specification library

**Migrations:**

- `mix_tanks`, `injector_channels`, `nutrient_products`
- `irrigation_groups`, `irrigation_programs`, `irrigation_schedules`
- `irrigation_runs`, `irrigation_step_runs`
- `interlock_events`, `device_commands`

**Services:**

```
Telemetry & Controls/Irrigation/
├── Application/Services/
│   ├── IrrigationOrchestratorService.cs   # Command queue
│   ├── ScheduleTriggerService.cs          # Time/phase triggers
│   └── ManualApprovalService.cs           # Optional gating
├── Application/Sagas/
│   └── IrrigationAbortSaga.cs             # Safe abort compensator
├── Interlocks/
│   ├── Domain/Services/
│   │   └── InterlockEvaluationService.cs  # Safety checks
│   └── Domain/Specifications/
│       ├── TankLevelSpec.cs
│       ├── EcPhBoundsSpec.cs
│       ├── Co2LockoutSpec.cs
│       └── MaxRuntimeSpec.cs
└── Infrastructure/DeviceAdapters/
    └── MqttCommandAdapter.cs              # MQTT → HydroCore/RoomHub
```

**Interlocks (Hard Requirements):**

| Interlock | Behavior | Recovery |
|-----------|----------|----------|
| **E-STOP open** | Immediate OFF, FAULT latched | Manual re-arm |
| **Door open** | Immediate OFF, FAULT latched | Manual re-arm |
| **Tank level < threshold** | Abort run, log reason | Auto-retry when level restored |
| **EC/pH out of bounds** | Abort run, alert raised | Manual override after review |
| **CO₂ exhaust lockout** | Block start, log reason | Wait for exhaust clear |
| **Max runtime exceeded** | Safe close valves, log | Manual restart |
| **Concurrency cap (INT-150VA)** | Reject > 1 HL + 6 STD | Queue next command |

**API Contracts:**

```yaml
# POST /irrigation/programs
request:
  name: "string"
  groupId: "uuid"
  steps: [{ zoneId, duration, volume }]
response:
  id: "uuid"

# POST /irrigation/runs
request:
  programId: "uuid"
  manualApproval: false  # Site policy
response:
  runId: "uuid"
  status: "Queued" | "Running" | "Completed" | "Aborted"

# POST /irrigation/runs/{id}/abort
request:
  reason: "string"
response:
  stoppedAt: "iso-timestamp"
```

**HIL Chaos Drill Matrix (A1):**

| Test | Injection | Expected Behavior | Pass Criteria |
|------|-----------|-------------------|---------------|
| E-STOP | Open ESTOP loop | Valves OFF, FAULT latched | Log "ESTOP_OPEN", no water |
| Door | Open door input | Valves OFF, FAULT latched | Log "DOOR_OPEN", no water |
| PoE→AC | Pull PoE, AC present | Run continues, log PoE loss | No spurious actuation |
| AC→PoE | Pull AC, PoE present | Run continues, log AC loss | No spurious actuation |
| Broker loss | Kill MQTT broker | Local program continues | Queue reconciles on restore |
| VLAN flap | Disconnect network 30s | Local buffer, restore | Logs synced, no data loss |
| Tank low | Inject low-level signal | Abort, log "TANK_LOW" | Alert raised |
| Concurrency | Command 2 HL + 8 STD | Reject excess opens | Thermal < spec, log |

**Deliverable:** HIL report signed by Hardware/Firmware Lead → **Firmware Sign-Off Gate**

**Testing:**

- **Unit:** Interlock specifications (20+ scenarios)
- **Integration:** Orchestrator saga (run → abort → compensate)
- **HIL:** Full chaos matrix on golden harness
- **E2E:** End-to-end irrigation flow (schedule → run → complete)

**Acceptance:**

- ✅ Program executes with step monitoring
- ✅ Safe aborts close valves
- ✅ HIL report green (zero unsafe actuations)
- ✅ Audit trail complete
- ✅ Enqueue→ack p95 < 800ms

---

### FRP-07: Inventory, Scanning & GS1 Labels (W7-W8)

**Goal:** Unified locations, lots/balances, movements, FEFO, GS1 labels.

**Migrations:**

- `inventory_lots`, `inventory_balances`, `inventory_movements`
- `inventory_adjustments`, `lot_relationships` (splits/merges)
- `uom_definitions`, `uom_conversions`
- `barcode_settings`, `label_templates`

**Services:**

```
Core Platform/Inventory/
├── Application/Services/
│   ├── LotMovementService.cs              # Movements/splits/merges
│   ├── BalanceReconciliationService.cs    # Balance verification
│   └── ScanningService.cs                 # Barcode → movement
├── Domain/Services/
│   ├── UomConversionService.cs            # kg↔lb, L↔gal
│   └── FefoAllocationService.cs           # First-Expired-First-Out
└── Integrations/Labeling/
    └── Gs1LabelService.cs                 # GS1 AI codes + barcode
```

**API Contracts:**

```yaml
# POST /inventory/movements
request:
  sourceLotId: "uuid"
  destinationLocationId: "uuid"
  quantity: 10.5
  uom: "kg"
response:
  movementId: "uuid"

# POST /inventory/lots/split
request:
  sourceLotId: "uuid"
  quantities: [5.0, 3.0, 2.5]  # Must sum to source
response:
  childLotIds: ["uuid", "uuid", "uuid"]

# POST /inventory/scan
request:
  barcode: "01012345678901112110ABC123"  # GS1 format
response:
  lotId: "uuid"
  movement: { id, from, to, quantity }
```

**Testing:**

- **Unit:** UoM conversions (property-based tests)
- **Integration:** Balance reconciliation after splits
- **E2E:** Scan → movement → balance update

**Acceptance:**

- ✅ Balances reconcile after splits
- ✅ FEFO allocation works
- ✅ Scans update movements
- ✅ GS1 labels render correctly

---

### FRP-08: Processing & Manufacturing (W9-W10)

**Goal:** Process definitions, WIP/yields, labor & waste capture.

**Reuse:** Track A costing services, batch event stream, shared reporting warehouse datasets

**Migrations:**

- `process_definitions`, `process_runs`, `process_steps`
- `labor_logs`, `waste_events`
- `process_cost_snapshots` (materials + labor + overhead)

**Services:**

```
Core Platform/Processing/
├── Application/Services/
│   ├── ProcessRunService.cs               # Orchestrate runs
│   ├── YieldCalculationService.cs         # Input → output
│   └── CostRollupService.cs               # WIP → COGS
├── Application/Handlers/
│   └── InventoryTransformationHandler.cs  # Consume inputs, create FG
└── Domain/Entities/
    ├── ProcessDefinition.cs, ProcessRun.cs
    └── LaborLog.cs, WasteEvent.cs
```

**API Contracts:**

```yaml
# POST /processing/runs
request:
  definitionId: "uuid"
  batchId: "uuid"
  inputLots: [{ lotId, quantity, uom }]
response:
  runId: "uuid"

# POST /processing/runs/{id}/complete
request:
  outputQuantity: 50.0
  outputUom: "kg"
  yieldRate: 0.82
response:
  outputLotId: "uuid"
  costSnapshot: { materials, labor, overhead, total }

# POST /processing/runs/{id}/labor
request:
  userId: "uuid"
  hours: 4.5
response:
  laborLogId: "uuid"
```

**Testing:**

- **Unit:** Yield calculations
- **Integration:** Input reconciliation (consumed correctly)
- **E2E:** Process run → outputs created → costs attached

**Acceptance:**

- ✅ Yields reconcile with inventory
- ✅ Labor and waste appear in cost rollups
- ✅ Rework lineage retained

---

### FRP-09: Compliance (METRC) & COA (W9-W10) ⚠️ **CRITICAL**

**Goal:** METRC sync framework, COA ingestion, holds, destruction.

**Reuse:** Track A METRC credential handler, queue worker skeletons, badge-based approval service

**Migrations:**

- `compliance_integrations`, `sync_queue`, `sync_events`
- `labs`, `lab_orders`, `lab_results`
- `holds`, `destruction_events`

**Services:**

```
Integrations/Compliance-METRC/
├── Application/Services/
│   ├── MetrcSyncService.cs                # Sync orchestrator
│   ├── CoaIngestionService.cs             # PDF/CSV parser
│   ├── HoldGatingService.cs               # Failed COA → HOLD
│   └── DestructionService.cs              # Two-person signoff
├── Infrastructure/External/
│   └── MetrcApiClient.cs                  # Colorado METRC
└── Infrastructure/Workers/
    └── ComplianceSyncWorker.cs            # Queue processor
```

**API Contracts:**

```yaml
# POST /compliance/metrc/sync
request:
  siteId: "uuid"
  syncType: "Plants" | "Packages" | "Transfers"
response:
  queuedEvents: 42

# POST /compliance/coa/upload
request:
  labOrderId: "uuid"
  file: "base64-pdf"
response:
  labResultId: "uuid"
  status: "Pass" | "Fail"
  holdsCreated: ["uuid"] if fail

# POST /compliance/destruction
request:
  lotIds: ["uuid"]
  reason: "Failed COA"
  initiatorBadgeId: "string"
  approverBadgeId: "string"  # Two-person
response:
  destructionId: "uuid"
  auditExportUrl: "/exports/{id}"
```

**Testing:**

- **Contract:** METRC API mock (retry/backoff, idempotency)
- **Integration:** COA fail → HOLD created
- **E2E:** Destruction with two-person signoff

**Acceptance:**

- ✅ Retry/backoff verified
- ✅ Failed COA → HOLD enforced
- ✅ Destruction logs exportable
- ✅ DLQ < 0.5% over 7 days

---

### FRP-10: QuickBooks Online (Item-Level) (W11) ⚠️ **CRITICAL**

**Goal:** OAuth2, item-level POs/Bills, idempotency, recon SLO ≤0.5%.

**Reuse:** Track A OAuth2 broker, accounting integration framework, existing QBO sandbox tenant

**Migrations:**

- `accounting_integrations`, `qbo_item_map`, `qbo_vendor_map`, `qbo_customer_map`
- `accounting_queue`, `accounting_events`
- `qbo_reconciliation_reports`

**Services:**

```
Integrations/QuickBooks/
├── Application/Services/
│   ├── QboSyncService.cs                  # Item-level sync
│   ├── QboReconciliationService.cs        # Variance calc (A2)
│   └── DlqReplayService.cs                # Retry failed events
├── Application/Handlers/
│   └── ReceivingToBillHandler.cs          # PO receipt → Bill
├── Infrastructure/External/
│   └── QboApiClient.cs                    # OAuth2 + Request-ID
└── Infrastructure/RateLimiting/
    └── AdaptiveThrottlingService.cs       # Rate limit backoff
```

**API Contracts:**

```yaml
# POST /accounting/qbo/bills
request:
  receivingId: "uuid"
  poId: "uuid"
  vendorId: "uuid"
  lineItems: [{ itemId, qty, cost }]
response:
  qboBillId: "string"
  queueEventId: "uuid"

# GET /accounting/qbo/reconciliation
query:
  startDate: "2025-09-01"
  endDate: "2025-09-30"
response:
  variance: 0.003  # 0.3%
  erpTotal: 125000.00
  qboTotal: 124625.00
  delta: -375.00
  dlqCount: 2
```

**Recon SLO (A2):**

```sql
-- Daily variance calculation
WITH erp_totals AS (
  SELECT SUM(amount) as erp_total
  FROM accounting_queue
  WHERE DATE(created_at) = CURRENT_DATE - 1
  AND status = 'Completed'
),
qbo_totals AS (
  SELECT SUM(amount) as qbo_total
  FROM qbo_sync_events
  WHERE DATE(synced_at) = CURRENT_DATE - 1
)
SELECT
  ABS(erp_total - qbo_total) / erp_total AS variance_pct
FROM erp_totals, qbo_totals;

-- Alert if variance > 0.005 (0.5%)
```

**Minimal Recon Dashboard (A2):**

```yaml
# Grafana panel: dashboards/qbo-recon-mvp.json
Panels:
  - Daily Variance % (stat panel, red if > 0.5%)
  - DLQ Count (stat panel, link to /admin/dlq)
  - Alert annotation (variance breach at 16:00 local)
```

**Testing:**

- **Contract:** QBO API mock (Request-ID idempotency)
- **Integration:** Receiving → Bill creation (amounts correct)
- **E2E:** 7-day SLO validation (variance ≤ 0.5%)

**Acceptance:**

- ✅ Receiving creates correct QBO Bill
- ✅ DLQ < 0.1% over 7 days
- ✅ Variance ≤ 0.5% daily
- ✅ Dashboard green

---

### FRP-15: Notifications & Escalations (W11)

**Goal:** Routing policies, escalation chains, quiet hours, SMS critical.

**Migrations:**

- `notification_rules`, `notification_instances`
- `escalation_chains`, `quiet_hours`, `notification_preferences`

**Services:**

```
Integrations/Notifications/
├── Application/Services/
│   ├── NotificationRoutingService.cs      # Policy engine
│   ├── EscalationService.cs               # Chain execution
│   └── DedupBundlingService.cs            # Storm control
└── Infrastructure/External/
    ├── TwilioSmsAdapter.cs                # SMS critical
    └── PagerDutyAdapter.cs                # On-call
```

**API Contracts:**

```yaml
# POST /notifications/rules
request:
  name: "Critical Alert Escalation"
  conditions: { severity: "Critical" }
  channels: ["Slack", "SMS"]
  escalationChainId: "uuid"
response:
  ruleId: "uuid"

# POST /notifications/send
request:
  alertId: "uuid"
  severity: "Critical"
response:
  instanceId: "uuid"
  channels: ["Slack", "SMS"]
  escalatedAt: null
```

**Testing:**

- **Unit:** Policy evaluation (20+ scenarios)
- **Unit:** Dedup/bundling (storm control)
- **Integration:** Escalation chain (timeout → escalate)
- **E2E:** End-to-end escalation flow

**Acceptance:**

- ✅ Monitor mode passes
- ✅ Quiet hours verified
- ✅ Escalation chains work
- ✅ Policy unit tests green

---

## W12: Pilot Readiness & UAT

### DR Drill (RPO ≤5m, RTO ≤30m)

**Owner:** SRE

**Procedure:**

1. Simulate primary region failure
2. Failover to warm replica
3. Verify data integrity (last 5m preserved)
4. Validate all services healthy
5. Measure RTO (target ≤30m)

**Deliverable:** DR drill report with sign-off

---

### UAT Execution

**Owner:** TPM/Delivery + Customer Onboarding

**Test Scenarios:**

1. **Identity:** Badge login, task gating (missing SOP)
2. **Irrigation:** Create program, run with manual approval, safe abort
3. **Inventory:** Scan lot, move location, split lot
4. **Processing:** Run process, capture labor, verify yield
5. **Compliance:** Upload COA (fail → HOLD), destruction with two-person
6. **QBO:** Receive PO → Bill creation, verify variance < 0.5%

**Deliverable:** UAT sign-off from pilot site operators

---

### Go/No-Go Checklist

**Owner:** TPM/Delivery Lead

| Criterion | Status | Evidence |
|-----------|--------|----------|
| DR drill passed (RPO ≤5m, RTO ≤30m) | ☐ | DR report |
| SLOs met for 7 consecutive days | ☐ | Grafana dashboard |
| UAT signed by pilot site | ☐ | UAT report |
| Firmware HIL sign-off | ☐ | HIL report |
| On-call rotation staffed | ☐ | PagerDuty schedule |
| Runbooks published | ☐ | docs/ops/runbooks/ |
| Feature flags configured | ☐ | Unleash dashboard |
| Seed data loaded | ☐ | DB verification |

**Decision:** ✅ Go-Live | ❌ Hold (specify blocker)

---

## Pilot Cutover

**Site:** Denver Grow Co., Colorado  
**Scope:** Open-loop irrigation, Slack notify-only, QBO item-level, COA gating, core dashboards  
**Flags Enabled:**

- `frp_01_identity_enabled = true`
- `frp_06_irrigation_enabled = true` (after firmware sign-off)
- `frp_10_qbo_sync_enabled = true` (sandbox validated)
- `closed_loop_ecph_enabled = false` (Phase 2)
- `slack_mirror_mode = false` (notify-only)

**Monitoring:** 48h intensive on-call with incident playbooks active

---

## Risk Mitigation Plan

| Risk | Mitigation | Contingency |
|------|------------|-------------|
| **Hardware delays in HIL drills** | Golden harness W0-W5; prioritize firmware sign-off | Sim-only fallback; delay W6 by 1 week |
| **Sequence bottlenecks from dependencies** | Dependency graph enforcement; parallel non-dependent work | Buffer in W8 for catch-up |
| **QBO recon variance spikes** | SLO monitoring + alerts; sandbox replays in CI | Manual reconciliation; pause sync |
| **Chart perf regressions** | CI tests (R5); dynamic imports; p95 monitoring | *Deferred to frontend work* |
| **Pilot data inconsistencies** | Automated seed script; greenfield approach; E2E tests | Data reset + reseed |

---

## Observability & SLOs

### Golden Signals

| Metric | SLO | Alert Threshold |
|--------|-----|-----------------|
| Telemetry ingest p95 | < 1.0s | > 1.2s for 5m |
| Realtime push p95 | < 1.5s | > 2.0s for 5m |
| Command enqueue→ack p95 | < 800ms | > 1.0s for 5m |
| Task/messaging p95 | < 300ms | > 500ms for 5m |
| QBO recon variance | ≤ 0.5% daily | > 0.5% |
| Rollup freshness | < 60s | > 90s for 5m |

### Dashboards

1. **SLO Overview** (ingest, push, commands, tasks)
2. **Burn Rate Monitoring** (1h/6h windows)
3. **Database Performance** (rollup freshness, replication lag)
4. **Queue Health** (Slack, QBO, METRC DLQ depth)
5. **QBO Recon MVP** (variance %, DLQ count) ← **A2 Dashboard**

---

## Security & Compliance

### RLS/ABAC

- All tables: site-scoped RLS policies
- High-risk actions: ABAC gating (destruction, manual override, COA release)
- Two-person signoff: ABAC enforces dual badge auth

### Audit Hash Chain

- `audit_logs.prev_hash` + `audit_logs.row_hash`
- Nightly verification job: `AuditChainVerificationJob.cs`
- WORM anchor optional (S3 Glacier)

### Token Rotation

- Badge credentials: 90-day rotation
- API tokens: 30-day rotation
- QBO OAuth2: refresh on expiry

### SAST

- CodeQL in CI (all PRs)
- Gitleaks + TruffleHog secret scanning
- SBOM generation

---

## Acceptance Criteria Summary

| FRP | Acceptance | Evidence |
|-----|------------|----------|
| **FRP-01** | Cross-site blocked; gated tasks show reason; audit verifies | RLS fuzz + E2E tests |
| **FRP-02** | Equipment heartbeat visible; calibration retrievable | Integration tests |
| **FRP-05** | Rollup freshness < 60s; realtime p95 < 1.5s | 7-day SLO validation |
| **FRP-04** | Task events notify Slack p95 < 2s; blocking works | E2E tests |
| **FRP-06** | Program executes; safe aborts; HIL green; p95 < 800ms | HIL report + E2E |
| **FRP-07** | Balances reconcile; FEFO works; scans update | E2E inventory flow |
| **FRP-08** | Yields reconcile; labor/waste in costs | E2E processing |
| **FRP-09** | Retry/backoff verified; COA fail → HOLD; DLQ < 0.5% | Contract + E2E |
| **FRP-10** | Bills correct; DLQ < 0.1%; variance ≤ 0.5% | 7-day recon report |
| **FRP-15** | Quiet hours; escalation chains; policy tests green | Unit + E2E |

---

## Artifacts & Documents

1. **HIL Chaos Drill Playbook** → `docs/hardware/hil-chaos-playbook.md`
2. **API Contracts (OpenAPI)** → `docs/api/contracts/track-b-*.yaml`
3. **Seed Data Script** → `scripts/seed/seed-pilot-site.sql`
4. **Testing Strategy Matrix** → `docs/testing/track-b-strategy.md`
5. **QBO Recon Dashboard** → `src/infrastructure/grafana/dashboards/qbo-recon-mvp.json`
6. **Runbooks** → `docs/ops/runbooks/frp-*.md`
7. **UAT Checklist** → `docs/pilot/uat-checklist.md`

---

## Next Actions (Immediate)

1. ✅ **FRP-01 Complete** → Marked as done (September 29, 2025)
2. 🚧 **W0 Foundation Work:**
   - Start golden harness build (Hardware/Firmware) - **PRIORITY**
   - Create seed data script (Core Platform) - **PRIORITY**
   - Instrument services with OpenTelemetry (SRE)
   - Reach 80%+ unit test coverage (all squads)
3. 🚧 **Next Sprint (W2-W3):**
   - **FRP-02:** Spatial Model & Equipment Registry (Core Platform/Spatial Squad)
   - **FRP-03:** Genetics, Strains & Batches (Core Platform Squad)
4. 📅 **Sprint Planning:**
   - Confirm squad capacity for W2-W3
   - Schedule W5-W6 for FRP-04 (Tasks/Slack) + FRP-05 (Telemetry)
   - Ensure golden harness on track for W7 HIL drills
5. 📊 **Track Progress:**
   - Weekly sprint reviews
   - Daily standups
   - Bi-weekly stakeholder demos
   - Update completion checklist after each FRP

---

**Status Legend:**

- ✅ Approved / Completed
- 🚧 In Progress / Blocked
- ⏳ Not Started
- ⚠️ Critical Path / High Risk
- ❌ Failed / Rejected

---

## Lessons Learned from FRP-01 (Apply to Future FRPs)

### 🏆 Best Practices to Replicate

1. **Clean Architecture Pattern**
   - Domain → Application → Infrastructure → API separation
   - All dependencies via interfaces
   - Repository pattern for data access
   - Service layer for business logic

2. **Security-First Design**
   - RLS policies from day 1
   - ABAC for fine-grained permissions
   - Audit trail with tamper-evident hash chain
   - Generic error messages (no enumeration attacks)

3. **Canonical JSON for All Audit/Hashing**
   - Use `CanonicalJsonSerializer` for deterministic serialization
   - Eliminates false positives from dictionary ordering
   - 100% backward compatible
   - Reuse `ValidationConstants` pattern

4. **Production-Ready from Start**
   - Error handling middleware (ProblemDetails)
   - Structured logging (Serilog JSON)
   - Rate limiting (sliding window)
   - Health checks (database + migrations)
   - CORS policy
   - OpenAPI documentation

5. **Comprehensive Testing Strategy**
   - Unit tests: ≥90% coverage, focus on business logic
   - Integration tests: Database + RLS scenarios
   - E2E tests: Complete user workflows
   - Contract tests: External API mocks
   - Load tests: Performance validation

6. **Modern .NET Patterns**
   - NpgsqlDataSource for connection pooling
   - Retry logic with exponential backoff
   - FluentValidation for clean validation
   - BackgroundService for scheduled jobs
   - Async/await throughout

7. **Centralization for Reusability**
   - ValidationConstants for patterns
   - Shared kernel for common utilities
   - Repository base classes
   - Common middleware

### 📊 Velocity Insights

- **Estimation Accuracy:** FRP-01 was 38% faster than estimated
- **Why:** Clear architecture, patterns established, comprehensive planning
- **Apply Forward:** Use FRP-01 patterns to accelerate FRP-02+

### ⚠️ Risk Mitigations

1. **Golden Harness Build (W0-W5):**
   - Start immediately, don't delay
   - Parallel track with FRP-02/03
   - Firmware sign-off is a hard gate for FRP-06

2. **Seed Data Script (W0):**
   - Complete before FRP-02 starts
   - Required for all integration tests
   - Idempotent and CI-integrated

3. **OpenTelemetry Instrumentation:**
   - Add incrementally with each FRP
   - Don't wait until end
   - Required for production observability

4. **Unit Test Coverage:**
   - Maintain 80%+ from day 1
   - Don't accumulate technical debt
   - CI gate enforces threshold

### 🔄 Continuous Improvement

- **Weekly Reviews:** Update completion checklist after each FRP
- **Retrospectives:** Capture learnings, adjust approach
- **Documentation:** Keep artifacts current
- **Communication:** Daily standups + bi-weekly demos

---

**Last Updated:** 2025-09-30  
**Track B Lead:** Engineering Squads  
**Review Frequency:** Weekly sprint reviews + daily standups  
**Overall Progress:** 9.7% Complete (35/360 items)
