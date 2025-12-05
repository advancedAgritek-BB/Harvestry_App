# FRP-06 Execution Plan — Irrigation Orchestration & HIL Validation

**Version:** 1.0  
**Date:** October 7, 2025  
**Status:** 📋 Ready for Kickoff (Pending FRP-05 Completion)  
**Owner:** Telemetry & Controls Squad  
**Duration:** 5-6 days (24-28 development hours) + 80 hours hardware (parallel)

---

## Execution Strategy

### Vertical Slice Approach

FRP-06 will be delivered in **4 vertical slices** + **1 hardware track** (parallel):

1. **Slice 1: Core Orchestration** (Groups, Programs, Runs)
2. **Slice 2: Safety Interlocks** (Evaluation service, telemetry integration)
3. **Slice 3: Device Commands** (MQTT adapter, command queue, ack tracking)
4. **Slice 4: Abort Saga** (Safe abort compensator, fault handling)
5. **Hardware Track: Golden Harness** (W0-W5, parallel with slices)

Each slice delivers working, tested code incrementally.

---

## Pre-Slice Setup (Day 0: 2-3 hours)

### Database Migrations

**File:** `src/database/migrations/frp06/20251007_01_CreateIrrigationTables.sql`

```sql
-- irrigation_groups, irrigation_group_zones
-- irrigation_programs, irrigation_steps
-- irrigation_schedules
-- irrigation_runs, irrigation_step_runs
-- interlock_events
-- device_commands
-- mix_tanks, injector_channels

-- RLS policies for all tables
-- Indexes for performance
```

**Owner:** Telemetry & Controls  
**Estimated Time:** 90 minutes  
**Deliverable:** Migration script ready, reviewed

### Domain Foundation

**Files:**
- `src/Harvestry.Irrigation/Domain/Enums/` (6 files)
- `src/Harvestry.Irrigation/Domain/ValueObjects/` (6 files)
- `src/Harvestry.Irrigation/Domain/Planning/ExecutionPlan.cs`

**Owner:** Telemetry & Controls  
**Estimated Time:** 30 minutes  
**Deliverable:** Enums and value objects defined

### DI Configuration

**File:** `src/Harvestry.API/Program.cs`

```csharp
// Add IrrigationDbContext
builder.Services.AddDbContext<IrrigationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Irrigation")));

// Register repositories
builder.Services.AddScoped<IIrrigationGroupRepository, IrrigationGroupRepository>();
builder.Services.AddScoped<IIrrigationProgramRepository, IrrigationProgramRepository>();
builder.Services.AddScoped<IIrrigationRunRepository, IrrigationRunRepository>();
builder.Services.AddScoped<IDeviceCommandRepository, DeviceCommandRepository>();
builder.Services.AddScoped<IInterlockEventRepository, InterlockEventRepository>();

// Register services
builder.Services.AddScoped<IIrrigationOrchestratorService, IrrigationOrchestratorService>();
builder.Services.AddScoped<IInterlockEvaluationService, InterlockEvaluationService>();
builder.Services.AddScoped<IScheduleTriggerService, ScheduleTriggerService>();
builder.Services.AddScoped<IRunExecutionService, RunExecutionService>();
builder.Services.AddScoped<IIrrigationAbortSaga, IrrigationAbortSaga>();

// Register device adapters
builder.Services.AddSingleton<IMqttCommandAdapter, MqttCommandAdapter>();
builder.Services.AddScoped<IDeviceCommandQueue, DeviceCommandQueue>();
builder.Services.AddSingleton<IRunExecutionQueue, RunExecutionQueue>();

// Register background workers
builder.Services.AddHostedService<ScheduleTriggerWorker>();
builder.Services.AddHostedService<RunExecutionWorker>();
builder.Services.AddHostedService<DeviceCommandWorker>();
builder.Services.AddHostedService<InterlockMonitorWorker>();
```

**Owner:** Telemetry & Controls  
**Estimated Time:** 20 minutes  
**Deliverable:** DI wiring complete

---

## Slice 1: Core Orchestration (Day 1-2: 8-10 hours)

### Objectives

- ✅ Create irrigation groups with zones
- ✅ Create irrigation programs with steps
- ✅ Start/stop irrigation runs manually
- ✅ Track run status and audit trail

### Database

