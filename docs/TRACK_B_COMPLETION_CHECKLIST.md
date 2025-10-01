# Track B Completion Checklist

**Track B: Core MVP - Feature Requirement Packages (FRPs)**

**Pilot Site:** Denver Grow Co., Colorado (METRC)  
**Duration:** 12 Weeks  
**Status:** 🚧 In Progress

---

## 🎯 Done When Criteria

Track B is complete when **all** of the following criteria are met:

1. All 10 FRPs delivered with acceptance criteria met
2. UAT signed off by pilot site operators
3. DR drill passed (RPO ≤5m, RTO ≤30m)
4. 7-day SLO validation successful in staging
5. Firmware HIL sign-off complete
6. Go/No-Go checklist 100% green

---

## ✅ FRP-01: Identity, Authentication & Authorization (W0-W1)

**Owner:** Core Platform/Identity Squad  
**Status:** ✅ **100% COMPLETE**

### Database Migrations

- [x] `users`, `roles`, `user_sites`, `user_roles` with RLS policies
- [x] `badges`, `badge_credentials`, `device_sessions`
- [x] `sops`, `trainings`, `assignments`, `quizzes`, `signoffs`
- [x] `audit_logs` with tamper-evident hash chain

### Domain & Application Layer

- [x] User, Role, Badge, Session domain entities
- [x] Email, PhoneNumber, BadgeCode value objects
- [x] PolicyEvaluationService (ABAC engine)
- [x] BadgeAuthenticationService
- [x] TaskGatingService
- [x] Canonical JSON serialization for audit hashing

### API Layer

- [x] AuthController (badge login/logout)
- [x] UsersController (CRUD with ABAC)
- [x] BadgesController (provisioning/revocation)
- [x] PermissionsController (two-person approval)
- [x] FluentValidation (8 validators)
- [x] OpenAPI/Swagger documentation

### Infrastructure

- [x] 12 repository classes with RLS
- [x] Connection pooling with NpgsqlDataSource
- [x] Retry logic with exponential backoff
- [x] 3 background jobs (audit verification, session cleanup, badge expiration)
- [x] Health checks (database + migration status)

### Testing

- [x] Unit tests (6 files, ≥90% coverage)
- [x] Integration tests (7 files, 20+ RLS scenarios)
- [x] Badge login flow E2E
- [x] Two-person approval workflow
- [x] Canonical JSON hashing tests (53 tests)

### Production Polish

- [x] CORS policy configured
- [x] Error handling middleware (ProblemDetails)
- [x] Serilog structured logging (JSON)
- [x] Rate limiting (sliding window)
- [x] ValidationConstants centralization
- [x] Backward compatible audit hash chain

**Status**: 32/32 complete (100%) ✅  
**Completion Date:** September 29, 2025  
**Actual Effort:** ~32 hours (38% ahead of estimate)

---

## ✅ FRP-02: Spatial Model & Equipment Registry (W2-W3)

**Owner:** Core Platform/Spatial Squad  
**Status:** ✅ **100% COMPLETE**  
**Started:** September 30, 2025  
**Completed:** October 1, 2025  
**Actual Effort:** ~29 hours (matched revised estimate + RLS polish)

### Database Migrations ✅

- [x] `rooms`, `inventory_locations` hierarchy with materialized-path trigger
- [x] `equipment`, `equipment_type_registry`, `equipment_channels`, `equipment_calibrations`, `valve_zone_mappings`
- [x] Reference data seeding (40+ fault codes, equipment templates)
- [x] RLS policies enforced across all spatial/equipment tables

### Domain Layer ✅

- [x] Enums for rooms, locations, equipment, calibration states
- [x] Entities: Room, InventoryLocation, Equipment, EquipmentChannel, Calibration, ValveZoneMapping
- [x] `FromPersistence` factories + invariant enforcement across aggregates

### Application Services ✅

- [x] SpatialHierarchyService (tenant-scoped hierarchy management)
- [x] EquipmentRegistryService (CRUD, telemetry, channel ops)
- [x] CalibrationService (recording, overdue analytics, equipment sync)
- [x] ValveZoneMappingService (routing CRUD, interlock warnings)

### Infrastructure ✅

- [x] Repositories for rooms, locations, equipment, channels, calibrations, valve mappings
- [x] SpatialDbContext retry/RLS orchestration + AsyncLocal accessor
- [x] NpgsqlDataSource pooling and configuration hooks

### API Layer ✅

- [x] Rooms & Locations controllers with tenant guards + hierarchy endpoints
- [x] Equipment controller (CRUD, heartbeat, network, channels, valve mappings)
- [x] Calibration controller (record/history/latest/overdue)
- [x] FluentValidation for rooms, locations, equipment, calibration, valve mappings

