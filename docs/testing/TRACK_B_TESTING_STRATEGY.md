# Track B Testing Strategy — Comprehensive Coverage Matrix

**Version:** 1.0  
**Date:** 2025-09-29  
**Scope:** All FRPs (FRP-01 through FRP-15)  
**Coverage Goal:** 80%+ unit test coverage; 100% critical path E2E coverage

---

## Testing Pyramid (Target Distribution)

```
           E2E Tests
          (10% of tests)
       Critical user journeys
      ─────────────────────────
        Integration Tests
       (20% of tests)
     API + DB + External services
    ────────────────────────────────
          Contract Tests
         (10% of tests)
      External API contracts
     ────────────────────────────────
           Unit Tests
         (60% of tests)
      Domain logic, services
```

---

## Testing Matrix by FRP

### FRP-01: Identity, Roles, RLS/ABAC

| Test Type | Coverage | Tools | Owner |
|-----------|----------|-------|-------|
| **Unit** | PolicyEvaluationService (20+ ABAC scenarios), Badge parser, SOP gating resolver | xUnit, FluentAssertions | Core Platform/Identity |
| **Integration** | RLS fuzz tests (cross-site access → 403), Auth flows (badge login → token), Audit chain verification | xUnit, EF Core in-memory | Core Platform/Identity |
| **Contract** | N/A (no external API) | - | - |
| **E2E** | Gated task flow (missing SOP → blocked with reason), Badge revoke ends sessions | Playwright | Core Platform |

**Test Files:**
```
tests/unit/Identity/
├── PolicyEvaluationServiceTests.cs        # 25+ test cases
├── BadgeAuthenticationServiceTests.cs     # 10+ test cases
└── TaskGatingServiceTests.cs              # 15+ test cases

tests/integration/Identity/
├── RlsFuzzTests.cs                        # 20+ cross-site scenarios
├── AuthFlowTests.cs                       # Badge login, revoke, sessions
└── AuditChainVerificationTests.cs         # Nightly job simulation

tests/e2e/
└── identity-gating.spec.ts                # Gated task E2E flow
```

**Acceptance Criteria:**
- ✅ RLS fuzz tests pass (403 on cross-site access)
- ✅ E2E test: Gated task blocked with explicit reason
- ✅ Audit chain verification passes

---

### FRP-02: Spatial Model & Equipment Registry

| Test Type | Coverage | Tools | Owner |
|-----------|----------|-------|-------|
| **Unit** | Spatial tree traversal, Valve→zone mapping logic | xUnit | Core Platform/Spatial |
| **Integration** | Equipment linkage (room → zones → equipment), Calibration tracking | xUnit, EF Core | Core Platform/Spatial |
| **E2E** | Create room → add zones → register equipment → track calibration | Playwright | Core Platform |

**Test Files:**
```
tests/unit/Spatial/
├── SpatialHierarchyServiceTests.cs        # Tree operations
└── ValveZoneMappingServiceTests.cs        # Mapping logic

tests/integration/Spatial/
├── SpatialHierarchyTests.cs               # Room → zones creation
└── EquipmentRegistryTests.cs              # Equipment CRUD + linkage

tests/e2e/
└── spatial-equipment.spec.ts              # End-to-end spatial setup
```

**Acceptance Criteria:**
- ✅ Equipment heartbeat visible in integration tests
- ✅ Calibration logs retrievable
- ✅ RLS blocks cross-site equipment access

---

### FRP-05: Telemetry Ingest & Rollups

| Test Type | Coverage | Tools | Owner |
|-----------|----------|-------|-------|
| **Unit** | Normalization service (unit coercion), Alert rule evaluation | xUnit, FsCheck (property-based) | Telemetry & Controls |
| **Integration** | Ingest → rollup → query (1m/5m/1h), Rollup freshness monitoring | xUnit, TimescaleDB | Telemetry & Controls |
| **Contract** | WebSocket deterministic scenarios (subscribe → push → unsubscribe) | Pact | Telemetry & Controls |
| **Load** | k6 telemetry ingest (10k msg/s, p95 < 1.0s), Realtime push (p95 < 1.5s) | k6 | DevOps |
| **E2E** | Device sends reading → appears in rollup → fires deviation alert | Playwright + MQTT sim | Telemetry & Controls |

