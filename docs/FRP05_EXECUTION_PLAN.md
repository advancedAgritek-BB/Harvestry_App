# FRP-05 Execution Plan - Telemetry Ingest & Rollups

**Date:** October 7, 2025  
**Status:** 🚧 In Progress — implementation delivered; validation pending  
**Approach:** Delivered via 4 vertical slices (see summary)  
**Estimated Time:** 30-39 hours (accounts for TimescaleDB setup, device adapters, real-time fan-out, and load testing)

---

## ✅ Execution Summary

- Core ingest, normalization, idempotency, and bulk insert implemented; endpoints live (HTTP), MQTT adapter wired via background worker.
- TimescaleDB migrations applied (schema, hypertable, compression, retention, continuous aggregates, publication) under `src/database/migrations/telemetry`.
- Query, alert rules, alerts, and realtime subscription endpoints implemented; SignalR hub and WAL fan‑out workers registered.
- Unit tests: 70 passing locally across domain/services/registry; load and integration tests pending execution.
- Outstanding: Day Zero infra validation (TimescaleDB privileges, replication), performance/load tests, realtime latency measurement, ops runbooks/dashboards.

> The detailed slice plan below is retained for context and onboarding.

## 📊 Current State Summary

### ✅ PREREQUISITES COMPLETE

- ✅ **FRP-01 Complete** - Identity, RLS, ABAC foundation
- ✅ **FRP-02 Complete** - Equipment registry for sensor association
- ✅ **TimescaleDB Available** - Extension ready for hypertables
- ✅ **MQTT Broker** - Available for device connectivity
- ✅ **API Infrastructure** - ASP.NET Core with established patterns
- ✅ **Test Infrastructure** - Integration test automation + k6 for load testing

### 🎯 TARGET DELIVERABLES

- 🎯 **High-Performance Ingest** - 10k msg/s sustained with p95 < 1.0s
- 🎯 **TimescaleDB Rollups** - Automatic 1m/5m/1h continuous aggregates
- 🎯 **Real-Time Fan-Out** - WebSocket/SSE push with p95 < 1.5s
- 🎯 **Alert Engine** - Threshold and deviation detection with < 30s latency
- 🎯 **Multi-Protocol Support** - MQTT, HTTP, and SDI-12 adapters

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
- ✅ Can test performance at each slice

**Note:** Total estimate is 35 hours over 6 days (accounts for TimescaleDB configuration, device adapter implementation, real-time infrastructure, performance tuning, and load testing).

---

## 📋 THE 4 SLICES

```
SLICE 1: Core Ingest & Storage
├── Services: TelemetryIngestService, NormalizationService, IdempotencyService
├── Repos: SensorStreamRepository, SensorReadingRepository (TimescaleDB)
├── Controllers: TelemetryIngestController, SensorStreamsController
├── DeviceAdapters: MqttIngestAdapter, HttpIngestAdapter
├── Validators: IngestValidators
└── Tests: Unit + Integration + Load

SLICE 2: Continuous Aggregations & Query
├── TimescaleDB: Continuous aggregate setup (1m/5m/1h)
├── Repos: Rollup query methods
├── Controller: TelemetryQueryController
├── Validators: QueryValidators
└── Tests: Rollup accuracy + freshness

SLICE 3: Alert Engine
├── Services: AlertEvaluationService
├── Workers: AlertEvaluationWorker
├── Repos: AlertRuleRepository, AlertInstanceRepository
├── Controllers: AlertRulesController, AlertsController
├── Validators: AlertValidators
└── Tests: Unit + Integration

SLICE 4: Real-Time Fan-Out
├── Service: WalFanoutService
├── Infrastructure: PostgreSQL WAL listener
├── API: SignalR Hub (WebSocket/SSE)
├── Workers: WAL processing worker
└── Tests: Real-time latency + subscription management
```

---

## 🧰 Pre-Slice Setup (2 hours)

