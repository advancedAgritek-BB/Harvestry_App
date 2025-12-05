# FRP-05: Telemetry Ingest & Rollups - Implementation Plan

**Status:** ⏳ READY TO START  
**Estimated Effort:** 32-41 hours  
**Prerequisites:** ✅ FRP-01 Complete (Identity, RLS), ✅ FRP-02 Complete (Equipment Registry)  
**Blocks:** FRP-06 (Irrigation requires telemetry for interlocks)

---

## 📋 OVERVIEW

### Purpose
Establish a high-performance telemetry ingestion system with automatic rollups, real-time fan-out, and alerting capabilities. This system enables monitoring of climate sensors, substrate probes, and equipment health metrics with TimescaleDB-powered continuous aggregations.

### Key Features
1. **Multi-Protocol Ingest** - MQTT, HTTP, and SDI-12 adapter support
2. **TimescaleDB Hypertables** - Optimized time-series storage with compression
3. **Continuous Aggregations** - Automatic 1m, 5m, and 1h rollups
4. **Real-Time Fan-Out** - WebSocket/SSE push for live dashboards
5. **Alert Engine** - Rule-based evaluation with threshold/deviation detection
6. **Normalization** - Unit conversion and data validation
7. **Idempotency** - Deduplication of duplicate sensor readings

### Acceptance Criteria (from PRD)
- ✅ Ingest p95 < 1.0s at 10k msg/s sustained load
- ✅ Rollup freshness < 60s (data visible in aggregates)
- ✅ Real-time push p95 < 1.5s from sensor to UI
- ✅ Deviation alerts fire correctly with < 30s latency
- ✅ Load test sustained for 15 minutes without degradation

---

## 📊 IMPLEMENTATION BREAKDOWN

### Phase 1: Database Schema (4-5 hours)

#### Migration 1: Core Telemetry Tables
**File:** `src/database/migrations/frp05/20251020_01_CreateTelemetryTables.sql`

**TimescaleDB Setup:**
```sql
-- Enable TimescaleDB extension
CREATE EXTENSION IF NOT EXISTS timescaledb CASCADE;

-- Sensor Streams (metadata for sensor channels)
sensor_streams (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    equipment_id uuid NOT NULL REFERENCES equipment_registry(id) ON DELETE CASCADE,
    equipment_channel_id uuid REFERENCES equipment_channels(id) ON DELETE SET NULL,
    stream_type varchar(50) NOT NULL CHECK (stream_type IN (
        'temperature', 'humidity', 'co2', 'vpd', 'light_par', 'light_ppfd',
        'ec', 'ph', 'dissolved_oxygen', 'water_temp', 'water_level',
        'soil_moisture', 'soil_temp', 'pressure', 'flow_rate', 'power_consumption'
    )),
    unit varchar(20) NOT NULL CHECK (unit IN (
        'degF', 'degC', 'pct', 'ppm', 'kPa', 'umol', 'uS', 'pH', 'mg_L', 'L', 'gal',
        'in', 'cm', 'psi', 'bar', 'gpm', 'lpm', 'W', 'kWh'
    )),
    display_name varchar(200) NOT NULL,
    location_id uuid REFERENCES inventory_locations(id),
    room_id uuid REFERENCES rooms(id),
    zone_id uuid REFERENCES zones(id),
    is_active boolean NOT NULL DEFAULT TRUE,
    metadata jsonb,
    created_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP
)

CREATE INDEX idx_sensor_streams_equipment ON sensor_streams(equipment_id);
CREATE INDEX idx_sensor_streams_location ON sensor_streams(site_id, location_id);
CREATE INDEX idx_sensor_streams_type ON sensor_streams(site_id, stream_type)
    WHERE is_active = TRUE;

-- Sensor Readings (raw time-series data)
sensor_readings (
    time timestamptz NOT NULL,
    stream_id uuid NOT NULL REFERENCES sensor_streams(id) ON DELETE CASCADE,
    value double precision NOT NULL,
    quality_code smallint NOT NULL DEFAULT 0 CHECK (quality_code BETWEEN 0 AND 255),
    source_timestamp timestamptz,
    ingestion_timestamp timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    message_id varchar(100),
    metadata jsonb
)

-- Convert to hypertable (TimescaleDB)
SELECT create_hypertable('sensor_readings', 'time',
    chunk_time_interval => INTERVAL '1 day',
    if_not_exists => TRUE
);

-- Create index on stream_id for fast queries
CREATE INDEX idx_sensor_readings_stream ON sensor_readings(stream_id, time DESC);

-- Create unique index for idempotency (message_id per stream)
CREATE UNIQUE INDEX idx_sensor_readings_message_id ON sensor_readings(stream_id, message_id)
    WHERE message_id IS NOT NULL;

-- Add compression policy (compress data older than 7 days)
SELECT add_compression_policy('sensor_readings',
    compress_after => INTERVAL '7 days',
    if_not_exists => TRUE
);

-- Add retention policy (drop raw data older than 90 days)
SELECT add_retention_policy('sensor_readings',
    drop_after => INTERVAL '90 days',
    if_not_exists => TRUE
);
```