**Test Files:**
```
tests/unit/Telemetry/
├── NormalizationServiceTests.cs           # Unit conversions
└── AlertEvaluationServiceTests.cs         # Rule evaluation

tests/integration/Telemetry/
├── IngestRollupTests.cs                   # Ingest → rollup flow
└── RollupFreshnessTests.cs                # Lag monitoring

tests/contract/
└── realtime-push-contract.test.cs         # WebSocket Pact scenarios

tests/load/
├── telemetry-ingest-load.js               # k6 ingest test (existing)
└── realtime-push-load.js                  # k6 realtime test (existing)

tests/e2e/
└── telemetry-alert.spec.ts                # Ingest → alert E2E
```

**Acceptance Criteria:**
- ✅ Load test: Ingest p95 < 1.0s under 10k msg/s
- ✅ Integration test: Rollup freshness < 60s
- ✅ E2E test: Deviation alert fires correctly

---

### FRP-04: Tasks, Messaging & Slack

| Test Type | Coverage | Tools | Owner |
|-----------|----------|-------|-------|
| **Unit** | Task lifecycle state machine, SOP gating resolver | xUnit | Workflow & Messaging |
| **Integration** | Slack bridge (task event → outbox → Slack API), Outbox reconciliation | xUnit | Integrations/Slack |
| **Contract** | Slack API mock (verify idempotent message posting) | Pact | Integrations/Slack |
| **E2E** | Create gated task → attempt start (blocked) → complete training → start succeeds → Slack notified | Playwright | Workflow & Messaging |

**Test Files:**
```
tests/unit/Tasks/
├── TaskLifecycleServiceTests.cs           # State machine
└── TaskGatingResolverTests.cs             # SOP checks

tests/integration/Slack/
├── SlackBridgeTests.cs                    # Outbox → Slack
└── SlackOutboxReconciliationTests.cs      # Retry logic

tests/contract/
└── slack-api-contract.test.cs             # Slack API Pact

tests/e2e/
└── task-slack-notify.spec.ts              # Gated task → Slack E2E
```

**Acceptance Criteria:**
- ✅ Integration test: Task events notify Slack p95 < 2s
- ✅ Contract test: Idempotency verified
- ✅ E2E test: Blocked task shows reason + Slack notify

---

### FRP-06: Irrigation Orchestrator (Open-Loop) + HIL

| Test Type | Coverage | Tools | Owner |
|-----------|----------|-------|-------|
| **Unit** | Interlock specifications (20+ scenarios), Saga compensators | xUnit | Telemetry & Controls |
| **Integration** | Orchestrator (program run → step monitoring → safe abort), Command outbox (idempotency) | xUnit | Telemetry & Controls |
| **Contract** | Device command adapter (MQTT → HydroCore acks) | Pact | Hardware/Firmware |
| **HIL/Chaos** | Full chaos matrix (E-STOP, door, PoE↔AC, broker loss, concurrency cap) — **See HIL Playbook** | Golden harness + Python scripts | Hardware/Firmware + Controls |
| **E2E** | Create program → schedule → run → abort (interlock trip) → audit trail | Playwright + MQTT sim | Telemetry & Controls |

**Test Files:**
```
tests/unit/Irrigation/
├── InterlockEvaluationServiceTests.cs     # 20+ interlock scenarios
└── IrrigationAbortSagaTests.cs            # Compensator logic

tests/integration/Irrigation/
├── IrrigationOrchestratorTests.cs         # Run → step monitoring
└── CommandOutboxTests.cs                  # Idempotency

tests/contract/
└── device-command-contract.test.cs        # MQTT ack scenarios

tests/chaos/
├── hil-chaos-orchestrator.py              # HIL test runner
└── hil-test-suite/                        # Test 1-12 scripts

tests/e2e/
└── irrigation-flow.spec.ts                # Program → run → abort E2E
```

**Acceptance Criteria:**
- ✅ Unit tests: All interlock specs pass
- ✅ HIL: All 12 chaos tests pass (see HIL Playbook)
- ✅ Integration test: Enqueue→ack p95 < 800ms
- ✅ E2E test: Safe abort works, audit trail complete

**Critical:** Firmware sign-off required before pilot enablement

---

### FRP-07: Inventory, Scanning & GS1 Labels