Complete these shared tasks before starting the feature slices:

1. **TimescaleDB Extension Setup (30 min)**
   - Enable TimescaleDB extension in database
   - Configure hypertable settings
   - Test chunk sizing and compression policies
   - Document compression/retention strategy

2. **Domain Rehydration Helpers (45 min)**
   - Add static `FromPersistence(...)` factories to `SensorReading`, `AlertRule`, `AlertInstance`
   - Add `FromIngestion(...)` factory to `SensorReading` for device data
   - Keep persistence-specific guardrails inside factories

3. **DTO Mapping Profiles (20 min)**
   - Create AutoMapper profile for telemetry DTOs
   - Ensure efficient mapping for high-volume ingest

4. **Configuration & DI Checklist (25 min)**
   - Register services, repositories, validators, and mappers in API `Program.cs`
   - Configure MQTT broker connection settings
   - Configure SignalR settings (WebSocket/SSE)
   - Document environment variables in `docs/infra/environment-variables.md`

---

## 🔧 SLICE 1: CORE INGEST & STORAGE

**Goal:** High-performance telemetry ingest with normalization and deduplication  
**Time:** 12-15 hours (after shared pre-work)  
**Owner:** Telemetry & Controls Squad

### Task 1.1: Create Folder Structure (5 min)

```bash
# Create directories
mkdir -p src/backend/services/telemetry-controls/telemetry/Application/Interfaces
mkdir -p src/backend/services/telemetry-controls/telemetry/Application/DTOs
mkdir -p src/backend/services/telemetry-controls/telemetry/Application/Services
mkdir -p src/backend/services/telemetry-controls/telemetry/Application/DeviceAdapters
mkdir -p src/backend/services/telemetry-controls/telemetry/Infrastructure/Persistence
mkdir -p src/backend/services/telemetry-controls/telemetry/API/Controllers
mkdir -p src/backend/services/telemetry-controls/telemetry/API/Validators
```

### Task 1.2: Create Service Interfaces (25 min)

**File:** `Application/Interfaces/ITelemetryIngestService.cs`

```csharp
using Harvestry.Telemetry.Application.DTOs;
using Harvestry.Telemetry.Domain.Entities;
using Harvestry.Telemetry.Domain.Enums;

namespace Harvestry.Telemetry.Application.Interfaces;

/// <summary>
/// Service for ingesting telemetry data from multiple protocols
/// </summary>
public interface ITelemetryIngestService
{
    // Batch ingest
    Task<IngestResult> IngestBatchAsync(Guid siteId, IngestTelemetryRequest request, CancellationToken ct = default);
    
    // Protocol-specific ingest
    Task<IngestResult> IngestMqttMessageAsync(string topic, byte[] payload, CancellationToken ct = default);
    Task<IngestResult> IngestHttpMessageAsync(Guid equipmentId, SensorReadingDto[] readings, CancellationToken ct = default);
    
    // Session management
    Task<IngestionSession> StartSessionAsync(Guid siteId, Guid equipmentId, IngestionProtocol protocol, CancellationToken ct = default);
    Task UpdateSessionHeartbeatAsync(Guid sessionId, CancellationToken ct = default);
    Task EndSessionAsync(Guid sessionId, CancellationToken ct = default);
    
    // Error tracking
    Task<List<IngestionError>> GetRecentErrorsAsync(Guid siteId, int limit, CancellationToken ct = default);
}

/// <summary>
/// Service for normalizing sensor values and units
/// </summary>
public interface INormalizationService
{
    double ConvertUnit(double value, Unit sourceUnit, Unit targetUnit);
    bool ValidateReading(double value, Unit unit, StreamType streamType);
    QualityCode DetermineQualityCode(double value, Unit unit, StreamType streamType, DateTimeOffset? sourceTimestamp);
    (double normalized, Unit targetUnit) NormalizeReading(double value, Unit sourceUnit, StreamType streamType);
}

/// <summary>
/// Service for enforcing message idempotency
/// </summary>
public interface IIdempotencyService
{
    Task<bool> IsD duplicateAsync(Guid streamId, string messageId, CancellationToken ct = default);
    Task<int> DeduplicateBatchAsync(IReadOnlyCollection<SensorReading> readings, CancellationToken ct = default);
}
```