**Tables:**
- `irrigation_groups`
- `irrigation_group_zones`
- `irrigation_programs`
- `irrigation_steps`
- `irrigation_runs`
- `irrigation_step_runs`

**Estimated Time:** 1 hour (already in pre-slice setup)

### Domain Layer (3-4 hours)

**Files to Create:**

1. **IrrigationGroup.cs** (Aggregate Root)
   ```csharp
   public class IrrigationGroup : Entity<Guid>
   {
       public Guid SiteId { get; private set; }
       public string Code { get; private set; }
       public string Name { get; private set; }
       public int MaxConcurrentValves { get; private set; }
       
       private readonly List<IrrigationGroupZone> _zones = new();
       public IReadOnlyCollection<IrrigationGroupZone> Zones => _zones.AsReadOnly();
       
       // Factory method, AddZone, RemoveZone
   }
   ```

2. **IrrigationProgram.cs** (Aggregate Root)
   ```csharp
   public class IrrigationProgram : Entity<Guid>
   {
       public Guid SiteId { get; private set; }
       public Guid GroupId { get; private set; }
       public string Code { get; private set; }
       public int MaxRuntimeSeconds { get; private set; }
       
       private readonly List<IrrigationStep> _steps = new();
       public IReadOnlyCollection<IrrigationStep> Steps => _steps.AsReadOnly();
       
       // Factory method, AddStep, RemoveStep
   }
   ```

3. **IrrigationRun.cs** (Aggregate Root)
   ```csharp
   public class IrrigationRun : Entity<Guid>
   {
       public RunStatus Status { get; private set; }
       
       // Factory method, Start, Complete, Abort, Fault
   }
   ```

**Estimated Time:** 3-4 hours

### Application Services (2-3 hours)

**Files to Create:**

1. **IrrigationOrchestratorService.cs**
   - `StartRunAsync` - Create run, queue commands, start execution
   - `AbortRunAsync` - Abort run, execute abort saga
   - `GetRunStatusAsync` - Query run status

**Estimated Time:** 2-3 hours

### Infrastructure Repositories (1-2 hours)

**Files to Create:**

1. **IrrigationGroupRepository.cs**
2. **IrrigationProgramRepository.cs**
3. **IrrigationRunRepository.cs**

**Estimated Time:** 1-2 hours

### API Controllers (1-2 hours)

**Files to Create:**

1. **IrrigationGroupsController.cs**
   - `POST /api/v1/irrigation/groups` - Create group
   - `GET /api/v1/irrigation/groups` - List groups
   - `GET /api/v1/irrigation/groups/{id}` - Get group
   - `PUT /api/v1/irrigation/groups/{id}` - Update group
   - `DELETE /api/v1/irrigation/groups/{id}` - Delete group

2. **IrrigationProgramsController.cs**
   - `POST /api/v1/irrigation/programs` - Create program
   - `GET /api/v1/irrigation/programs` - List programs
   - `GET /api/v1/irrigation/programs/{id}` - Get program

3. **IrrigationRunsController.cs**
   - `POST /api/v1/irrigation/runs` - Start run
   - `GET /api/v1/irrigation/runs/{id}` - Get run status
   - `POST /api/v1/irrigation/runs/{id}/abort` - Abort run

**Estimated Time:** 1-2 hours

### Validators (30 minutes)

**Files to Create:**

1. **CreateIrrigationGroupValidator.cs**
2. **CreateIrrigationProgramValidator.cs**
3. **StartIrrigationRunValidator.cs**

**Estimated Time:** 30 minutes

### Testing (2-3 hours)

**Unit Tests:**
- `IrrigationGroupTests.cs` - Group creation, zone management
- `IrrigationProgramTests.cs` - Program creation, step management
- `IrrigationRunTests.cs` - Run state machine

**Integration Tests:**
- `IrrigationGroupRepositoryTests.cs` - CRUD operations
- `IrrigationProgramRepositoryTests.cs` - CRUD with steps
- `IrrigationRunRepositoryTests.cs` - Run lifecycle

**Estimated Time:** 2-3 hours

### Slice 1 Acceptance Criteria

