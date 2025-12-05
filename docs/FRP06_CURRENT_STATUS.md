# FRP-06 Current Status — Irrigation Orchestration & HIL Validation

**Date:** October 7, 2025  
**Status:** 📋 **Planning Complete, Ready for Development** (Pending FRP-05)  
**Completion:** 0% (0/46 items)  
**Owner:** Telemetry & Controls Squad

---

## 📊 PROGRESS SUMMARY

| Phase | Items | Complete | In Progress | Not Started | % Complete |
|-------|-------|----------|-------------|-------------|------------|
| **Pre-Slice Setup** | 3 | 0 | 0 | 3 | 0% |
| **Slice 1: Core Orchestration** | 12 | 0 | 0 | 12 | 0% |
| **Slice 2: Safety Interlocks** | 8 | 0 | 0 | 8 | 0% |
| **Slice 3: Device Commands** | 7 | 0 | 0 | 7 | 0% |
| **Slice 4: Abort Saga** | 4 | 0 | 0 | 4 | 0% |
| **Testing & Polish** | 6 | 0 | 0 | 6 | 0% |
| **Hardware: Golden Harness** | 6 | 0 | 0 | 6 | 0% |
| **TOTAL** | **46** | **0** | **0** | **46** | **0%** |

---

## 🎯 OVERALL STATUS

### Current State

- 📋 **Planning Complete** - Implementation Plan and Execution Plan published
- ⏳ **Development Not Started** - Waiting on FRP-05 performance validation
- 🚧 **Golden Harness Not Started** - Needs immediate kickoff (W0-W5 target)
- ✅ **HIL Playbook Complete** - 12 chaos drill tests defined and approved

### Prerequisites

- ✅ FRP-01 Complete (Identity, RLS, ABAC)
- ✅ FRP-02 Complete (Spatial/Equipment)
- ✅ FRP-03 Complete (Genetics/Batches)
- ✅ FRP-04 Complete (Tasks/Messaging)
- ⚠️ FRP-05 In Progress (Telemetry - 93% complete, load testing outstanding)

### Blockers

1. **FRP-05 Performance Validation** - CRITICAL BLOCKER
   - Status: ⏳ Load testing not complete
   - Impact: Cannot start FRP-06 development
   - Mitigation: Prioritize FRP-05 k6 load tests (10k msg/s sustained, p95 < 1.0s)
   - Target: Complete by October 10, 2025

2. **Golden Harness Build** - URGENT
   - Status: ⏳ Not started
   - Impact: HIL drills cannot execute (W6-W7 target)
   - Mitigation: Start fabrication immediately, parallel track with dev work
   - Target: Complete by Week 5 (November 4, 2025)

### Next Actions

1. **FRP-05 Load Testing** - Complete performance validation (blocker)
2. **Golden Harness Kickoff** - Start fabrication immediately
3. **FRP-06 Development Kickoff** - After FRP-05 complete (target October 14, 2025)

---

## 📋 DETAILED STATUS BY PHASE

### Pre-Slice Setup (0% Complete - 0/3 items)

| Item | Status | Notes |
|------|--------|-------|
| Database migrations | ⏳ Not Started | 8 tables + RLS policies + indexes |
| Domain enums/value objects | ⏳ Not Started | 4 enums + 5 value objects |
| DI configuration | ⏳ Not Started | Register repositories, services, workers |

**Estimated Time Remaining:** 2-3 hours

---

### Slice 1: Core Orchestration (0% Complete - 0/12 items)

| Item | Status | Notes |
|------|--------|-------|
| IrrigationGroup aggregate | ⏳ Not Started | Domain entity with zone management |
| IrrigationProgram aggregate | ⏳ Not Started | Domain entity with step management |
| IrrigationRun aggregate | ⏳ Not Started | Domain entity with state machine |
| IrrigationOrchestratorService | ⏳ Not Started | Application service (start/abort runs) |
| IrrigationGroupRepository | ⏳ Not Started | Infrastructure repository |
| IrrigationProgramRepository | ⏳ Not Started | Infrastructure repository |
| IrrigationRunRepository | ⏳ Not Started | Infrastructure repository |
| IrrigationGroupsController | ⏳ Not Started | API controller |
| IrrigationProgramsController | ⏳ Not Started | API controller |
| IrrigationRunsController | ⏳ Not Started | API controller |
| Validators (3 files) | ⏳ Not Started | FluentValidation for requests |
| Unit + integration tests | ⏳ Not Started | ≥90% coverage target |