### Task 1.3: Create DTOs (40 min)

**File:** `Application/DTOs/TelemetryDtos.cs`

```csharp
using Harvestry.Telemetry.Domain.Enums;

namespace Harvestry.Telemetry.Application.DTOs;

public record IngestTelemetryRequest(
    Guid EquipmentId,
    List<SensorReadingDto> Readings
);

public record SensorReadingDto(
    Guid StreamId,
    double Value,
    Unit Unit,
    DateTimeOffset? SourceTimestamp,
    string? MessageId
);

public record IngestResult(
    int Accepted,
    int Duplicates,
    int Errors,
    TimeSpan IngestionDuration
);

public record CreateSensorStreamRequest(
    Guid EquipmentId,
    Guid? EquipmentChannelId,
    StreamType StreamType,
    Unit Unit,
    string DisplayName,
    Guid? LocationId,
    Guid? RoomId,
    Guid? ZoneId
);

public record SensorStreamResponse(
    Guid Id,
    Guid SiteId,
    Guid EquipmentId,
    Guid? EquipmentChannelId,
    StreamType StreamType,
    Unit Unit,
    string DisplayName,
    Guid? LocationId,
    Guid? RoomId,
    Guid? ZoneId,
    bool IsActive,
    DateTime CreatedAt
);

public record QueryTelemetryRequest(
    Guid StreamId,
    DateTimeOffset Start,
    DateTimeOffset End,
    RollupInterval? RollupInterval
);

public record TelemetryReadingResponse(
    DateTimeOffset Time,
    double Value,
    QualityCode QualityCode
);

public record TelemetryRollupResponse(
    DateTimeOffset Bucket,
    int SampleCount,
    double AvgValue,
    double MinValue,
    double MaxValue,
    double MedianValue,
    double? StdDevValue
);
```

### Task 1.4: Implement Services (3.5 hours)

**File:** `Application/Services/TelemetryIngestService.cs`

**Pattern to follow:** High-performance bulk insert pattern

**Key implementation notes:**
- Batch insert using COPY for TimescaleDB efficiency
- Call NormalizationService for unit conversion
- Call IdempotencyService for deduplication
- Create IngestionSession on first message
- Update session heartbeat every 30 seconds
- Log errors to ingestion_errors table
- Emit metrics for monitoring

**Estimated lines:** ~550

---

**File:** `Application/Services/NormalizationService.cs`

**Key implementation notes:**
- Unit conversion lookup table (degF ↔ degC, psi ↔ bar, etc.)
- Range validation per stream type (temp: -50 to 150°F, humidity: 0-100%, etc.)
- Quality code determination (good: 0, suspect: 64, bad: 192)
- Timestamp validation (reject readings > 5 minutes in future)

**Estimated lines:** ~300

---

**File:** `Application/Services/IdempotencyService.cs`

**Key implementation notes:**
- Check existence using unique index (stream_id, message_id)
- Batch deduplication before insert
- Return count of duplicates filtered

**Estimated lines:** ~150

### Task 1.5: Implement Device Adapters (2 hours)

**File:** `Application/DeviceAdapters/MqttIngestAdapter.cs`

**Key implementation notes:**
- Subscribe to MQTT topics: `site/{siteId}/equipment/{equipmentId}/telemetry`
- Parse JSON payload to SensorReadingDto[]
- Call TelemetryIngestService.IngestMqttMessageAsync
- Handle malformed messages gracefully
- Emit adapter metrics

**Estimated lines:** ~280

---