- ✅ Can create irrigation group with zones
- ✅ Can create irrigation program with steps
- ✅ Can start irrigation run manually
- ✅ Run status tracked correctly
- ✅ Audit trail complete (all events logged)
- ✅ Unit tests passing (≥90% coverage)
- ✅ Integration tests passing
- ✅ RLS policies enforced

---

## Slice 2: Safety Interlocks (Day 2-3: 6-8 hours)

### Objectives

- ✅ Evaluate 7 safety interlocks before run start
- ✅ Monitor telemetry for interlock trips during run
- ✅ Abort run safely on interlock violation
- ✅ Log interlock events for audit trail

### Domain Layer (1 hour)

**Files to Create:**

1. **InterlockEvent.cs** (Entity)
   ```csharp
   public class InterlockEvent : Entity<Guid>
   {
       public Guid SiteId { get; private set; }
       public Guid? RunId { get; private set; }
       public InterlockType InterlockType { get; private set; }
       public string Reason { get; private set; }
       public bool FaultLatched { get; private set; }
       
       // Factory method, Rearm
   }
   ```

**Estimated Time:** 1 hour

### Application Services (3-4 hours)

**Files to Create:**

1. **InterlockEvaluationService.cs**
   - `EvaluateAsync` - Check all interlocks
   - `CheckEStopAsync` - E-STOP status
   - `CheckDoorAsync` - Door interlock
   - `CheckTankLevelAsync` - Tank level
   - `CheckEcPhBoundsAsync` - EC/pH bounds
   - `CheckCo2LockoutAsync` - CO₂ enrichment
   - `CheckConcurrencyCapAsync` - Max concurrent valves
   - `CheckMaxRuntimeAsync` - Max runtime

**Estimated Time:** 3-4 hours

### Infrastructure (1 hour)

**Files to Create:**

1. **InterlockEventRepository.cs**
2. **InterlockMonitorWorker.cs** (Background service)
   - Monitors telemetry streams
   - Detects interlock trips
   - Aborts runs on violation

**Estimated Time:** 1 hour

### Integration with Telemetry (30 minutes)

**Dependencies:**
- FRP-05 telemetry service
- Query latest sensor readings
- Check reading staleness (< 5 min)

**Estimated Time:** 30 minutes

### Testing (2-3 hours)

**Unit Tests:**
- `InterlockEvaluationServiceTests.cs` - 20+ scenarios
  - E-STOP open → deny
  - Door open → deny
  - Tank low → deny
  - EC out of bounds → deny
  - CO₂ lockout → deny
  - Concurrency exceeded → deny
  - All clear → allow

**Integration Tests:**
- `InterlockIntegrationTests.cs`
  - Run start → interlock deny → rejected
  - Run running → interlock trip → abort

**Estimated Time:** 2-3 hours

### Slice 2 Acceptance Criteria

- ✅ All 7 interlocks evaluated before run start
- ✅ Interlock violation rejects run start
- ✅ Running run aborts on interlock trip
- ✅ Interlock events logged with reason
- ✅ Telemetry staleness enforced per interlock threshold (≤30s safety sensors, ≤2 min tank)
- ✅ Unit tests covering 20+ interlock scenarios
- ✅ Integration tests passing

---

## Slice 3: Device Commands (Day 3-4: 4-6 hours)

### Objectives

- ✅ Queue device commands (OpenValve, CloseValve, StartPump, StopPump)
- ✅ Dispatch commands via MQTT to HydroCore/RoomHub
- ✅ Track command acknowledgments
- ✅ Retry failed commands with exponential backoff

### Domain Layer (1 hour)

**Files to Create:**

1. **DeviceCommand.cs** (Entity)
   ```csharp
   public class DeviceCommand : Entity<Guid>
   {
       public Guid SiteId { get; private set; }
       public Guid? RunId { get; private set; }
       public Guid? StepId { get; private set; }
       public int SequenceNumber { get; private set; }
       public CommandScope Scope { get; private set; }
       public Guid? TargetDeviceId { get; private set; }
       public Guid[] TargetZoneIds { get; private set; } = Array.Empty<Guid>();
       public CommandType CommandType { get; private set; }
       public CommandPriority Priority { get; private set; }
       public CommandStatus Status { get; private set; }
       public int RetryCount { get; private set; }
       public DateTime? SentAt { get; private set; }
       public DateTime? AckedAt { get; private set; }
       public string CommandPayload { get; private set; } = string.Empty;
       public string? ErrorMessage { get; private set; }
       
       // Factory method ensures scope→target consistency, state transitions
   }
   ```