**Estimated Time Remaining:** 8-10 hours

---

### Slice 2: Safety Interlocks (0% Complete - 0/8 items)

| Item | Status | Notes |
|------|--------|-------|
| InterlockEvent entity | ⏳ Not Started | Domain entity |
| InterlockEvaluationService | ⏳ Not Started | Application service (7 interlock checks) |
| CheckEStopAsync | ⏳ Not Started | E-STOP interlock |
| CheckDoorAsync | ⏳ Not Started | Door interlock |
| CheckTankLevelAsync | ⏳ Not Started | Tank level interlock |
| CheckEcPhBoundsAsync | ⏳ Not Started | EC/pH interlock |
| CheckCo2LockoutAsync | ⏳ Not Started | CO₂ enrichment interlock |
| CheckConcurrencyCapAsync | ⏳ Not Started | Max concurrent valves interlock |
| InterlockEventRepository | ⏳ Not Started | Infrastructure repository |
| InterlockMonitorWorker | ⏳ Not Started | Background service |
| Unit tests (20+ scenarios) | ⏳ Not Started | Comprehensive interlock coverage |
| Integration tests | ⏳ Not Started | Telemetry integration |

**Estimated Time Remaining:** 6-8 hours

---

### Slice 3: Device Commands (0% Complete - 0/7 items)

| Item | Status | Notes |
|------|--------|-------|
| DeviceCommand entity | ⏳ Not Started | Domain entity with state machine |
| MqttCommandAdapter | ⏳ Not Started | Infrastructure adapter (HydroCore/RoomHub) |
| DeviceCommandQueue | ⏳ Not Started | Outbox pattern implementation |
| DeviceCommandWorker | ⏳ Not Started | Background service (queue processor) |
| DeviceCommandRepository | ⏳ Not Started | Infrastructure repository |
| Unit tests | ⏳ Not Started | Command state machine tests |
| Integration tests | ⏳ Not Started | Queue operations + retry logic |
| Contract tests | ⏳ Not Started | MQTT mock tests |

**Estimated Time Remaining:** 4-6 hours

---

### Slice 4: Abort Saga (0% Complete - 0/4 items)

| Item | Status | Notes |
|------|--------|-------|
| IrrigationAbortSaga | ⏳ Not Started | Application saga (safe abort compensator) |
| Integration with orchestrator | ⏳ Not Started | Call saga on abort/interlock trip |
| Integration tests | ⏳ Not Started | Abort scenarios |
| E2E tests | ⏳ Not Started | End-to-end abort flow |

**Estimated Time Remaining:** 2-3 hours

---

### Testing & Polish (0% Complete - 0/6 items)

| Item | Status | Notes |
|------|--------|-------|
| Unit test coverage | ⏳ Not Started | Target ≥90% |
| Integration test coverage | ⏳ Not Started | Target 100% of critical paths |
| RLS fuzz tests | ⏳ Not Started | Cross-site access blocked |
| Validators | ⏳ Not Started | FluentValidation for all requests |
| Error handling | ⏳ Not Started | ProblemDetails responses |
| OpenAPI documentation | ⏳ Not Started | Swagger annotations |

**Estimated Time Remaining:** 4-6 hours

---

### Hardware: Golden Harness (0% Complete - 0/6 items)

| Item | Status | Notes |
|------|--------|-------|
| Golden harness fabrication | ⏳ Not Started | W0-W2 target (40 hours) |
| Relay boards (E-STOP/door) | ⏳ Not Started | 8x SPST relays + status LEDs |
| Power injection (PoE/AC) | ⏳ Not Started | Failover logic + kill switches |
| Network chaos controller | ⏳ Not Started | MQTT kill switch + VLAN flap |
| Load simulator | ⏳ Not Started | Dummy valve loads + current measurement |
| Logging infrastructure | ⏳ Not Started | FRAM + InfluxDB + video |
| Firmware RC1 release | ⏳ Not Started | W3-W4 target (20 hours) |
| HIL rehearsal | ⏳ Not Started | W5 target (10 hours) |
| HIL chaos drill execution | ⏳ Not Started | W6 target (10 hours) |
| Firmware sign-off | ⏳ Not Started | Required before go-live |