**File:** `Application/DeviceAdapters/HttpIngestAdapter.cs`

**Key implementation notes:**
- Accept POST to /api/telemetry/ingest/http/{equipmentId}
- Validate equipment exists and is active
- Call TelemetryIngestService.IngestHttpMessageAsync
- Return 200 OK with ingest result

**Estimated lines:** ~150

### Task 1.6: Create Repositories (3 hours)

**File:** `Infrastructure/Persistence/SensorStreamRepository.cs`

**Pattern to follow:** `identity/Infrastructure/Persistence/UserRepository.cs`

**Key methods:**
```csharp
- GetByIdAsync (with RLS context)
- GetBySiteAsync
- GetByEquipmentAsync
- GetActiveStreamsAsync
- InsertAsync
- UpdateAsync
- DeactivateAsync
```

**RLS Context:** 
```csharp
const string setRlsSql = "SET LOCAL app.current_user_id = @userId";
```

**Estimated lines:** ~280

---

**File:** `Infrastructure/Persistence/SensorReadingRepository.cs`

**Key methods:**
```csharp
- BulkInsertAsync (using COPY for performance)
- GetByStreamAsync (time range query)
- GetRollupsAsync (query continuous aggregates)
- GetLatestAsync (last reading per stream)
- GetLatestBatchAsync (batch query for dashboard)
- ExistsAsync (idempotency check)
```

**TimescaleDB Optimization:**
```csharp
// Use COPY for bulk insert (10x faster than individual inserts)
await using var writer = connection.BeginBinaryImport(
    "COPY sensor_readings (time, stream_id, value, quality_code, source_timestamp, ingestion_timestamp, message_id) FROM STDIN BINARY");
```

**Estimated lines:** ~450

### Task 1.7: Create Controllers (1.5 hours)

**File:** `API/Controllers/TelemetryIngestController.cs`

**Endpoints to implement:**
- POST /api/telemetry/ingest
- POST /api/telemetry/ingest/mqtt/{equipmentId}
- POST /api/telemetry/ingest/http/{equipmentId}
- GET /api/sites/{siteId}/telemetry/errors

**Estimated lines:** ~200

---

**File:** `API/Controllers/SensorStreamsController.cs`

**Endpoints to implement:**
- POST /api/sites/{siteId}/sensor-streams
- GET /api/sites/{siteId}/sensor-streams
- GET /api/sites/{siteId}/sensor-streams/{streamId}
- PUT /api/sites/{siteId}/sensor-streams/{streamId}
- DELETE /api/sites/{siteId}/sensor-streams/{streamId}

**Estimated lines:** ~180

### Task 1.8: Create Validators (45 min)

**File:** `API/Validators/TelemetryValidators.cs`

**Validators needed:**
- IngestTelemetryRequestValidator
- SensorReadingDtoValidator
- CreateSensorStreamRequestValidator

**Estimated lines:** ~180 total

### Task 1.9: Unit Tests (2 hours)

**File:** `Tests/Unit/Services/NormalizationServiceTests.cs`

**Tests needed:**
- Unit conversion accuracy (all pairs)
- Range validation per stream type
- Quality code determination
- Timestamp validation

**Estimated lines:** ~350

---

**File:** `Tests/Unit/Services/IdempotencyServiceTests.cs` (~150 lines)
**File:** `Tests/Unit/Domain/SensorReadingTests.cs` (~200 lines)

### Task 1.10: Integration Tests (2 hours)

**File:** `Tests/Integration/TelemetryIngestIntegrationTests.cs`

**Tests needed:**
- Ingest batch → verify readings stored
- Duplicate message → idempotency check
- Unit conversion during ingest
- RLS: cross-site stream access blocked
- Session management (start → heartbeat → end)

**Estimated lines:** ~400

---

**File:** `Tests/Integration/RlsTelemetryTests.cs` (~180 lines)

### Task 1.11: Load Tests (1.5 hours)