**Estimated Time:** 1 hour

### Infrastructure (2-3 hours)

**Files to Create:**

1. **MqttCommandAdapter.cs**
   - Publishes commands to MQTT topics
   - Subscribes to acknowledgment topics
   - Handles reconnection

2. **DeviceCommandQueue.cs**
   - Enqueues commands with `priority`
   - Persists retry metadata + sequence numbers
   - Exposes dequeue API for workers

3. **RunExecutionQueue.cs**
   - Stores `RunExecutionRequest` payloads (run + plan)
   - Supports delayed requeue for retry/backoff

4. **DeviceCommandWorker.cs** (Background service)
   - Pulls next command by `priority`
   - Dispatches via MQTT adapter
   - Waits for ack (with timeout) and updates status

5. **RunExecutionWorker.cs** (Background service)
   - Pops execution requests
   - Emits open/pump commands, awaits ack before scheduling close
   - Monitors duration/volume thresholds and records `IrrigationStepRun`

**Estimated Time:** 2-3 hours

### Application Services (30 minutes)

**New:** `RunExecutionService.cs`
- Translate `IrrigationProgram` steps into sequenced `ExecutionPlanStep` records
- Validate scope/target combinations and assign `sequence_number`
- Collaborate with `RunExecutionQueue` to enqueue run execution

**Update:** `IrrigationOrchestratorService.cs`
- Build execution plan via `RunExecutionService`
- Enqueue plan for `RunExecutionWorker` processing once interlocks pass
- Persist run state transitions (Queued → Running → Completed/Faulted)

**Estimated Time:** 30 minutes

### Testing (1-2 hours)

**Unit Tests:**
- `DeviceCommandTests.cs` - Command state machine
- `DeviceCommandQueueTests.cs` - Queue operations
- `RunExecutionServiceTests.cs` - Plan building, sequencing, validation

**Integration Tests:**
- `DeviceCommandIntegrationTests.cs`
  - Enqueue → process → ack
  - Enqueue → process → timeout → retry
  - Enqueue → process → max retries → fail
- `RunExecutionWorkerTests.cs`
  - Plan executes commands sequentially, honoring ack
  - Emergency command preempts normal queue

**Contract Tests:**
- Mock MQTT broker
- Verify command payloads
- Verify ack handling
- Validate broadcast payload for `CloseAllValves` and zone fan-out

**Estimated Time:** 1-2 hours

### Slice 3 Acceptance Criteria

- ✅ Execution plans generated with ordered `sequence_number` values
- ✅ RunExecutionWorker gates next step on ack/step completion
- ✅ Broadcast `CloseAllValves` honored and prioritized (`priority = Emergency`)
- ✅ MQTT adapter publishes correct payloads including scope/targets
- ✅ Failed commands retried (max 3 attempts) with exponential backoff
- ✅ Command queue p95 < 800ms (enqueue→ack)
- ✅ Unit/Integration/Contract tests passing

---

## Slice 4: Abort Saga (Day 4-5: 2-3 hours)

### Objectives

- ✅ Safe abort on interlock trip
- ✅ Close all valves immediately
- ✅ Wait for device acknowledgment
- ✅ Fault run if ack timeout

### Application Sagas (1-2 hours)

**Files to Create:**

1. **IrrigationAbortSaga.cs**
   ```csharp
   public record AbortOutcome(bool Faulted, string? FailureReason)
   {
       public static AbortOutcome Success() => new(false, null);
       public static AbortOutcome Fault(string reason) => new(true, reason);
   }

   public class IrrigationAbortSaga : IIrrigationAbortSaga
   {
       public async Task<AbortOutcome> ExecuteAsync(IrrigationRun run, string reason, CancellationToken cancellationToken = default)
       {
           var closeCommand = DeviceCommand.CreateBroadcast(
               run.SiteId,
               run.Id,
               run.CurrentStepId,
               CommandType.CloseAllValves,
               CommandPriority.Emergency);

           await _deviceCommandQueue.EnqueueAsync(closeCommand, cancellationToken);
           
           var ackReceived = await _ackService.WaitForAckAsync(closeCommand.Id, TimeSpan.FromSeconds(5), cancellationToken);
           if (!ackReceived)
           {
               return AbortOutcome.Fault("Abort failed: valves did not acknowledge close command within 5s");
           }

           await _runExecutionService.CancelAsync(run.Id, cancellationToken);
           return AbortOutcome.Success();
       }
   }
   ```