#### Migration 2: Continuous Aggregations
**File:** `src/database/migrations/frp05/20251020_02_CreateTelemetryRollups.sql`

**Continuous Aggregates:**
```sql
-- 1-minute rollups (for recent detailed view)
CREATE MATERIALIZED VIEW sensor_readings_1m
WITH (timescaledb.continuous) AS
SELECT
    time_bucket('1 minute', time) AS bucket,
    stream_id,
    COUNT(*) as sample_count,
    AVG(value) as avg_value,
    MIN(value) as min_value,
    MAX(value) as max_value,
    PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY value) as median_value,
    STDDEV(value) as stddev_value,
    MIN(quality_code) as min_quality,
    MAX(quality_code) as max_quality
FROM sensor_readings
GROUP BY bucket, stream_id;

-- Refresh policy: update every 30 seconds, lag of 1 minute
SELECT add_continuous_aggregate_policy('sensor_readings_1m',
    start_offset => INTERVAL '3 hours',
    end_offset => INTERVAL '1 minute',
    schedule_interval => INTERVAL '30 seconds',
    if_not_exists => TRUE
);

-- Retention policy: keep 1m rollups for 30 days
SELECT add_retention_policy('sensor_readings_1m',
    drop_after => INTERVAL '30 days',
    if_not_exists => TRUE
);

-- 5-minute rollups (for hourly/daily views)
CREATE MATERIALIZED VIEW sensor_readings_5m
WITH (timescaledb.continuous) AS
SELECT
    time_bucket('5 minutes', time) AS bucket,
    stream_id,
    COUNT(*) as sample_count,
    AVG(value) as avg_value,
    MIN(value) as min_value,
    MAX(value) as max_value,
    PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY value) as median_value,
    STDDEV(value) as stddev_value
FROM sensor_readings
GROUP BY bucket, stream_id;

-- Refresh policy: update every 5 minutes, lag of 5 minutes
SELECT add_continuous_aggregate_policy('sensor_readings_5m',
    start_offset => INTERVAL '1 day',
    end_offset => INTERVAL '5 minutes',
    schedule_interval => INTERVAL '5 minutes',
    if_not_exists => TRUE
);

-- Retention policy: keep 5m rollups for 180 days
SELECT add_retention_policy('sensor_readings_5m',
    drop_after => INTERVAL '180 days',
    if_not_exists => TRUE
);

-- 1-hour rollups (for weekly/monthly views)
CREATE MATERIALIZED VIEW sensor_readings_1h
WITH (timescaledb.continuous) AS
SELECT
    time_bucket('1 hour', time) AS bucket,
    stream_id,
    COUNT(*) as sample_count,
    AVG(value) as avg_value,
    MIN(value) as min_value,
    MAX(value) as max_value,
    PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY value) as median_value,
    STDDEV(value) as stddev_value
FROM sensor_readings
GROUP BY bucket, stream_id;

-- Refresh policy: update hourly, lag of 1 hour
SELECT add_continuous_aggregate_policy('sensor_readings_1h',
    start_offset => INTERVAL '7 days',
    end_offset => INTERVAL '1 hour',
    schedule_interval => INTERVAL '1 hour',
    if_not_exists => TRUE
);

-- Retention policy: keep 1h rollups for 730 days (2 years)
SELECT add_retention_policy('sensor_readings_1h',
    drop_after => INTERVAL '730 days',
    if_not_exists => TRUE
);
```