**File:** `tests/load/telemetry-ingest.js` (k6 script)

**Test scenarios:**
- Ramp-up: 0 → 10k msg/s over 2 minutes
- Sustained: 10k msg/s for 15 minutes
- Measure: p95 latency, error rate, throughput

**Acceptance:** p95 < 1.0s, error rate < 0.1%

**Estimated lines:** ~150

### Slice 1 Summary

**Total Time:** 12-15 hours  
**Total Files:** 18 files  
**Total Lines:** ~3,800 lines

**Deliverables:**
- ✅ Service interfaces + implementations (3 services)
- ✅ 2 device adapters (MQTT, HTTP)
- ✅ 2 repositories with RLS and TimescaleDB optimization
- ✅ 2 controllers with 9 endpoints
- ✅ 3 validator files
- ✅ 5 test files (unit + integration + load)

---

## 🔧 SLICE 2: CONTINUOUS AGGREGATIONS & QUERY

**Goal:** TimescaleDB rollups with query API  
**Time:** 8-10 hours  
**Dependencies:** Slice 1 (sensor_readings table must exist)

### Quick Overview (Detailed steps similar to Slice 1)

**TimescaleDB Setup:**
- Create continuous aggregates (1m/5m/1h) with refresh policies
- Configure compression and retention policies
- Validate rollup accuracy and freshness

**Repository:** Extend `SensorReadingRepository` with rollup queries (~250 lines)

- GetRollups1mAsync, GetRollups5mAsync, GetRollups1hAsync
- Query materialized views with time bucketing
- Efficient aggregation queries (AVG, MIN, MAX, STDDEV)

**Controller:** `TelemetryQueryController.cs` (~300 lines)

- GET /api/sites/{siteId}/telemetry/streams/{streamId}/readings
- GET /api/sites/{siteId}/telemetry/streams/{streamId}/rollups
- GET /api/sites/{siteId}/telemetry/streams/{streamId}/latest
- GET /api/sites/{siteId}/telemetry/streams/latest (batch query)

**Validators:** `QueryValidators.cs` (~120 lines)

**Tests:** 3 files (~450 lines total)

- RollupAccuracyTests.cs (unit - verify aggregate calculations)
- RollupFreshnessTests.cs (integration - verify refresh < 60s)
- QueryPerformanceTests.cs (integration - verify query p95 < 200ms)

---

## 🔧 SLICE 3: ALERT ENGINE

**Goal:** Rule-based alert evaluation with threshold/deviation detection  
**Time:** 6-8 hours  
**Dependencies:** Slice 1 (sensor readings), Slice 2 (rollup queries for evaluation)

### Quick Overview

**Service:** `AlertEvaluationService.cs` (~450 lines)

- EvaluateRulesAsync (evaluate all active rules for site)
- EvaluateRuleAsync (single rule evaluation)
- FireAlertAsync, ClearAlertAsync
- GetActiveAlertsAsync, AcknowledgeAlertAsync
- Threshold evaluation (above, below, range)
- Deviation evaluation (percent, absolute)
- Rate-of-change evaluation
- Missing data detection

**Worker:** `AlertEvaluationWorker.cs` (~280 lines)

- Background service running every 30 seconds
- Fetch active rules per site
- Evaluate against recent readings (evaluation_window_minutes)
- Fire/clear alerts based on results
- Respect cooldown periods

**Repositories:**
- `AlertRuleRepository.cs` (~280 lines)
- `AlertInstanceRepository.cs` (~250 lines)

**Controllers:**
- `AlertRulesController.cs` (~280 lines) - CRUD for alert rules
- `AlertsController.cs` (~220 lines) - Query and acknowledge alerts

**Validators:** `AlertValidators.cs` (~180 lines)

**Tests:** 3 files (~500 lines total)