### Testing ✅

- [x] Unit: SpatialHierarchyServiceTests, EquipmentRegistryServiceTests, CalibrationServiceTests, ValveZoneMappingServiceTests
- [x] Integration: SpatialHierarchyIntegrationTests, CalibrationIntegrationTests, ValveZoneMappingIntegrationTests
- [x] Unit: RoomTests, InventoryLocationTests, EquipmentTests
- [x] Integration: EquipmentRegistryIntegrationTests, RlsSpatialTests, RlsEquipmentTests
- [x] All 28 Spatial tests passing (100%)
- [x] All 70 Identity tests passing (verified compatibility)
- [x] RLS policies fully functional with service_account context

### Acceptance Criteria ✅

- [x] Calibration logs retrievable with history + overdue alerts surfaced
- [x] Valve→zone mapping CRUD with interlock safety messaging
- [x] Device heartbeat visible in equipment status (heartbeat integration test added)
- [x] RLS blocks cross-site access (spatial + equipment repositories verified)
- [x] Hierarchy path auto-calculates correctly (integration path tests in place)
- [x] All database type mappings validated (inet, macaddr, enums)
- [x] 100% integration test pass rate achieved

**Status**: 28/28 complete (100%) ✅  
**Completion Date:** October 1, 2025  
**Actual Effort:** ~29 hours (on target with RLS refinements)

### Final Polish Completed ✅

**All critical work finished:**
- [x] RLS policies fully operational across all tables
- [x] Database type mappings (inet, macaddr, enums) corrected
- [x] Test seeder enhanced with proper role switching
- [x] Sessions table policies added (service_account access)
- [x] Authorization audit INSERT policy corrected
- [x] All 28 Spatial integration tests passing
- [x] All 70 Identity integration tests passing (verified compatibility)

**Total:** 100% complete, production-ready

### Execution Plan ✅

- **Detailed Execution Plan:** See `docs/FRP02_EXECUTION_PLAN.md`
- **Strategy:** 4 vertical slices (Spatial → Equipment → Calibration → Valve Mapping)
- **Timeline:** 5 days, 28.5 hours total
- **Current Phase:** Release prep & QA (post-slice wrap-up)

### Progress Review ✅

- **Review Date:** September 30, 2025
- **Quality Grade:** ⭐⭐⭐⭐⭐ EXCELLENT (5/5)
- **All Quality Gates:** ✅ PASSED (40/40 points)
- **Velocity:** Tracking to 28.5h (~27.5h invested; ahead of FRP-01 schedule)
- **Recommendation:** ✅ **PROCEED TO RELEASE PREP / UAT HANDOFF**
- **Details:** See `docs/FRP02_PROGRESS_REVIEW.md`

### Test Infrastructure ✅

- [x] Test automation script `scripts/test/run-with-local-postgres.sh`
- [x] Ephemeral PostgreSQL container for isolated runs
- [x] Automatic migration application for integration tests
- [x] CI/CD-ready configuration with `SPATIAL_DB_CONNECTION`

---

## ✅ FRP-03: Genetics, Strains & Batches (W3-W4)

**Owner:** Core Platform Squad  
**Status:** 🎯 Ready to Start

### Database Migrations

- [ ] `genetics`, `phenotypes`, `strains`
- [ ] `batches`, `batch_events`, `batch_relationships`
- [ ] `batch_code_settings` (user-configurable batch code generation)
- [ ] `mother_plants`, `mother_health_logs`
- [ ] `mother_health_reminder_settings` (user-configurable health reminders)
- [ ] RLS policies

### Domain & Application Layer

- [ ] Genetics, Phenotype, Strain entities
- [ ] Batch, BatchEvent entities with batch code generation
- [ ] BatchCodeSettings entity for user-configurable settings
- [ ] MotherPlant entity with health tracking and configurable limits
- [ ] MotherHealthReminderSettings entity for user-configurable reminders
- [ ] GeneticsManagementService
- [ ] BatchLifecycleService (with batch code generation and settings management)
- [ ] MotherHealthService (with reminder management)

### API Layer

- [ ] GeneticsController (strain CRUD)
- [ ] BatchesController (lifecycle management, batch code generation, settings)
- [ ] MotherPlantsController (health tracking, reminder settings)
- [ ] Validators for genetics, batches, and settings
- [ ] OpenAPI documentation

### Testing

- [ ] Unit tests (batch lifecycle state machine, batch code generation)
- [ ] Integration tests (batch splits/merges, configurable limits)
- [ ] E2E tests (strain → batch → tracking, health reminders)

### Acceptance Criteria