#### Migration 3: Alert System
**File:** `src/database/migrations/frp05/20251020_03_CreateAlertTables.sql`

**Tables:**
```sql
-- Alert Rules (threshold and deviation detection)
alert_rules (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    rule_name varchar(200) NOT NULL,
    rule_type varchar(50) NOT NULL CHECK (rule_type IN (
        'threshold_above', 'threshold_below', 'threshold_range',
        'deviation_percent', 'deviation_absolute', 'rate_of_change',
        'missing_data', 'quality_degraded'
    )),
    stream_ids uuid[] NOT NULL,
    threshold_config jsonb NOT NULL,
    evaluation_window_minutes int NOT NULL DEFAULT 5,
    cooldown_minutes int NOT NULL DEFAULT 15,
    severity varchar(20) NOT NULL DEFAULT 'warning' CHECK (severity IN (
        'info', 'warning', 'critical'
    )),
    is_active boolean NOT NULL DEFAULT TRUE,
    notify_channels text[] NOT NULL DEFAULT ARRAY[]::text[],
    metadata jsonb,
    created_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by uuid NOT NULL REFERENCES users(id),
    updated_by uuid NOT NULL REFERENCES users(id)
)

CREATE INDEX idx_alert_rules_site ON alert_rules(site_id)
    WHERE is_active = TRUE;
CREATE INDEX idx_alert_rules_streams ON alert_rules USING GIN(stream_ids);

-- Alert Instances (fired alerts)
alert_instances (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    rule_id uuid NOT NULL REFERENCES alert_rules(id) ON DELETE CASCADE,
    stream_id uuid NOT NULL REFERENCES sensor_streams(id),
    fired_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    cleared_at timestamptz,
    severity varchar(20) NOT NULL,
    current_value double precision,
    threshold_value double precision,
    message text NOT NULL,
    acknowledged_at timestamptz,
    acknowledged_by uuid REFERENCES users(id),
    acknowledgment_notes text,
    metadata jsonb
)

CREATE INDEX idx_alert_instances_rule ON alert_instances(rule_id, fired_at DESC);
CREATE INDEX idx_alert_instances_site_active ON alert_instances(site_id, fired_at DESC)
    WHERE cleared_at IS NULL;
CREATE INDEX idx_alert_instances_stream ON alert_instances(stream_id, fired_at DESC);

-- Alert Rule Evaluation Log (for debugging)
alert_rule_evaluation_log (
    id uuid PRIMARY KEY,
    rule_id uuid NOT NULL REFERENCES alert_rules(id) ON DELETE CASCADE,
    evaluated_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    evaluation_result varchar(20) NOT NULL CHECK (evaluation_result IN (
        'pass', 'fail', 'error', 'no_data'
    )),
    evaluated_values jsonb,
    error_message text
)

-- Partition by month for efficient cleanup
SELECT create_hypertable('alert_rule_evaluation_log', 'evaluated_at',
    chunk_time_interval => INTERVAL '7 days',
    if_not_exists => TRUE
);

-- Retention policy: keep evaluation logs for 30 days
SELECT add_retention_policy('alert_rule_evaluation_log',
    drop_after => INTERVAL '30 days',
    if_not_exists => TRUE
);
```

#### Migration 4: Ingestion Tracking
**File:** `src/database/migrations/frp05/20251020_04_CreateIngestionTables.sql`