**Estimated Time:** 1-2 hours

### Integration (30 minutes)

**Update:** `IrrigationOrchestratorService.cs`
- Invoke abort saga for manual aborts and interlock trips
- Persist `run.Fault` when `AbortOutcome.Faulted`
- Cancel outstanding execution plan items on abort

**Estimated Time:** 30 minutes

### Testing (1 hour)

**Integration Tests:**
- `IrrigationAbortSagaTests.cs`
  - Abort → close command sent → ack received → success
  - Abort → close command sent → timeout → `AbortOutcome.Faulted`
  - Abort → broadcast command → RunExecutionService cancel invoked

**E2E Tests:**
- `IrrigationE2ETests.cs`
  - Start run → abort → valves close < 5s
  - Start run → interlock trip → run faulted and valves close < 5s

**Estimated Time:** 1 hour

### Slice 4 Acceptance Criteria

- ✅ Abort command closes all valves
- ✅ Ack received within 5s
- ✅ Run faulted on ack timeout via `AbortOutcome.Faulted`
- ✅ Integration tests passing
- ✅ E2E tests passing

---

## Hardware Track: Golden Harness (W0-W5: 80 hours, parallel)

### Objectives

- ✅ Fabricate physical test rig
- ✅ Relay boards for E-STOP/door simulation
- ✅ Power injection (PoE/AC failover)
- ✅ MQTT broker kill switch
- ✅ Logging infrastructure (FRAM + InfluxDB)
- ✅ Execute 12 chaos drill tests
- ✅ Generate HIL report with firmware sign-off

### Week 0-2: Fabrication (40 hours)

**Tasks:**
- Assemble relay board array (8x SPST relays)
- Wire E-STOP/door simulation
- Install power injection board (PoE/AC)
- Install network chaos controller (MQTT kill switch)
- Wire load simulator (dummy valve loads)
- Install current measurement (clamp on 24VAC transformer)

**Owner:** Hardware/Firmware  
**Deliverable:** Golden harness assembled, tested

### Week 3-4: Firmware RC1 (20 hours)

**Tasks:**
- Flash firmware RC1 to devices
- Validate MFG-TEST mode
- Test local program execution (offline-first)
- Test interlock wiring

**Owner:** Firmware Lead  
**Deliverable:** Firmware RC1 ready for HIL drills

### Week 5-6: HIL Rehearsal (10 hours)

**Tasks:**
- Dry run (no faults)
- Validate logging infrastructure
- Verify video recording
- Tune test scripts

**Owner:** Telemetry & Controls  
**Deliverable:** HIL rehearsal passed

### Week 6: HIL Chaos Drill Execution (10 hours)

**Tasks:**
- Execute 12 chaos tests (see HIL playbook)
- Capture logs (device + cloud + video)
- Generate report
- Firmware sign-off

**Owner:** Hardware/Firmware + Controls  
**Deliverable:** HIL report with firmware sign-off

---

## Post-Slice: Testing & Polish (Day 5-6: 4-6 hours)

### Unit Test Coverage (2 hours)

**Target:** ≥90% code coverage

**Focus Areas:**
- Domain entities (100% coverage)
- Application services (≥90% coverage)
- Infrastructure repositories (≥85% coverage)

**Estimated Time:** 2 hours

### Integration Test Coverage (2 hours)

**Target:** 100% of critical paths

**Test Suites:**
- Irrigation group CRUD
- Irrigation program CRUD with steps
- Irrigation run lifecycle
- Interlock evaluation
- Device command queue
- Abort saga

**Estimated Time:** 2 hours

### RLS Fuzz Tests (1 hour)

**Scenarios:**
- User A reads User B's irrigation groups (different site) → blocked
- User A starts run for different site → blocked
- User A publishes device command for different site → blocked
- Service account bypasses RLS → allowed

**Estimated Time:** 1 hour

### Validators & Error Handling (30 minutes)

**Tasks:**
- FluentValidation for all request DTOs
- ProblemDetails responses
- Error handling middleware