- [ ] Batch lineage tracked correctly
- [ ] Mother plant health logs retrievable
- [ ] Strain-specific blueprints associable
- [ ] Batch code auto-generation with user-defined prefix
- [ ] User-configurable mother plant propagation limits
- [ ] Event-driven health reminders with user-configurable frequency
- [ ] Unlimited lineage depth with performance monitoring
- [ ] Both partial and complete batch splits with validation

**Status**: 0/28 complete (0%)

---

## ✅ FRP-04: Tasks, Messaging & Slack (W5-W6)

**Owner:** Workflow & Messaging Squad  
**Status:** 🚧 Not Started

### Database Migrations

- [ ] `tasks`, `task_dependencies`, `task_watchers`
- [ ] `conversations`, `messages`, `message_attachments`
- [ ] `slack_message_bridge_log` (idempotent mapping)
- [ ] RLS policies

### Domain & Application Layer

- [ ] Task, TaskDependency, TaskWatcher entities
- [ ] Conversation, Message entities
- [ ] TaskLifecycleService (state machine)
- [ ] TaskGatingResolver (SOP/training checks)
- [ ] SlackNotificationService (notify-only)
- [ ] SlackOutboxWorker (retry failed)

### API Layer

- [ ] TasksController (lifecycle endpoints)
- [ ] MessagesController (conversations)
- [ ] Slack webhook receivers
- [ ] Validators
- [ ] OpenAPI documentation

### Infrastructure

- [ ] SlackApiClient with retry/idempotency
- [ ] Outbox pattern for Slack messages
- [ ] Background worker for queue processing

### Testing

- [ ] Unit tests (task lifecycle state machine)
- [ ] Contract tests (Slack API mock)
- [ ] E2E tests (blocked task → Slack notify)
- [ ] Idempotency tests

### Acceptance Criteria

- [ ] Task events notify Slack p95 < 2s
- [ ] Blocked reasons explicit
- [ ] Task gating works E2E
- [ ] Slack message delivery ≥99.9%

**Status**: 0/24 complete (0%)

---

## ✅ FRP-05: Telemetry Ingest & Rollups (W5-W6) ⚠️ **CRITICAL**

**Owner:** Telemetry & Controls Squad  
**Status:** 🚧 Not Started

### Database Migrations

- [ ] `sensor_streams`, `sensor_readings` (hypertable)
- [ ] Continuous aggregates: `sensor_readings_1m`, `sensor_readings_5m`, `sensor_readings_1h`
- [ ] Compression policies (7d)
- [ ] Retention policies (90d raw, 730d rollups)
- [ ] `alert_rules`, `alert_instances`
- [ ] BRIN indexes on (site_id, ts)

### Domain & Application Layer

- [ ] SensorStream, SensorReading entities
- [ ] AlertRule, AlertInstance entities
- [ ] TelemetryIngestService (MQTT/HTTP/SDI-12 adapters)
- [ ] NormalizationService (unit coercion)
- [ ] IdempotencyService (dedupe)
- [ ] AlertEvaluationService (rule engine)
- [ ] RollupFreshnessMonitor

### API Layer

- [ ] TelemetryController (ingest endpoint)
- [ ] AlertsController (rule management)
- [ ] WebSocket /realtime/subscribe endpoint
- [ ] Validators
- [ ] OpenAPI documentation

### Infrastructure

- [ ] MqttAdapter (device communication)
- [ ] HttpAdapter (legacy devices)
- [ ] Sdi12Adapter (substrate sensors)
- [ ] WalFanoutService (realtime push)
- [ ] OpenTelemetry instrumentation

### Testing

- [ ] Unit tests (normalization logic)
- [ ] Integration tests (ingest → rollup → query)
- [ ] Load tests (k6: 10k msg/s, p95 < 1.0s)
- [ ] Contract tests (WebSocket scenarios)
- [ ] 15-minute sustained load test

### Acceptance Criteria

- [ ] Ingest p95 < 1.0s
- [ ] Rollup freshness < 60s
- [ ] Realtime push p95 < 1.5s
- [ ] Deviation alerts fire correctly
- [ ] Burn-rate alerts verified in staging

**Exit Gate Required:** Load tests green before FRP-06 can start

**Status**: 0/30 complete (0%)

---

## ✅ FRP-06: Irrigation Orchestrator + HIL (W7-W8) ⚠️ **CRITICAL**

**Owner:** Telemetry & Controls/Irrigation Squad  
**Status:** 🚧 Not Started

### W0 Prerequisite: Golden Harness Build

- [ ] Physical test rig assembled
- [ ] Relay boards for E-STOP/door simulation
- [ ] PoE↔AC failover injection
- [ ] MQTT broker kill switch
- [ ] VLAN flap simulator
- [ ] Logging infrastructure (FRAM + timeseries DB)
- [ ] 12 scripted chaos scenarios executed
- [ ] Runbook published

### Database Migrations