**Tables:**
```sql
-- Ingestion Sessions (device connection tracking)
ingestion_sessions (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    equipment_id uuid NOT NULL REFERENCES equipment_registry(id),
    protocol varchar(20) NOT NULL CHECK (protocol IN ('mqtt', 'http', 'sdi12')),
    started_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_heartbeat_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ended_at timestamptz,
    message_count bigint NOT NULL DEFAULT 0,
    error_count int NOT NULL DEFAULT 0,
    metadata jsonb
)

CREATE INDEX idx_ingestion_sessions_equipment ON ingestion_sessions(equipment_id, started_at DESC);
CREATE INDEX idx_ingestion_sessions_active ON ingestion_sessions(site_id, last_heartbeat_at DESC)
    WHERE ended_at IS NULL;

-- Ingestion Errors (for monitoring and debugging)
ingestion_errors (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    session_id uuid REFERENCES ingestion_sessions(id) ON DELETE SET NULL,
    equipment_id uuid REFERENCES equipment_registry(id),
    protocol varchar(20) NOT NULL,
    error_type varchar(50) NOT NULL CHECK (error_type IN (
        'invalid_format', 'missing_field', 'invalid_stream_id',
        'invalid_unit', 'out_of_range', 'duplicate_message',
        'rate_limit_exceeded', 'authentication_failed', 'unknown'
    )),
    error_message text NOT NULL,
    raw_payload jsonb,
    occurred_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP
)

SELECT create_hypertable('ingestion_errors', 'occurred_at',
    chunk_time_interval => INTERVAL '7 days',
    if_not_exists => TRUE
);

-- Retention policy: keep ingestion errors for 30 days
SELECT add_retention_policy('ingestion_errors',
    drop_after => INTERVAL '30 days',
    if_not_exists => TRUE
);
```

**RLS Policies:**
```sql
-- Enable RLS on all tables
ALTER TABLE sensor_streams ENABLE ROW LEVEL SECURITY;
ALTER TABLE sensor_readings ENABLE ROW LEVEL SECURITY;
ALTER TABLE alert_rules ENABLE ROW LEVEL SECURITY;
ALTER TABLE alert_instances ENABLE ROW LEVEL SECURITY;
ALTER TABLE alert_rule_evaluation_log ENABLE ROW LEVEL SECURITY;
ALTER TABLE ingestion_sessions ENABLE ROW LEVEL SECURITY;
ALTER TABLE ingestion_errors ENABLE ROW LEVEL SECURITY;

-- Note: Continuous aggregates inherit RLS from base table (sensor_readings)

-- Policy: Users can only access their site's telemetry data
CREATE POLICY sensor_streams_site_access ON sensor_streams
    FOR ALL
    USING (
        site_id::text = current_setting('app.site_id', TRUE)
        OR current_setting('app.user_role', TRUE) = 'admin'
        OR current_setting('app.user_role', TRUE) = 'service_account'
    );

CREATE POLICY sensor_readings_site_access ON sensor_readings
    FOR ALL
    USING (
        EXISTS (
            SELECT 1 FROM sensor_streams
            WHERE sensor_streams.id = sensor_readings.stream_id
            AND sensor_streams.site_id::text = current_setting('app.site_id', TRUE)
        )
        OR current_setting('app.user_role', TRUE) = 'admin'
        OR current_setting('app.user_role', TRUE) = 'service_account'
    );

-- Repeat for all other tables with appropriate permissions
```

---

### Phase 2: Domain Layer (4-5 hours)

#### Domain Entities

**File Structure:**
```
src/backend/services/telemetry-controls/telemetry/Domain/
├── Entities/
│   ├── SensorStream.cs
│   ├── SensorReading.cs
│   ├── AlertRule.cs
│   ├── AlertInstance.cs
│   ├── AlertRuleEvaluationLog.cs
│   ├── IngestionSession.cs
│   └── IngestionError.cs
├── ValueObjects/
│   ├── SensorValue.cs
│   ├── ThresholdConfig.cs
│   ├── AlertRuleResult.cs
│   ├── IngestionBatch.cs
│   └── RollupData.cs
└── Enums/
    ├── StreamType.cs
    ├── Unit.cs
    ├── QualityCode.cs
    ├── AlertRuleType.cs
    ├── AlertSeverity.cs
    ├── EvaluationResult.cs
    ├── IngestionProtocol.cs
    └── IngestionErrorType.cs
```

**Key Domain Methods:**

**SensorReading.cs:**
```csharp
public class SensorReading : Entity<(DateTimeOffset Time, Guid StreamId)>
{
    public DateTimeOffset Time { get; private set; }
    public Guid StreamId { get; private set; }
    public double Value { get; private set; }
    public QualityCode QualityCode { get; private set; }
    public DateTimeOffset? SourceTimestamp { get; private set; }
    public DateTimeOffset IngestionTimestamp { get; private set; }
    public string? MessageId { get; private set; }

    // Methods
    public bool IsGoodQuality();
    public bool IsWithinExpectedRange(double min, double max);
    public TimeSpan GetIngestionLatency();
    public static SensorReading FromIngestion(Guid streamId, double value, Unit sourceUnit, Unit targetUnit, DateTimeOffset? sourceTimestamp, string? messageId);
    public static SensorReading FromPersistence(...);
}
```