| Test Type | Coverage | Tools | Owner |
|-----------|----------|-------|-------|
| **Unit** | UoM conversion service (property-based tests), FEFO allocation, Balance reconciliation | xUnit, FsCheck | Core Platform/Inventory |
| **Integration** | Lot movements (split, merge, adjust), Balance verification after splits | xUnit | Core Platform/Inventory |
| **Contract** | GS1 label validator (verify AI codes, barcode rendering) | Custom validator | Integrations/Labeling |
| **E2E** | Scan lot → move location → split lot → verify balances → print GS1 label | Playwright | Core Platform |

**Test Files:**
```
tests/unit/Inventory/
├── UomConversionServiceTests.cs           # Property-based (FsCheck)
├── FefoAllocationServiceTests.cs          # Allocation logic
└── BalanceReconciliationServiceTests.cs   # Balance verification

tests/integration/Inventory/
├── LotMovementTests.cs                    # Split, merge, adjust
└── BalanceAccuracyTests.cs                # Post-split balances

tests/contract/
└── gs1-label-validator.test.cs            # GS1 AI codes

tests/e2e/
└── inventory-scanning.spec.ts             # Scan → move → split E2E
```

**Acceptance Criteria:**
- ✅ Unit tests: UoM conversions 100% accurate (property-based)
- ✅ Integration test: Balances reconcile after splits
- ✅ E2E test: Scans update movements, GS1 labels render

---

### FRP-08: Processing & Manufacturing

| Test Type | Coverage | Tools | Owner |
|-----------|----------|-------|-------|
| **Unit** | Yield calculation, Cost rollup service | xUnit | Core Platform/Processing |
| **Integration** | Process runs (input consumption → output creation), Labor/waste capture | xUnit | Core Platform/Processing |
| **E2E** | Run process → capture labor → verify yields → check cost rollup | Playwright | Core Platform |

**Test Files:**
```
tests/unit/Processing/
├── YieldCalculationServiceTests.cs        # Yield math
└── CostRollupServiceTests.cs              # Materials + labor + overhead

tests/integration/Processing/
├── ProcessRunTests.cs                     # Input/output reconciliation
└── InventoryTransformationTests.cs        # Consume inputs, create FG

tests/e2e/
└── processing-flow.spec.ts                # Process run E2E
```

**Acceptance Criteria:**
- ✅ Integration test: Inputs reconcile, outputs correct
- ✅ E2E test: Labor/waste appear in costs, lineage preserved

---

### FRP-09: Compliance (METRC) & COA

| Test Type | Coverage | Tools | Owner |
|-----------|----------|-------|-------|
| **Unit** | COA parser (PDF/CSV), Hold gating policy | xUnit | Integrations/Compliance |
| **Integration** | Sync queue worker (retry/backoff), COA fail → HOLD creation | xUnit | Integrations/Compliance |
| **Contract** | METRC API mock (verify retry/backoff, idempotency) | Pact | Integrations/Compliance-METRC |
| **E2E** | Upload COA (fail) → HOLD created → destruction with two-person → audit export | Playwright | Integrations |

**Test Files:**
```
tests/unit/Compliance/
├── CoaIngestionServiceTests.cs            # PDF/CSV parsing
└── HoldGatingServiceTests.cs              # Gating policy

tests/integration/Compliance/
├── MetrcSyncTests.cs                      # Sync queue + retry
└── CoaHoldTests.cs                        # Failed COA → HOLD

tests/contract/
└── metrc-api-contract.test.cs             # METRC Pact scenarios

tests/e2e/
└── compliance-coa-hold.spec.ts            # COA fail → HOLD E2E
```

**Acceptance Criteria:**
- ✅ Contract test: Retry/backoff verified
- ✅ Integration test: Failed COA → HOLD enforced
- ✅ E2E test: Destruction with two-person, audit exportable

---

### FRP-10: QuickBooks Online (Item-Level)

| Test Type | Coverage | Tools | Owner |
|-----------|----------|-------|-------|
| **Unit** | QBO reconciliation service (variance calculation), DLQ replay logic | xUnit | Integrations/QuickBooks |
| **Integration** | Receiving → Bill creation (three-way match), Adaptive throttling | xUnit | Integrations/QuickBooks |
| **Contract** | QBO API mock (Request-ID idempotency, throttling) | Pact | Integrations/QuickBooks |
| **E2E** | Receive PO → QBO Bill created → variance < 0.5% → DLQ < 0.1% | Playwright | Integrations |