- [ ] `mix_tanks`, `injector_channels`, `nutrient_products`
- [ ] `irrigation_groups`, `irrigation_programs`, `irrigation_schedules`
- [ ] `irrigation_runs`, `irrigation_step_runs`
- [ ] `interlock_events`, `device_commands`
- [ ] RLS policies

### Domain & Application Layer

- [ ] IrrigationGroup, IrrigationProgram entities
- [ ] IrrigationRun, IrrigationStepRun entities
- [ ] MixTank, InjectorChannel entities
- [ ] IrrigationOrchestratorService (command queue)
- [ ] ScheduleTriggerService (time/phase)
- [ ] ManualApprovalService (optional gating)
- [ ] IrrigationAbortSaga (safe abort compensator)
- [ ] InterlockEvaluationService (safety checks)
- [ ] 7 interlock specifications (E-STOP, door, tank level, EC/pH, CO₂, max runtime, concurrency)

### API Layer

- [ ] IrrigationController (programs, schedules, runs)
- [ ] Abort endpoint
- [ ] Manual approval endpoints
- [ ] Validators
- [ ] OpenAPI documentation

### Infrastructure

- [ ] MqttCommandAdapter (HydroCore/RoomHub)
- [ ] Device command queue with retry
- [ ] Saga compensation logic

### HIL Chaos Drill Matrix (A1 Requirement)

- [ ] E-STOP test (valves OFF, FAULT latched)
- [ ] Door open test (valves OFF, FAULT latched)
- [ ] PoE→AC failover test (run continues)
- [ ] AC→PoE failover test (run continues)
- [ ] Broker loss test (local buffer)
- [ ] VLAN flap test (restore with no data loss)
- [ ] Tank low test (abort, alert raised)
- [ ] Concurrency cap test (reject excess opens)

### Testing

- [ ] Unit tests (interlock specifications, 20+ scenarios)
- [ ] Integration tests (orchestrator saga)
- [ ] HIL tests (full chaos matrix on golden harness)
- [ ] E2E tests (schedule → run → complete)

### Acceptance Criteria

- [ ] Program executes with step monitoring
- [ ] Safe aborts close valves
- [ ] HIL report green (zero unsafe actuations)
- [ ] Audit trail complete
- [ ] Enqueue→ack p95 < 800ms
- [ ] **Firmware Sign-Off Gate:** HIL report signed by Hardware/Firmware Lead

**Exit Gate Required:** Firmware sign-off before irrigation can go-live

**Status**: 0/46 complete (0%)

---

## ✅ FRP-07: Inventory, Scanning & GS1 Labels (W7-W8)

**Owner:** Core Platform/Inventory Squad  
**Status:** 🚧 Not Started

### Database Migrations

- [ ] `inventory_lots`, `inventory_balances`, `inventory_movements`
- [ ] `inventory_adjustments`, `lot_relationships` (splits/merges)
- [ ] `uom_definitions`, `uom_conversions`
- [ ] `barcode_settings`, `label_templates`
- [ ] RLS policies

### Domain & Application Layer

- [ ] InventoryLot, InventoryBalance entities
- [ ] InventoryMovement, InventoryAdjustment entities
- [ ] LotMovementService (movements/splits/merges)
- [ ] BalanceReconciliationService
- [ ] ScanningService (barcode → movement)
- [ ] UomConversionService (kg↔lb, L↔gal)
- [ ] FefoAllocationService (First-Expired-First-Out)
- [ ] Gs1LabelService (GS1 AI codes)

### API Layer

- [ ] InventoryController (movements, adjustments)
- [ ] LotsController (splits, merges, CRUD)
- [ ] ScanningController (barcode operations)
- [ ] LabelsController (GS1 generation)
- [ ] Validators
- [ ] OpenAPI documentation

### Testing

- [ ] Unit tests (UoM conversions, property-based tests)
- [ ] Integration tests (balance reconciliation after splits)
- [ ] E2E tests (scan → movement → balance update)
- [ ] GS1 label rendering tests

### Acceptance Criteria

- [ ] Balances reconcile after splits
- [ ] FEFO allocation works
- [ ] Scans update movements
- [ ] GS1 labels render correctly

**Status**: 0/30 complete (0%)

---

## ✅ FRP-08: Processing & Manufacturing (W9-W10)

**Owner:** Core Platform/Processing Squad  
**Status:** 🚧 Not Started

### Database Migrations

- [ ] `process_definitions`, `process_runs`, `process_steps`
- [ ] `labor_logs`, `waste_events`
- [ ] `process_cost_snapshots` (materials + labor + overhead)
- [ ] RLS policies

### Domain & Application Layer