- AlertRuleTests.cs (unit - evaluation logic)
- AlertEvaluationServiceTests.cs (unit)
- AlertIntegrationTests.cs (integration - E2E alert lifecycle)

---

## 🔧 SLICE 4: REAL-TIME FAN-OUT

**Goal:** WebSocket/SSE push for live dashboards with p95 < 1.5s  
**Time:** 4-6 hours  
**Dependencies:** Slice 1 (sensor readings)

### Quick Overview

**Service:** `WalFanoutService.cs` (~400 lines)

- StartListeningAsync (connect to PostgreSQL WAL)
- StopListeningAsync
- SubscribeAsync (connection subscribes to stream IDs)
- UnsubscribeAsync
- Fan-out new readings to subscribed connections
- Connection management (track active subscriptions)

**Infrastructure:** PostgreSQL WAL Listener (~250 lines)

- Use Npgsql.Replication for logical replication
- Filter WAL events for sensor_readings inserts
- Parse WAL data to SensorReading
- Emit to WalFanoutService

**API:** SignalR Hub (~200 lines)

- `TelemetryHub.cs` - WebSocket/SSE endpoint
- Subscribe(streamIds[]) - client subscribes to streams
- Unsubscribe() - client unsubscribes
- Push readings to subscribed clients
- Handle connection lifecycle (connect, disconnect, reconnect)

**Worker:** `WalFanoutWorker.cs` (~150 lines)

- Background service managing WAL listener
- Restart on connection loss
- Health check monitoring

**Tests:** 2 files (~350 lines total)

- WalFanoutServiceTests.cs (unit - subscription management)
- RealTimeFanoutTests.cs (integration - measure latency, verify delivery)

---

## 📅 RECOMMENDED TIMELINE

### Day 1 (6 hours) - Foundation + Slice 1 Part 1

- Morning (2h): Pre-slice setup (TimescaleDB, rehydration factories, configuration)
- Midday (2h): Tasks 1.1-1.3 (folder structure, service interfaces, DTOs)
- Afternoon (2h): Begin Task 1.4 (ingest service implementation)

### Day 2 (7 hours) - Slice 1 Part 2

- Morning (3.5h): Complete Task 1.4 (NormalizationService + IdempotencyService)
- Afternoon (3.5h): Task 1.5 (MqttIngestAdapter + HttpIngestAdapter)

### Day 3 (7 hours) - Slice 1 Part 3

- Morning (3h): Task 1.6 (SensorStreamRepository + SensorReadingRepository with TimescaleDB optimization)
- Afternoon (2h): Task 1.7 (controllers)
- Evening (2h): Tasks 1.8-1.9 (validators + unit tests)

### Day 4 (5 hours) - Slice 1 Complete + Slice 2 Start

- Morning (2h): Task 1.10-1.11 (integration tests + load tests)
- Afternoon (3h): Begin Slice 2 (continuous aggregates + query API)

### Day 5 (6 hours) - Slice 2 Complete + Slice 3 Start

- Morning (3h): Complete Slice 2 (rollup queries + tests)
- Afternoon (3h): Begin Slice 3 (alert service + worker)

### Day 6 (6 hours) - Slice 3 Complete + Slice 4

- Morning (3h): Complete Slice 3 (alert repositories + controllers + tests)
- Afternoon (3h): Slice 4 (WAL fan-out + SignalR hub + tests)

**Total:** 37 hours over 6 days (includes pre-slice setup, implementation, performance optimization, load testing, and real-time infrastructure)

---

## 🎯 Definition of Done

Each slice is complete when:

- ✅ Service implemented with all methods
- ✅ Repository implemented with RLS context
- ✅ Controller with all endpoints and OpenAPI docs
- ✅ FluentValidation validators
- ✅ Unit tests passing (≥90% coverage)
- ✅ Integration tests passing (including RLS verification)
- ✅ Performance targets met (load/latency)
- ✅ No linter errors
- ✅ Manual API testing via Swagger
- ✅ Program.cs, appsettings, and deployment secrets updated