**Test Files:**
```
tests/unit/QuickBooks/
├── QboReconciliationServiceTests.cs       # Variance calc (A2)
└── DlqReplayServiceTests.cs               # Replay logic

tests/integration/QuickBooks/
├── ReceivingToBillTests.cs                # Bill creation
└── AdaptiveThrottlingTests.cs             # Rate limiting

tests/contract/
└── qbo-api-contract.test.cs               # QBO Pact scenarios

tests/e2e/
└── qbo-sync-recon.spec.ts                 # Receiving → Bill → Recon E2E
```

**Acceptance Criteria:**
- ✅ Integration test: Bills correct, amounts match
- ✅ E2E test: 7-day variance ≤ 0.5%, DLQ < 0.1%

---

### FRP-15: Notifications & Escalations

| Test Type | Coverage | Tools | Owner |
|-----------|----------|-------|-------|
| **Unit** | Notification routing policy (20+ scenarios), Dedup/bundling logic, Escalation chains | xUnit | Integrations/Notifications |
| **Integration** | Escalation chain (timeout → escalate), Slack/SMS delivery | xUnit | Integrations/Notifications |
| **E2E** | Alert fires → Slack notify → unacknowledged → escalate to SMS | Playwright | Integrations |

**Test Files:**
```
tests/unit/Notifications/
├── NotificationRoutingServiceTests.cs     # Policy engine
├── NotificationDedupTests.cs              # Storm control
└── NotificationPolicyTests.cs             # Quiet hours, burn-rate

tests/integration/Notifications/
├── EscalationChainTests.cs                # Timeout → escalate
└── SlackSmsDeliveryTests.cs               # Multi-channel

tests/e2e/
└── notification-escalation.spec.ts        # Alert → Slack → SMS E2E
```

**Acceptance Criteria:**
- ✅ Unit tests: Policy tests pass (quiet hours, escalation)
- ✅ Integration test: Escalation chain works
- ✅ E2E test: End-to-end escalation flow

---

## Test Automation & CI Integration

### CI Pipeline (GitHub Actions)

**File:** `.github/workflows/test-track-b.yml`

```yaml
name: Track B Test Suite

on:
  pull_request:
    paths:
      - 'src/backend/services/**'
      - 'tests/**'

jobs:
  unit-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Run Unit Tests
        run: |
          dotnet test tests/unit/ --filter "Category=Unit" --collect:"XPlat Code Coverage"
      - name: Check Coverage
        run: |
          dotnet reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage
          COVERAGE=$(grep -oP 'line-rate="\K[0-9.]+' coverage/index.html | head -1)
          if (( $(echo "$COVERAGE < 0.80" | bc -l) )); then
            echo "Coverage $COVERAGE < 80%. FAIL."
            exit 1
          fi

  integration-tests:
    runs-on: ubuntu-latest
    services:
      postgres:
        image: timescale/timescaledb:latest-pg15
      redis:
        image: redis:7-alpine
    steps:
      - uses: actions/checkout@v3
      - name: Run Integration Tests
        run: |
          dotnet test tests/integration/ --filter "Category=Integration"

  contract-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Run Contract Tests
        run: |
          dotnet test tests/contract/ --filter "Category=Contract"

  load-tests:
    runs-on: ubuntu-latest
    if: github.event.pull_request.base.ref == 'main'
    steps:
      - uses: actions/checkout@v3
      - name: Run k6 Load Tests
        run: |
          docker run --rm -i grafana/k6 run - <tests/load/telemetry-ingest-load.js
```

---

## Performance Testing (SLO Validation)

### Load Test Scenarios

| Scenario | Tool | Target SLO | Schedule |
|----------|------|-----------|----------|
| **Telemetry Ingest** | k6 | p95 < 1.0s @ 10k msg/s | PR merge to main |
| **Realtime Push** | k6 | p95 < 1.5s @ 1k concurrent | PR merge to main |
| **Command Dispatch** | k6 | p95 < 800ms | PR merge to main |
| **7-Day SLO Validation** | Prometheus + Python | All SLOs met for 7d | Staging pre-prod |

**Files:**
- `tests/load/telemetry-ingest-load.js` (existing)
- `tests/load/realtime-push-load.js` (existing)
- `tests/load/command-dispatch-load.js` (new)
- `tests/acceptance/run-acceptance-tests.sh` (existing, 7-day validation)