- [ ] ProcessDefinition, ProcessRun entities
- [ ] LaborLog, WasteEvent entities
- [ ] ProcessRunService (orchestrate runs)
- [ ] YieldCalculationService (input → output)
- [ ] CostRollupService (WIP → COGS)
- [ ] InventoryTransformationHandler (consume inputs, create FG)

### API Layer

- [ ] ProcessingController (runs, completion)
- [ ] LaborController (labor logs)
- [ ] WasteController (waste events)
- [ ] Validators
- [ ] OpenAPI documentation

### Testing

- [ ] Unit tests (yield calculations)
- [ ] Integration tests (input reconciliation)
- [ ] E2E tests (process run → outputs created → costs attached)

### Acceptance Criteria

- [ ] Yields reconcile with inventory
- [ ] Labor and waste appear in cost rollups
- [ ] Rework lineage retained

**Status**: 0/21 complete (0%)

---

## ✅ FRP-09: Compliance (METRC) & COA (W9-W10) ⚠️ **CRITICAL**

**Owner:** Integrations/Compliance Squad  
**Status:** 🚧 Not Started

### Database Migrations

- [ ] `compliance_integrations`, `sync_queue`, `sync_events`
- [ ] `labs`, `lab_orders`, `lab_results`
- [ ] `holds`, `destruction_events`
- [ ] RLS policies

### Domain & Application Layer

- [ ] ComplianceIntegration, SyncEvent entities
- [ ] Lab, LabOrder, LabResult entities
- [ ] Hold, DestructionEvent entities
- [ ] MetrcSyncService (sync orchestrator)
- [ ] CoaIngestionService (PDF/CSV parser)
- [ ] HoldGatingService (failed COA → HOLD)
- [ ] DestructionService (two-person signoff)

### API Layer

- [ ] ComplianceController (METRC sync)
- [ ] CoaController (upload, results)
- [ ] DestructionController (two-person approval)
- [ ] Validators
- [ ] OpenAPI documentation

### Infrastructure

- [ ] MetrcApiClient (Colorado METRC, [[memory:9462269]])
- [ ] ComplianceSyncWorker (queue processor)
- [ ] Retry/backoff with idempotency
- [ ] DLQ management

### Testing

- [ ] Contract tests (METRC API mock, retry/backoff, idempotency)
- [ ] Integration tests (COA fail → HOLD created)
- [ ] E2E tests (destruction with two-person signoff)

### Acceptance Criteria

- [ ] Retry/backoff verified
- [ ] Failed COA → HOLD enforced
- [ ] Destruction logs exportable
- [ ] DLQ < 0.5% over 7 days

**Status**: 0/27 complete (0%)

---

## ✅ FRP-10: QuickBooks Online (Item-Level) (W11) ⚠️ **CRITICAL**

**Owner:** Integrations/QuickBooks Squad  
**Status:** 🚧 Not Started

### Database Migrations

- [ ] `accounting_integrations`, `qbo_item_map`, `qbo_vendor_map`, `qbo_customer_map`
- [ ] `accounting_queue`, `accounting_events`
- [ ] `qbo_reconciliation_reports`
- [ ] RLS policies

### Domain & Application Layer

- [ ] AccountingIntegration, AccountingEvent entities
- [ ] QboItemMap, QboVendorMap entities
- [ ] QboSyncService (item-level sync)
- [ ] QboReconciliationService (variance calc, A2 requirement)
- [ ] DlqReplayService (retry failed events)
- [ ] ReceivingToBillHandler (PO receipt → Bill)

### API Layer

- [ ] AccountingController (QBO bills, reconciliation)
- [ ] QboController (OAuth2, sync)
- [ ] ReconciliationController (variance reports)
- [ ] Validators
- [ ] OpenAPI documentation

### Infrastructure

- [ ] QboApiClient (OAuth2 + Request-ID)
- [ ] AdaptiveThrottlingService (rate limit backoff)
- [ ] OAuth2 token refresh
- [ ] DLQ management

### Recon SLO (A2 Requirement)

- [ ] Daily variance calculation implemented
- [ ] Variance ≤ 0.5% SLO enforced
- [ ] Alert if variance > 0.5%
- [ ] Minimal Recon Dashboard (Grafana panel)
  - [ ] Daily Variance % (stat panel, red if > 0.5%)
  - [ ] DLQ Count (stat panel, link to /admin/dlq)
  - [ ] Alert annotation (variance breach)

### Testing

- [ ] Contract tests (QBO API mock, Request-ID idempotency)
- [ ] Integration tests (receiving → Bill creation)
- [ ] E2E tests (7-day SLO validation, variance ≤ 0.5%)
- [ ] OAuth2 flow tests

### Acceptance Criteria