**Estimated Time Remaining:** 80 hours (parallel track)

---

## 🎯 ACCEPTANCE CRITERIA STATUS

| Criteria | Status | Evidence |
|----------|--------|----------|
| Program executes with step monitoring | ⏳ Not Started | E2E test: 3-step program completes |
| Safe aborts close valves | ⏳ Not Started | Integration test: abort → valves close < 5s |
| HIL report green (zero unsafe actuations) | ⏳ Not Started | HIL chaos drill report + firmware sign-off |
| Audit trail complete | ⏳ Not Started | Database audit: runs, steps, interlocks, commands |
| Enqueue→ack p95 < 800ms | ⏳ Not Started | k6 load test: command queue latency |

---

## 📈 TIMELINE & ESTIMATES

### Original Estimates
- **Total Estimated Time:** 24-28 hours development + 80 hours hardware (parallel)
- **Planned Duration:** 5-6 days development + W0-W6 hardware
- **Target Completion:** Week 8 (end of Sprint 4)

### Actual Progress
- **Time Invested:** 0 hours (planning only)
- **Time Remaining:** 24-28 hours development + 80 hours hardware
- **On Track:** 🔴 Blocked by FRP-05 performance validation

### Revised Timeline

| Milestone | Target Date | Status |
|-----------|-------------|--------|
| FRP-05 load testing complete | October 10, 2025 | ⏳ Pending |
| FRP-06 kickoff (Day 0) | October 14, 2025 | ⏳ Pending |
| Slice 1 complete | October 16, 2025 | ⏳ Pending |
| Slice 2 complete | October 18, 2025 | ⏳ Pending |
| Slice 3 complete | October 21, 2025 | ⏳ Pending |
| Slice 4 complete | October 22, 2025 | ⏳ Pending |
| Testing & polish complete | October 23, 2025 | ⏳ Pending |
| Golden harness complete | November 4, 2025 | ⏳ Pending |
| HIL drills complete | November 11, 2025 | ⏳ Pending |
| Firmware sign-off | November 13, 2025 | ⏳ Pending |

---

## 🚧 BLOCKERS & RISKS

### Critical Blockers

1. **FRP-05 Performance Validation** - 🔴 CRITICAL
   - **Impact:** Cannot start FRP-06 development
   - **Mitigation:** Prioritize FRP-05 k6 load tests immediately
   - **Target:** Complete by October 10, 2025
   - **Status:** ⏳ Outstanding

2. **Golden Harness Build Not Started** - 🟠 URGENT
   - **Impact:** HIL drills delayed, firmware sign-off delayed, go-live delayed
   - **Mitigation:** Start fabrication immediately, assign Hardware/Firmware squad
   - **Target:** Complete by Week 5 (November 4, 2025)
   - **Status:** ⏳ Not Started

### Identified Risks

1. **HIL Drills Fail** - 🟠 HIGH
   - **Mitigation:** Comprehensive interlock specs, re-test after firmware fix
   - **Status:** ⏳ To be validated

2. **MQTT Ack Timeouts** - 🟡 MEDIUM
   - **Mitigation:** Retry logic with exponential backoff, manual intervention UI
   - **Status:** ⏳ To be implemented

3. **Telemetry Staleness** - 🟡 MEDIUM
   - **Mitigation:** Staleness check (< 5 min), fallback to safe deny
   - **Status:** ⏳ To be validated

4. **Device Concurrency Thermal** - 🟡 MEDIUM
   - **Mitigation:** Hardware current limiting, firmware cap enforcement
   - **Status:** ⏳ To be tested in HIL drills

---

## 📝 NOTES & DECISIONS

### Architectural Decisions

1. **Command Queue Pattern**
   - Decision: Use outbox pattern for device commands
   - Rationale: Guarantees at-least-once delivery, retry logic, idempotency
   - Impact: Requires DeviceCommandQueue implementation

2. **Interlock Evaluation**
   - Decision: Evaluate all interlocks before run start + monitor during run
   - Rationale: Safety-first approach, prevent unsafe actuations
   - Impact: Requires telemetry integration (FRP-05)