**Estimated Time:** 30 minutes

### OpenAPI Documentation (30 minutes)

**Tasks:**
- Swagger annotations
- Example requests/responses
- Error response documentation

**Estimated Time:** 30 minutes

---

## Timeline Summary

| Day | Slice | Tasks | Hours |
|-----|-------|-------|-------|
| **Day 0** | Pre-Slice Setup | Migrations, DI, enums | 2-3 |
| **Day 1-2** | Slice 1: Core Orchestration | Groups, Programs, Runs | 8-10 |
| **Day 2-3** | Slice 2: Safety Interlocks | Evaluation service, monitoring | 6-8 |
| **Day 3-4** | Slice 3: Device Commands | MQTT adapter, command queue | 4-6 |
| **Day 4-5** | Slice 4: Abort Saga | Safe abort compensator | 2-3 |
| **Day 5-6** | Testing & Polish | Unit/integration tests, validators | 4-6 |
| **TOTAL** | | | **24-28 hours** |

### Parallel Hardware Track

| Week | Phase | Hours |
|------|-------|-------|
| **W0-W2** | Golden harness fabrication | 40 |
| **W3-W4** | Firmware RC1 release | 20 |
| **W5** | HIL rehearsal | 10 |
| **W6** | HIL chaos drill execution | 10 |
| **TOTAL** | | **80 hours** |

---

## Quality Gates

### Slice Completion Gates

**Each slice must meet:**
- ✅ All unit tests passing (≥90% coverage)
- ✅ All integration tests passing
- ✅ No linter warnings
- ✅ Code review approved
- ✅ RLS policies tested
- ✅ OpenAPI documentation updated

### FRP-06 Completion Gates

**Before go-live:**
- ✅ All 4 slices complete
- ✅ HIL report green (12/12 tests passed)
- ✅ Firmware sign-off received
- ✅ SLO targets met (enqueue→ack p95 < 800ms)
- ✅ Security review passed
- ✅ Runbook published

---

## Dependencies & Prerequisites

### Before Starting

1. ✅ **FRP-05 load testing complete** (blocker)
   - Telemetry ingest p95 < 1.0s
   - Real-time push p95 < 1.5s
   - Rollup freshness < 60s

2. ✅ **Golden harness build started** (W0-W2)
   - Can proceed with slices 1-3 in parallel
   - HIL drills depend on harness completion

3. ✅ **MQTT broker available**
   - Test broker configured
   - Device topics mapped

### During Execution

- Telemetry & Controls Squad available (5-6 days)
- Hardware/Firmware Squad available (W0-W6, parallel)
- Code reviews scheduled (daily)

---

## Risk Mitigation

| Risk | Mitigation |
|------|------------|
| **FRP-05 delays FRP-06 start** | Prioritize FRP-05 load testing; unblock FRP-06 ASAP |
| **Golden harness delays HIL drills** | Start fabrication immediately; parallel track with dev work |
| **HIL drills fail** | Re-test after firmware fix; iterate until all tests pass |
| **MQTT ack timeouts** | Implement retry logic; fallback to manual intervention |

---

## Definition of Done

### FRP-06 Complete When:

- ✅ All 4 slices delivered and tested
- ✅ Golden harness built and tested
- ✅ HIL chaos drills passed (12/12)
- ✅ Firmware sign-off received
- ✅ All acceptance criteria met
- ✅ Documentation complete (runbook, API docs)
- ✅ Security review passed
- ✅ UAT passed (pilot site operators)

---

## Next Actions

1. ✅ **Complete FRP-05 load testing** (blocker)
   - Run k6 scripts (10k msg/s sustained)
   - Validate SLO targets
   - Publish FRP-05 completion report

2. 🚧 **Start golden harness build** (W0-W2, immediate)
   - Order components
   - Assign Hardware/Firmware squad
   - Schedule daily check-ins

3. 🚧 **FRP-06 kickoff** (after FRP-05 complete)
   - Day 0: Pre-slice setup
   - Day 1-2: Slice 1 (Core Orchestration)
   - Daily standups + code reviews

---

**Last Updated:** October 7, 2025  
**Status:** 📋 Ready for Kickoff (Pending FRP-05 Completion)  
**Next Review:** After FRP-05 load testing complete