- [ ] Receiving creates correct QBO Bill
- [ ] DLQ < 0.1% over 7 days
- [ ] Variance ≤ 0.5% daily
- [ ] Dashboard green
- [ ] OAuth2 refresh on expiry

**Status**: 0/32 complete (0%)

---

## ✅ FRP-15: Notifications & Escalations (W11)

**Owner:** Integrations/Notifications Squad  
**Status:** 🚧 Not Started

### Database Migrations

- [ ] `notification_rules`, `notification_instances`
- [ ] `escalation_chains`, `quiet_hours`, `notification_preferences`
- [ ] RLS policies

### Domain & Application Layer

- [ ] NotificationRule, NotificationInstance entities
- [ ] EscalationChain, QuietHours entities
- [ ] NotificationRoutingService (policy engine)
- [ ] EscalationService (chain execution)
- [ ] DedupBundlingService (storm control)

### API Layer

- [ ] NotificationsController (rules, send)
- [ ] EscalationsController (chains, preferences)
- [ ] Validators
- [ ] OpenAPI documentation

### Infrastructure

- [ ] TwilioSmsAdapter (SMS critical)
- [ ] PagerDutyAdapter (on-call)
- [ ] Email adapter
- [ ] Slack integration (reuse from FRP-04)

### Testing

- [ ] Unit tests (policy evaluation, 20+ scenarios)
- [ ] Unit tests (dedup/bundling, storm control)
- [ ] Integration tests (escalation chain timeout → escalate)
- [ ] E2E tests (end-to-end escalation flow)

### Acceptance Criteria

- [ ] Monitor mode passes
- [ ] Quiet hours verified
- [ ] Escalation chains work
- [ ] Policy unit tests green

**Status**: 0/22 complete (0%)

---

## ✅ W0 Foundation Setup (Pre-FRP Work)

**Owner:** SRE + Hardware/Firmware + Core Platform  
**Status:** 🚧 Partially Complete

### Golden Harness Build (A1 Requirement)

- [ ] Physical test rig assembled (80 engineer-hours estimated)
- [ ] Relay boards for E-STOP/door simulation
- [ ] PoE↔AC failover injection
- [ ] MQTT broker kill switch
- [ ] VLAN flap simulator
- [ ] Logging infrastructure
- [ ] 12 scripted chaos scenarios executed
- [ ] Firmware sign-off checklist drafted
- [ ] Runbook published

### Seed Data Script (R3)

- [ ] `scripts/seed/seed-pilot-site.sql` created
- [ ] 1 Organization: Denver Grow Co. (Colorado, METRC)
- [ ] 1 Site: Denver Main Facility
- [ ] 2 Rooms: Veg (4 zones), Flower (6 zones)
- [ ] 10 Users: 4 Operators (badges), 3 Managers, 2 Compliance, 1 Admin
- [ ] 3 Strains: Blue Dream, OG Kush, Gorilla Glue
- [ ] 2 Batches: BD-V-001 (Veg), OG-F-002 (Flower)
- [ ] Equipment: 10 sensors, 5 valves, 2 pumps
- [ ] 20 Inventory Lots: 10 inputs, 5 WIP, 5 FG
- [ ] CI integration (run before every test suite)
- [ ] Idempotent (DROP CASCADE + INSERT)

### Track A Gap Closure

- [x] Alert routing (Slack/PagerDuty webhooks) - **COMPLETE**
- [ ] OpenTelemetry instrumentation (services with OTel SDK)
- [ ] Unit test coverage (80%+ across all services)

**Status**: 3/22 complete (13.6%)

---

## ✅ W12: Pilot Readiness & UAT

**Owner:** TPM/Delivery + All Squads  
**Status:** 🚧 Not Started

### DR Drill (RPO ≤5m, RTO ≤30m)

- [ ] Simulate primary region failure
- [ ] Failover to warm replica
- [ ] Verify data integrity (last 5m preserved)
- [ ] Validate all services healthy
- [ ] Measure RTO (target ≤30m)
- [ ] DR drill report with sign-off

### UAT Execution

- [ ] Test Scenario 1: Badge login, task gating (missing SOP)
- [ ] Test Scenario 2: Create irrigation program, run with manual approval, safe abort
- [ ] Test Scenario 3: Scan lot, move location, split lot
- [ ] Test Scenario 4: Run process, capture labor, verify yield
- [ ] Test Scenario 5: Upload COA (fail → HOLD), destruction with two-person
- [ ] Test Scenario 6: Receive PO → Bill creation, verify variance < 0.5%
- [ ] UAT sign-off from pilot site operators

### Go/No-Go Checklist

- [ ] DR drill passed (RPO ≤5m, RTO ≤30m)
- [ ] SLOs met for 7 consecutive days
- [ ] UAT signed by pilot site
- [ ] Firmware HIL sign-off
- [ ] On-call rotation staffed
- [ ] Runbooks published
- [ ] Feature flags configured
- [ ] Seed data loaded