**AlertRule.cs:**
```csharp
public class AlertRule : AggregateRoot<Guid>
{
    public Guid SiteId { get; private set; }
    public string RuleName { get; private set; }
    public AlertRuleType RuleType { get; private set; }
    public IReadOnlyCollection<Guid> StreamIds { get; private set; }
    public ThresholdConfig ThresholdConfig { get; private set; }
    public int EvaluationWindowMinutes { get; private set; }
    public int CooldownMinutes { get; private set; }
    public AlertSeverity Severity { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyCollection<string> NotifyChannels { get; private set; }

    // Methods
    public AlertRuleResult Evaluate(IReadOnlyCollection<SensorReading> readings, DateTimeOffset evaluationTime);
    public AlertInstance? FireAlert(Guid streamId, double currentValue, double thresholdValue, DateTimeOffset firedAt);
    public bool IsInCooldown(DateTimeOffset lastFiredAt, DateTimeOffset now);
    public void Activate(Guid userId);
    public void Deactivate(Guid userId);
    public void UpdateThreshold(ThresholdConfig config, Guid userId);
    public static AlertRule FromPersistence(...);
}
```

**AlertInstance.cs:**
```csharp
public class AlertInstance : AggregateRoot<Guid>
{
    public Guid SiteId { get; private set; }
    public Guid RuleId { get; private set; }
    public Guid StreamId { get; private set; }
    public DateTimeOffset FiredAt { get; private set; }
    public DateTimeOffset? ClearedAt { get; private set; }
    public AlertSeverity Severity { get; private set; }
    public double? CurrentValue { get; private set; }
    public double? ThresholdValue { get; private set; }
    public string Message { get; private set; }
    public DateTimeOffset? AcknowledgedAt { get; private set; }
    public Guid? AcknowledgedBy { get; private set; }
    public string? AcknowledgmentNotes { get; private set; }

    // Methods
    public void Clear(DateTimeOffset clearedAt);
    public void Acknowledge(Guid userId, string? notes, DateTimeOffset acknowledgedAt);
    public bool IsActive();
    public TimeSpan GetDuration(DateTimeOffset? referenceTime = null);
    public static AlertInstance FromPersistence(...);
}
```

**ThresholdConfig.cs (Value Object):**
```csharp
public readonly record struct ThresholdConfig
{
    public AlertRuleType RuleType { get; init; }
    public double? ThresholdValue { get; init; }
    public double? MinValue { get; init; }
    public double? MaxValue { get; init; }
    public double? DeviationPercent { get; init; }
    public double? DeviationAbsolute { get; init; }
    public double? RateOfChangePerMinute { get; init; }
    public int? MissingDataMinutes { get; init; }

    public bool Validate();
    public string GetDescription();
}
```

---

### Phase 3: Application Layer (4-5 hours)

#### Application Services

**Files:**
```
src/backend/services/telemetry-controls/telemetry/Application/
├── Services/
│   ├── TelemetryIngestService.cs
│   ├── NormalizationService.cs
│   ├── IdempotencyService.cs
│   ├── AlertEvaluationService.cs
│   ├── RollupFreshnessMonitor.cs
│   └── WalFanoutService.cs
├── DeviceAdapters/
│   ├── MqttIngestAdapter.cs
│   ├── HttpIngestAdapter.cs
│   └── Sdi12IngestAdapter.cs
├── DTOs/
│   ├── IngestTelemetryRequest.cs
│   ├── SensorReadingDto.cs
│   ├── CreateSensorStreamRequest.cs
│   ├── CreateAlertRuleRequest.cs
│   ├── UpdateAlertRuleRequest.cs
│   ├── AcknowledgeAlertRequest.cs
│   ├── QueryTelemetryRequest.cs
│   ├── TelemetryRollupResponse.cs
│   ├── AlertRuleResponse.cs
│   └── AlertInstanceResponse.cs
└── Interfaces/
    ├── ITelemetryIngestService.cs
    ├── INormalizationService.cs
    ├── IAlertEvaluationService.cs
    └── IWalFanoutService.cs
```