---

## 🔧 Helper Commands

### Run Tests

```bash
# Run all tests with coverage
dotnet test /p:CollectCoverage=true

# Run load tests
cd tests/load
k6 run telemetry-ingest.js

# Monitor ingestion rate
watch -n 1 "psql -c \"SELECT COUNT(*) FROM sensor_readings WHERE ingestion_timestamp > NOW() - INTERVAL '1 minute';\""
```

### Monitor TimescaleDB

```bash
# Check hypertable chunks
psql -c "SELECT * FROM timescaledb_information.chunks WHERE hypertable_name = 'sensor_readings' ORDER BY range_end DESC LIMIT 10;"

# Check compression status
psql -c "SELECT * FROM timescaledb_information.compression_settings WHERE hypertable_name = 'sensor_readings';"

# Check continuous aggregate refresh status
psql -c "SELECT * FROM timescaledb_information.continuous_aggregate_stats;"
```

### Monitor Real-Time Fan-Out

```bash
# Check active SignalR connections
psql -c "SELECT COUNT(*) FROM (SELECT DISTINCT connection_id FROM telemetry_subscriptions) AS active_connections;"

# Check WAL lag
psql -c "SELECT pg_current_wal_lsn(), pg_last_wal_replay_lsn(), pg_last_wal_replay_lsn() - pg_current_wal_lsn() AS lag_bytes;"
```

---

## 📋 Progress Tracking

After each task, update:

1. `docs/TRACK_B_COMPLETION_CHECKLIST.md` - Mark checkboxes
2. `docs/FRP05_CURRENT_STATUS.md` - Update % complete
3. Git commit with meaningful message

Example commit messages:

```
feat(frp05): implement telemetry ingest service with bulk insert
feat(frp05): add TimescaleDB continuous aggregates (1m/5m/1h)
feat(frp05): implement alert evaluation service with threshold detection
feat(frp05): add SignalR hub for real-time telemetry fan-out
test(frp05): add load test for 10k msg/s sustained throughput
```

---

## 🚦 Quality Gates

Before marking a slice complete, verify:

1. ✅ All tests passing
2. ✅ Code coverage ≥90% for services
3. ✅ Load test passing (15 min sustained, p95 < 1.0s)
4. ✅ RLS verification tests passing
5. ✅ Swagger UI loads without errors
6. ✅ Manual testing of happy path works
7. ✅ No linter warnings
8. ✅ Follows FRP-01/FRP-02/FRP-03 patterns

---

## ⚠️ Key Risks & Mitigations

- **Database privilege gaps:** TimescaleDB setup, compression, and logical replication need superuser rights; confirm with DevOps on Day 0 and keep a contingency path (reduced feature toggle) if managed Postgres blocks any commands.
- **Performance validation timeboxed:** If load tests fail to reach 10k msg/s by Day 4, trigger the escalation path (optimize COPY batching, relax targets, or expand infrastructure) rather than slipping the schedule silently.
- **Real-time fan-out dependencies:** WAL listener relies on replication slots and network reachability; schedule connectivity dry-runs before Day 6 so Slice 4 effort is implementation-only.
- **External device coordination:** Hardware/adapters must publish sample telemetry early; work with the sensors team before Slice 1 Task 1.5 to secure topics and payload fixtures.

---

## 🎯 Success Metrics

FRP-05 is successfully complete when:

- ✅ All 4 slices delivered and tested
- ✅ All checklist items marked complete
- ✅ All acceptance criteria met (ingest p95 < 1.0s, rollup < 60s, push p95 < 1.5s)
- ✅ Load test sustained for 15 minutes
- ✅ Documentation updated
- ✅ Ready for FRP-06 (Irrigation with telemetry interlocks)

---

**Ready to start?** Begin with **Pre-Slice Setup** then **Slice 1, Task 1.1** 🚀