**Status**: 0/15 complete (0%)

---

## ✅ Pilot Cutover

**Owner:** TPM/Delivery Lead  
**Status:** 🚧 Not Started

### Preparation

- [ ] Feature flags configured
  - [ ] `frp_01_identity_enabled = true`
  - [ ] `frp_06_irrigation_enabled = true` (after firmware sign-off)
  - [ ] `frp_10_qbo_sync_enabled = true` (sandbox validated)
  - [ ] `closed_loop_ecph_enabled = false` (Phase 2)
  - [ ] `slack_mirror_mode = false` (notify-only)
- [ ] Seed data loaded for Denver Grow Co.
- [ ] Monitoring dashboards configured
- [ ] On-call schedule published
- [ ] Incident playbooks activated

### Cutover Execution

- [ ] Blue/green deployment executed
- [ ] Health checks passed
- [ ] Smoke tests passed
- [ ] Pilot site operations validated

### Post-Cutover

- [ ] 48h intensive on-call monitoring
- [ ] Daily SLO verification
- [ ] Incident response (if any) documented
- [ ] Pilot feedback collected
- [ ] Lessons learned documented

**Status**: 0/15 complete (0%)

---

## 📊 Overall Completion Status

| Category | Completed | Total | % |
|----------|-----------|-------|---|
| **FRP-01: Identity** | 32 | 32 | **100%** ✅ |
| **FRP-02: Spatial** | 28 | 28 | **100%** ✅ |
| **FRP-03: Genetics** | 0 | 28 | 0% |
| **FRP-04: Tasks/Slack** | 0 | 24 | 0% |
| **FRP-05: Telemetry** | 0 | 30 | 0% |
| **FRP-06: Irrigation** | 0 | 46 | 0% |
| **FRP-07: Inventory** | 0 | 30 | 0% |
| **FRP-08: Processing** | 0 | 21 | 0% |
| **FRP-09: Compliance** | 0 | 27 | 0% |
| **FRP-10: QBO** | 0 | 32 | 0% |
| **FRP-15: Notifications** | 0 | 22 | 0% |
| **W0 Foundation** | 3 | 22 | 13.6% |
| **W12 UAT & DR** | 0 | 15 | 0% |
| **Pilot Cutover** | 0 | 15 | 0% |
| **TOTAL** | **63** | **367** | **17.2%** |

---

## 🚧 Critical Path Tracking

### Critical Path Items (In Order)

1. ✅ **FRP-01 (Identity/RLS)** - COMPLETE (September 29, 2025)
2. ✅ **FRP-02 (Spatial/Equipment)** - COMPLETE (October 1, 2025)
3. 🚧 **W0 Foundation** - In Progress (13.6%)
   - Golden harness build (required for FRP-06 HIL)
   - Seed data script (required for testing)
   - Track A gaps (OpenTelemetry, unit test coverage)
4. ⏳ **FRP-05 (Telemetry)** - Load test gate blocks FRP-06
5. ⏳ **FRP-06 (Irrigation)** - Firmware sign-off blocks pilot go-live
6. ⏳ **FRP-09 (Compliance)** - DLQ < 0.5% required for go-live
7. ⏳ **FRP-10 (QBO)** - Variance ≤ 0.5% required for go-live
8. ⏳ **W12 UAT & DR Drill** - Required for go-live
9. ⏳ **Pilot Cutover** - Final deployment

### Critical Path Risks

| Risk | Impact | Mitigation | Status |
|------|--------|------------|--------|
| **FRP-01 delay** | Blocks all downstream FRPs | ✅ COMPLETE - Risk eliminated | ✅ |
| **FRP-02 delay** | Blocks FRP-05, FRP-06, FRP-07 | ✅ COMPLETE - Risk eliminated | ✅ |
| **Golden harness delays** | Blocks FRP-06 HIL, delays irrigation | Parallel sim-only fallback; W0-W5 priority | 🚧 |
| **FRP-05 telemetry load gate fails** | Blocks FRP-06 start, hardware idle | Early load testing, k6 scenarios ready | ⏳ |
| **FRP-06 HIL failure** | Irrigation cannot enable, pilot blocked | Comprehensive interlock specs, firmware sign-off | ⏳ |
| **FRP-10 QBO variance > 0.5%** | Financial integrity issue, go-live blocked | Daily recon monitoring, DLQ alerts | ⏳ |

---

## 🎯 Acceptance Criteria Summary