3. **Abort Saga**
   - Decision: Safe abort compensator with ack timeout
   - Rationale: Ensures valves close even on device failure
   - Impact: Requires emergency close command + fault handling

4. **HIL Chaos Drills**
   - Decision: 12 chaos tests with firmware sign-off gate
   - Rationale: Zero tolerance for unsafe actuations
   - Impact: Requires golden harness build (W0-W5)

### Technical Notes

- Reuse FRP-05 telemetry service for interlock evaluation
- Reuse FRP-02 equipment registry for device mapping
- Reuse FRP-04 MQTT infrastructure for device commands
- RLS context via `SET LOCAL app.current_site_id` (established pattern)
- FluentValidation for all request DTOs (established pattern)

---

## 🎯 NEXT STEPS

### Immediate Actions (Pre-Development)

1. **Complete FRP-05 Load Testing** (BLOCKER)
   - Run k6 scripts (10k msg/s sustained, p95 < 1.0s)
   - Validate real-time push latency (p95 < 1.5s)
   - Validate rollup freshness (< 60s)
   - Publish FRP-05 completion report
   - **Target:** October 10, 2025

2. **Start Golden Harness Build** (URGENT)
   - Order components (relay boards, power injection, etc.)
   - Assign Hardware/Firmware squad
   - Schedule daily check-ins
   - **Target:** Kickoff October 7, 2025

3. **FRP-06 Development Kickoff** (After FRP-05 Complete)
   - Day 0: Pre-slice setup (migrations, DI, enums)
   - Day 1-2: Slice 1 (Core Orchestration)
   - Daily standups + code reviews
   - **Target:** October 14, 2025

### Slice 1: Core Orchestration (Days 1-2)
1. Create database migrations (irrigation_groups, programs, runs)
2. Implement IrrigationGroup aggregate (zone management)
3. Implement IrrigationProgram aggregate (step management)
4. Implement IrrigationRun aggregate (state machine)
5. Implement IrrigationOrchestratorService (start/abort runs)
6. Create repositories (groups, programs, runs)
7. Create API controllers (groups, programs, runs)
8. Write unit + integration tests (≥90% coverage)
9. Run RLS fuzz tests (cross-site access blocked)

### Slice 2: Safety Interlocks (Days 2-3)
1. Implement InterlockEvent entity
2. Implement InterlockEvaluationService (7 interlock checks)
3. Integrate with FRP-05 telemetry service
4. Implement InterlockMonitorWorker (background service)
5. Write unit tests (20+ interlock scenarios)
6. Write integration tests (telemetry integration)

---

## 📊 METRICS TO TRACK

### Performance Metrics
- Command enqueue→ack latency (target: p95 < 800ms)
- Interlock evaluation latency (target: p95 < 200ms)
- Safe abort latency (target: < 5s)
- HIL test pass rate (target: 100%)

### Quality Metrics
- Unit test coverage (target: ≥90%)
- Integration test coverage (target: 100% of critical paths)
- RLS test coverage (target: 100% of repository methods)
- API documentation coverage (target: 100% of endpoints)

### Operational Metrics
- Interlock trip rate (monitor)
- Device command failure rate (target: < 1%)
- MQTT ack timeout rate (target: < 0.5%)
- Run abort rate (monitor)

---

## 🔗 RELATED DOCUMENTS

- [FRP-06 Implementation Plan](./FRP06_IMPLEMENTATION_PLAN.md) - Detailed technical design
- [FRP-06 Execution Plan](./FRP06_EXECUTION_PLAN.md) - Step-by-step development guide
- [HIL Chaos Drill Playbook](./hardware/HIL_CHAOS_DRILL_PLAYBOOK.md) - 12 chaos drill tests
- [FRP-05 Current Status](./FRP05_CURRENT_STATUS.md) - Telemetry dependency status
- [Track B Completion Checklist](./TRACK_B_COMPLETION_CHECKLIST.md) - Master progress tracker

---

**Last Updated:** October 7, 2025  
**Next Update:** After FRP-05 load testing complete  
**Status:** 📋 Planning Complete, Ready for Development (Pending FRP-05)