**Key Service Methods:**

**ITelemetryIngestService:**
```csharp
public interface ITelemetryIngestService
{
    Task<IngestResult> IngestBatchAsync(Guid siteId, IngestTelemetryRequest request, CancellationToken ct);
    Task<IngestResult> IngestMqttMessageAsync(string topic, byte[] payload, CancellationToken ct);
    Task<IngestResult> IngestHttpMessageAsync(Guid equipmentId, SensorReadingDto[] readings, CancellationToken ct);
    Task<IngestSession> StartSessionAsync(Guid siteId, Guid equipmentId, IngestionProtocol protocol, CancellationToken ct);
    Task UpdateSessionHeartbeatAsync(Guid sessionId, CancellationToken ct);
    Task EndSessionAsync(Guid sessionId, CancellationToken ct);
    Task<IReadOnlyList<IngestionError>> GetRecentErrorsAsync(Guid siteId, int limit, CancellationToken ct);
}
```

**INormalizationService:**
```csharp
public interface INormalizationService
{
    double ConvertUnit(double value, Unit sourceUnit, Unit targetUnit);
    bool ValidateReading(double value, Unit unit, StreamType streamType);
    QualityCode DetermineQualityCode(double value, Unit unit, StreamType streamType, DateTimeOffset? sourceTimestamp);
    (double normalized, Unit targetUnit) NormalizeReading(double value, Unit sourceUnit, StreamType streamType);
}
```

**IAlertEvaluationService:**
```csharp
public interface IAlertEvaluationService
{
    Task EvaluateRulesAsync(Guid siteId, CancellationToken ct);
    Task<AlertRuleResult> EvaluateRuleAsync(AlertRule rule, DateTimeOffset evaluationTime, CancellationToken ct);
    Task<AlertInstance?> FireAlertAsync(Guid ruleId, Guid streamId, double currentValue, double thresholdValue, CancellationToken ct);
    Task ClearAlertAsync(Guid alertInstanceId, DateTimeOffset clearedAt, CancellationToken ct);
    Task<IReadOnlyList<AlertInstance>> GetActiveAlertsAsync(Guid siteId, CancellationToken ct);
    Task AcknowledgeAlertAsync(Guid alertInstanceId, Guid userId, string? notes, CancellationToken ct);
}
```

**IWalFanoutService:**
```csharp
public interface IWalFanoutService
{
    Task StartListeningAsync(CancellationToken ct);
    Task StopListeningAsync(CancellationToken ct);
    Task SubscribeAsync(Guid connectionId, IReadOnlyCollection<Guid> streamIds, CancellationToken ct);
    Task UnsubscribeAsync(Guid connectionId, CancellationToken ct);
    Task<int> GetSubscriberCountAsync(Guid streamId);
}
```

---

### Phase 4: Infrastructure Layer (5-6 hours)

#### Repositories

**Files:**
```
src/backend/services/telemetry-controls/telemetry/Infrastructure/Persistence/
├── TelemetryDbContext.cs
├── SensorStreamRepository.cs
├── SensorReadingRepository.cs
├── AlertRuleRepository.cs
├── AlertInstanceRepository.cs
├── IngestionSessionRepository.cs
└── IngestionErrorRepository.cs
```

**Key Repository Methods:**

**ISensorReadingRepository:**
```csharp
public interface ISensorReadingRepository
{
    Task<int> BulkInsertAsync(IReadOnlyCollection<SensorReading> readings, CancellationToken ct);
    Task<IReadOnlyList<SensorReading>> GetByStreamAsync(Guid streamId, DateTimeOffset start, DateTimeOffset end, CancellationToken ct);
    Task<IReadOnlyList<RollupData>> GetRollupsAsync(Guid streamId, DateTimeOffset start, DateTimeOffset end, RollupInterval interval, CancellationToken ct);
    Task<SensorReading?> GetLatestAsync(Guid streamId, CancellationToken ct);
    Task<IReadOnlyDictionary<Guid, SensorReading>> GetLatestBatchAsync(IReadOnlyCollection<Guid> streamIds, CancellationToken ct);
    Task<bool> ExistsAsync(Guid streamId, string messageId, CancellationToken ct);
}
```