| FRP | Acceptance Criteria | Evidence |
|-----|---------------------|----------|
| **FRP-01** | Cross-site blocked; gated tasks show reason; audit verifies | ✅ RLS fuzz + E2E tests passing (70/70) |
| **FRP-02** | Equipment heartbeat visible; calibration retrievable; RLS enforced | ✅ Integration tests passing (28/28) |
| **FRP-03** | Batch lineage tracked; mother health logs retrievable | Integration tests |
| **FRP-04** | Task events notify Slack p95 < 2s; blocking works | E2E tests |
| **FRP-05** | Rollup freshness < 60s; realtime p95 < 1.5s | 7-day SLO validation |
| **FRP-06** | Program executes; safe aborts; HIL green; p95 < 800ms | HIL report + E2E |
| **FRP-07** | Balances reconcile; FEFO works; scans update | E2E inventory flow |
| **FRP-08** | Yields reconcile; labor/waste in costs | E2E processing |
| **FRP-09** | Retry/backoff verified; COA fail → HOLD; DLQ < 0.5% | Contract + E2E |
| **FRP-10** | Bills correct; DLQ < 0.1%; variance ≤ 0.5% | 7-day recon report |
| **FRP-15** | Quiet hours; escalation chains; policy tests green | Unit + E2E |

---

## 🎓 Key Learnings from FRP-01

### What Went Well ✅

1. **Clean Architecture** - Clear separation (Domain/Application/Infrastructure/API)
2. **Security-First** - RLS and ABAC from day 1
3. **Canonical JSON** - Eliminated audit hash false positives
4. **Comprehensive Testing** - 90%+ coverage, 53 canonical JSON tests
5. **Production Polish** - ValidationConstants, error handling, rate limiting
6. **Ahead of Schedule** - 38% faster than estimate

### Patterns to Reuse

1. **Repository Pattern** - All data access via interfaces
2. **Service Layer** - Business logic separated from domain
3. **FluentValidation** - Clean, testable validation
4. **Background Jobs** - BackgroundService pattern
5. **Canonical JSON** - Use for all audit/hashing scenarios
6. **NpgsqlDataSource** - Connection pooling with retry
7. **ValidationConstants** - Centralized validation patterns

---

## 📚 Artifacts & Documents

1. ✅ **FRP-01 Completion Certificate** → `docs/FRP01_COMPLETION_CERTIFICATE.md`
2. ✅ **Canonical JSON Guide** → `docs/architecture/canonical-json-hashing.md`
3. ✅ **Canonical JSON Migration** → `docs/migrations/canonical-json-migration-guide.md`
4. 🚧 **HIL Chaos Drill Playbook** → `docs/hardware/hil-chaos-playbook.md`
5. 🚧 **API Contracts (OpenAPI)** → `docs/api/contracts/track-b-*.yaml`
6. 🚧 **Seed Data Script** → `scripts/seed/seed-pilot-site.sql`
7. 🚧 **Testing Strategy Matrix** → `docs/testing/track-b-strategy.md`
8. 🚧 **QBO Recon Dashboard** → `src/infrastructure/grafana/dashboards/qbo-recon-mvp.json`
9. 🚧 **Runbooks** → `docs/ops/runbooks/frp-*.md`
10. 🚧 **UAT Checklist** → `docs/pilot/uat-checklist.md`

---

## 📞 Next Actions (Immediate)

1. ✅ **FRP-01 Complete** - DONE (September 29, 2025)
2. ✅ **FRP-02 Complete** - DONE (October 1, 2025) - All tests passing!
3. 🚧 **W0 Foundation Work:**
   - Start golden harness build (Hardware/Firmware squad)
   - Create seed data script (Core Platform squad)
   - Instrument services with OpenTelemetry (SRE squad)
   - Reach 80%+ unit test coverage (all squads)
4. 🚧 **Sprint Planning:**
   - **W3-W4:** FRP-03 (Genetics) 
   - **W5-W6:** FRP-04 (Tasks) + FRP-05 (Telemetry)
   - Confirm squad capacity and assignments
5. 📅 **Schedule Reviews:**
   - Weekly sprint reviews
   - Daily standups
   - Bi-weekly stakeholder demos

---

**Status Legend:**

- ✅ Complete
- 🚧 In Progress
- ⏳ Not Started
- ⚠️ Critical Path / High Risk
- ❌ Blocked

---

**Last Updated:** 2025-10-01  
**Track B Lead:** Engineering Squads  
**Review Frequency:** Weekly sprint reviews + daily standups  
**Overall Status:** 17.2% Complete (63/367 items)

---

## 🎉 Milestones

- ✅ **FRP-01 Complete:** September 29, 2025 (100% done, 38% ahead of schedule)
- ✅ **FRP-02 Complete:** October 1, 2025 (100% done, matched estimate with RLS refinements)
- 🎯 **Target Pilot Go-Live:** Week 12 (pending W0 foundation + 8 remaining FRPs)