---

## Chaos Engineering (Beyond HIL)

### Service-Level Chaos

| Test | Injection | Expected Behavior |
|------|-----------|-------------------|
| **DB failover** | Kill primary Postgres | Promote replica, RTO < 30s |
| **MQTT broker loss** | Kill broker 60s | Devices buffer, reconcile on restore |
| **Redis cache loss** | Flush Redis | Degrade gracefully, rebuild cache |
| **API rate limit** | Spike 10x traffic | Throttle, 429 responses, no 500s |

**Tool:** Chaos Mesh or custom scripts

---

## Test Data Management

### Seed Data Script
**File:** `scripts/seed/seed-pilot-site.sql`

**Contents:**
- 1 Organization: Denver Grow Co. (Colorado, METRC)
- 1 Site: Denver Main Facility
- 2 Rooms: Veg (4 zones), Flower (6 zones)
- 10 Users: 4 Operators (badges), 3 Managers, 2 Compliance, 1 Admin
- 3 Strains: Blue Dream, OG Kush, Gorilla Glue
- 2 Batches: BD-V-001 (Veg), OG-F-002 (Flower)
- Equipment: 10 sensors, 5 valves, 2 pumps
- 20 Inventory Lots

**Idempotency:** DROP CASCADE + INSERT (safe for re-runs)

**CI Integration:** Run before every test suite

---

## Test Reporting

### Coverage Report (Codecov)
- Publish to Codecov after CI run
- PR comments show delta coverage
- Fail PR if coverage < 80%

### Test Results (xUnit + GitHub Actions)
- JUnit XML output → GitHub Actions test report
- Failed test annotations in PR

### Performance Trends (Grafana)
- Load test results → InfluxDB → Grafana
- Dashboard: "Track B Performance Trends"
- Alert if p95 exceeds SLO by 10%

---

## Acceptance Gate (7-Day Validation)

**Process:**
1. Deploy to staging
2. Run `tests/acceptance/run-acceptance-tests.sh` daily for 7 days
3. Query Prometheus for SLO metrics:
   - `histogram_quantile(0.95, telemetry_ingest_duration_seconds)` < 1.0s
   - `histogram_quantile(0.95, realtime_push_duration_seconds)` < 1.5s
   - `histogram_quantile(0.95, command_enqueue_duration_seconds)` < 0.8s
4. Generate pass/fail report
5. If all pass for 7 consecutive days → approve prod deployment

**Owner:** DevOps + TPM

---

## Test Ownership by Squad

| Squad | FRPs | Test Responsibilities |
|-------|------|----------------------|
| **Core Platform** | FRP-01, 02, 07, 08 | Unit, Integration, E2E for Identity, Spatial, Inventory, Processing |
| **Telemetry & Controls** | FRP-05, 06 | Unit, Integration, HIL, E2E for Telemetry, Irrigation |
| **Workflow & Messaging** | FRP-04 | Unit, Integration, E2E for Tasks, Slack bridge |
| **Integrations** | FRP-09, 10, 15 | Unit, Integration, Contract, E2E for Compliance, QBO, Notifications |
| **Hardware/Firmware** | FRP-06 (HIL) | HIL chaos drills, firmware sign-off |
| **DevOps/SRE** | All | CI/CD, load tests, 7-day validation, chaos engineering |

---

## Test Metrics & KPIs

| Metric | Target | Current | Trend |
|--------|--------|---------|-------|
| **Unit Test Coverage** | ≥ 80% | TBD | 🔼 |
| **Integration Test Coverage** | ≥ 60% | TBD | 🔼 |
| **E2E Test Coverage (Critical Paths)** | 100% | TBD | 🔼 |
| **Contract Test Coverage (External APIs)** | 100% | TBD | 🔼 |
| **HIL Test Pass Rate** | 100% | TBD | 🔼 |
| **Load Test SLO Pass Rate** | 100% | TBD | 🔼 |
| **7-Day Staging Validation** | 7 consecutive passes | TBD | 🔼 |

**Review Frequency:** Weekly sprint retrospectives

---

**Last Updated:** 2025-09-29  
**Version:** 1.0  
**Status:** ✅ Approved  
**Next Review:** After S1 (W2) completion