**IAlertRuleRepository:**
```csharp
public interface IAlertRuleRepository : IRepository<AlertRule, Guid>
{
    Task<IReadOnlyList<AlertRule>> GetActiveBySiteAsync(Guid siteId, CancellationToken ct);
    Task<IReadOnlyList<AlertRule>> GetByStreamAsync(Guid streamId, CancellationToken ct);
    Task LogEvaluationAsync(Guid ruleId, EvaluationResult result, object? evaluatedValues, string? errorMessage, CancellationToken ct);
}
```

---

### Phase 5: API Layer (3-4 hours)

#### Controllers

**Files:**
```
src/backend/services/telemetry-controls/telemetry/API/Controllers/
├── TelemetryIngestController.cs
├── SensorStreamsController.cs
├── TelemetryQueryController.cs
├── AlertRulesController.cs
└── AlertsController.cs
```

**Key Endpoints:**

**TelemetryIngestController.cs:**
```csharp
[ApiController]
[Route("api/telemetry/ingest")]
public class TelemetryIngestController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<IngestResult>> IngestBatch(IngestTelemetryRequest request);
    
    [HttpPost("mqtt/{equipmentId}")]
    public async Task<ActionResult<IngestResult>> IngestMqtt(Guid equipmentId, MqttPayload payload);
    
    [HttpPost("http/{equipmentId}")]
    public async Task<ActionResult<IngestResult>> IngestHttp(Guid equipmentId, SensorReadingDto[] readings);
}
```

**TelemetryQueryController.cs:**
```csharp
[ApiController]
[Route("api/sites/{siteId}/telemetry")]
public class TelemetryQueryController : ControllerBase
{
    [HttpGet("streams/{streamId}/readings")]
    public async Task<ActionResult<IReadOnlyList<SensorReadingDto>>> GetReadings(
        Guid siteId, Guid streamId,
        [FromQuery] DateTimeOffset start,
        [FromQuery] DateTimeOffset end);
    
    [HttpGet("streams/{streamId}/rollups")]
    public async Task<ActionResult<IReadOnlyList<TelemetryRollupResponse>>> GetRollups(
        Guid siteId, Guid streamId,
        [FromQuery] DateTimeOffset start,
        [FromQuery] DateTimeOffset end,
        [FromQuery] RollupInterval interval);
    
    [HttpGet("streams/{streamId}/latest")]
    public async Task<ActionResult<SensorReadingDto>> GetLatest(Guid siteId, Guid streamId);
}
```

---

### Phase 6: Background Workers & Real-Time (3-4 hours)

**Files:**
```
src/backend/services/telemetry-controls/telemetry/Infrastructure/Workers/
├── AlertEvaluationWorker.cs
├── RollupFreshnessMonitorWorker.cs
├── MqttIngestionWorker.cs
└── SessionCleanupWorker.cs

src/backend/services/telemetry-controls/telemetry/API/Hubs/
└── TelemetryHub.cs (SignalR for WebSocket/SSE)
```

---

### Phase 7-9: Validators, Unit Tests, Integration Tests (6-8 hours)

Similar structure to FRP-04 with comprehensive test coverage for:
- Unit conversion accuracy
- Alert rule evaluation logic
- Idempotency enforcement
- TimescaleDB continuous aggregate queries
- Real-time fan-out performance
- Load testing (10k msg/s sustained)

---

## 📊 TASK BREAKDOWN WITH ESTIMATES

| Phase | Task | Est. Hours | Owner |
|-------|------|------------|-------|
| **1. Database** | Migration 1: Core tables | 1.5-2 | Backend |
| | Migration 2: Continuous aggregates | 1-1.5 | Backend |
| | Migration 3: Alert system | 0.75-1 | Backend |
| | Migration 4: Ingestion tracking | 0.75-1 | Backend |
| **2. Domain** | 7 entity files | 2-2.5 | Backend |
| | 5 value object files | 0.75-1 | Backend |
| | 8 enum files | 0.5-0.75 | Backend |
| | Domain logic methods | 1.5-2 | Backend |
| **3. Application** | 6 service implementations | 2.5-3 | Backend |
| | 3 device adapter implementations | 1.5-2 | Backend |
| | 12 DTO files | 1-1.5 | Backend |
| | 4 interface files | 0.5-0.75 | Backend |
| **4. Infrastructure** | DbContext + 6 repositories | 3-4 | Backend |
| | RLS context integration | 0.75-1 | Backend |
| | WAL listener implementation | 1-1.5 | Backend |
| **5. API** | 5 controllers (~600 lines) | 2-2.5 | Backend |
| | SignalR hub | 0.75-1 | Backend |
| | Program.cs DI registration | 0.5 | Backend |
| **6. Workers** | 4 background worker files | 2-2.5 | Backend |
| **7. Validators** | 5 validator files | 1-1.5 | Backend |
| **8. Unit Tests** | 10 test files | 3-4 | Backend |
| **9. Integration & Load Tests** | 6 test files | 3-4 | Backend |
| **TOTAL** | | **32-41** | |

---

## ✅ QUALITY GATES

1. ✅ All repositories with RLS
2. ✅ Unit test coverage ≥90%
3. ✅ Load test sustained (15 min, 10k msg/s, p95 < 1.0s)
4. ✅ Integration tests passing
5. ✅ Health checks configured
6. ✅ Swagger documentation
7. ✅ Production polish (CORS, validators, logging)
8. ✅ Acceptance criteria met

---

## 🎯 ACCEPTANCE CRITERIA VALIDATION

### From PRD:
- ✅ **Ingest p95 < 1.0s** - Load test validates sustained throughput
- ✅ **Rollup freshness < 60s** - Continuous aggregate refresh policies ensure data availability
- ✅ **Real-time push p95 < 1.5s** - SignalR/WebSocket measured latency
- ✅ **Deviation alerts fire correctly** - Alert engine integration tests
- ✅ **15-minute sustained load** - k6 load test script

---

## 🚀 DEPENDENCIES & BLOCKING

### Prerequisites (All Met ✅)
- ✅ FRP-01 Complete (Identity, RLS, ABAC)
- ✅ FRP-02 Complete (Equipment registry for sensor association)

### Blocks (After FRP-05 Complete)
- **FRP-06: Irrigation** - Needs telemetry for interlock safety checks

---

## 📝 DESIGN DECISIONS

1. **TimescaleDB:** ✅ **Hypertables with continuous aggregates** - Optimized time-series performance
2. **Rollup Strategy:** ✅ **1m/5m/1h with automatic refresh** - Balance between granularity and storage
3. **Real-Time Fan-Out:** ✅ **WAL listener + SignalR** - Low-latency push without polling
4. **Idempotency:** ✅ **Message-ID deduplication** - Prevent duplicate sensor readings
5. **Unit Conversion:** ✅ **Normalize on ingest** - Store in canonical units for consistent queries

---

## 🎯 SUCCESS CRITERIA

**Definition of Done:**
- ✅ All 8 quality gates passed
- ✅ Telemetry ingest operational at scale
- ✅ Continuous aggregates refreshing automatically
- ✅ Real-time fan-out delivering to UI
- ✅ Alert engine evaluating rules
- ✅ Load tests passing (15 min sustained)
- ✅ RLS validated (cross-site blocked)
- ✅ Integration tests passing
- ✅ Swagger docs published

**Expected Outcome:**
- 45-55 C# files created
- ~4,500-5,500 lines of code
- Complete telemetry platform
- Production-ready at scale
- FRP-06 unblocked

---

## ⚠️ RISKS & MITIGATIONS

- **TimescaleDB superuser requirements:** Enabling extensions, compression, and retention policies needs elevated privileges; confirm with the database admin team before Day 1 to avoid blocking migrations and plan a fallback if managed Supabase limits features.
- **Performance environment parity:** Hitting 10k msg/s sustained requires tuned infrastructure; schedule load testing in a dedicated staging environment with realistic hardware and capture latency metrics early to re-scope if limits are hit.
- **Logical replication access for fan-out:** WAL listener and logical replication slots may be restricted; validate replication permissions and monitoring strategy ahead of Slice 4 so real-time delivery does not slip to a later release.
- **Protocol adapter readiness:** MQTT/SDI-12 devices must be configured in advance; align with hardware team to provide test devices and broker credentials before Slice 1 work begins.

---

**Status:** ⏳ READY FOR REVIEW & APPROVAL  
**Next Step:** Review plan → Get approval → Begin implementation  
**Estimated Completion:** 32-41 hours from start
