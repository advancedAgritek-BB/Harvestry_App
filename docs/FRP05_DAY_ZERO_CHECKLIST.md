# FRP-05 Day Zero Checklist

**Purpose:** Infrastructure validation and setup before Slice 1 execution  
**Duration:** 4-6 hours  
**Owner:** DevOps + Telemetry & Controls Squad  
**Status:** ⏳ Not Started

---

## 🎯 OBJECTIVES

Validate that all infrastructure prerequisites are operational and ready to support FRP-05 execution. Identify any limitations and implement fallback strategies before development begins.

---

## ✅ CHECKLIST

### 1. TimescaleDB Validation (1-1.5 hours)

#### Extension Enablement
- [ ] Connect to target database (dev/staging)
- [ ] Execute: `CREATE EXTENSION IF NOT EXISTS timescaledb CASCADE;`
- [ ] Verify extension version: `SELECT extversion FROM pg_extension WHERE extname = 'timescaledb';`
- [ ] **Expected:** timescaledb 2.x or higher
- [ ] **If fails:** Document error; escalate to database team; consider managed time-series alternative

#### Hypertable Creation Test
- [ ] Create test table: `CREATE TABLE test_hypertable (time TIMESTAMPTZ NOT NULL, value DOUBLE PRECISION);`
- [ ] Convert to hypertable: `SELECT create_hypertable('test_hypertable', 'time', chunk_time_interval => INTERVAL '1 day');`
- [ ] Verify: `SELECT * FROM timescaledb_information.hypertables WHERE hypertable_name = 'test_hypertable';`
- [ ] Insert test data: `INSERT INTO test_hypertable VALUES (NOW(), 42.0);`
- [ ] Query test data: `SELECT * FROM test_hypertable;`
- [ ] **If fails:** Document error; consider PostgreSQL partitioning fallback

#### Compression Policy Test
- [ ] Add compression policy: `SELECT add_compression_policy('test_hypertable', compress_after => INTERVAL '1 hour');`
- [ ] Verify: `SELECT * FROM timescaledb_information.jobs WHERE proc_name = 'policy_compression';`
- [ ] Wait 5 minutes and check if compression executed
- [ ] **If fails:** Document error; manual compression may be required

#### Retention Policy Test
- [ ] Add retention policy: `SELECT add_retention_policy('test_hypertable', drop_after => INTERVAL '7 days');`
- [ ] Verify: `SELECT * FROM timescaledb_information.jobs WHERE proc_name = 'policy_retention';`
- [ ] **If fails:** Document error; manual cleanup job required

#### Continuous Aggregate Test
- [ ] Create continuous aggregate:
  ```sql
  CREATE MATERIALIZED VIEW test_agg
  WITH (timescaledb.continuous) AS
  SELECT time_bucket('5 minutes', time) AS bucket, AVG(value) as avg_value
  FROM test_hypertable
  GROUP BY bucket;
  ```
- [ ] Add refresh policy: `SELECT add_continuous_aggregate_policy('test_agg', start_offset => INTERVAL '1 hour', end_offset => INTERVAL '1 minute', schedule_interval => INTERVAL '5 minutes');`
- [ ] Verify: `SELECT * FROM timescaledb_information.continuous_aggregates WHERE view_name = 'test_agg';`
- [ ] **If fails:** Document error; consider application-level rollups

#### Cleanup
- [ ] Drop test objects: `DROP MATERIALIZED VIEW test_agg CASCADE; DROP TABLE test_hypertable CASCADE;`

---

### 2. Logical Replication Validation (30-45 minutes)

#### Replication Slot Creation
- [ ] Check replication permissions: `SELECT * FROM pg_roles WHERE rolname = current_user;`
- [ ] Verify `replication` privilege exists
- [ ] Create test replication slot: `SELECT * FROM pg_create_logical_replication_slot('test_slot', 'pgoutput');`
- [ ] List slots: `SELECT * FROM pg_replication_slots WHERE slot_name = 'test_slot';`
- [ ] Drop test slot: `SELECT pg_drop_replication_slot('test_slot');`
- [ ] **If fails:** Document error; design polling-based fallback for real-time push

#### WAL Level Check
- [ ] Verify WAL level: `SHOW wal_level;`
- [ ] **Expected:** `logical`
- [ ] **If not:** Request database team to enable logical replication; document timeline

#### Replication Connection Test
- [ ] Test connection string with replication flag
- [ ] Verify `max_replication_slots` setting: `SHOW max_replication_slots;`
- [ ] **Expected:** >= 5 slots available
- [ ] **If insufficient:** Request increase or plan to share slots

---

### 3. MQTT Broker Setup (1-1.5 hours)

#### Broker Configuration
- [ ] Identify MQTT broker endpoint (URL, port)
- [ ] Document broker type (Mosquitto, EMQ, AWS IoT Core, etc.)
- [ ] Verify authentication method (username/password, certificates, tokens)
- [ ] Test broker connectivity from staging environment

#### Topic Design
- [ ] Define topic format: `site/{siteId}/equipment/{equipmentId}/telemetry`
- [ ] Document QoS level (recommend: QoS 1 for at-least-once delivery)
- [ ] Define retained message policy
- [ ] Configure topic ACLs (access control)

#### Test Device Setup
- [ ] Provision 2-3 test sensor stream IDs
- [ ] Create test payload JSON schema:
  ```json
  {
    "stream_id": "uuid",
    "timestamp": "ISO-8601",
    "value": 0.0,
    "unit": "degF|pct|ppm|etc",
    "quality_code": 0,
    "message_id": "unique-string"
  }
  ```
- [ ] Publish test message to broker
- [ ] Subscribe and verify message received
- [ ] Document topic, payload, and credentials in `docs/telemetry/mqtt-configuration.md`

#### Rate Limit Testing
- [ ] Test broker with 100 msg/s for 1 minute
- [ ] Test broker with 1k msg/s for 30 seconds (if possible)
- [ ] Document observed throughput and latency
- [ ] **If < 10k msg/s:** Plan infrastructure upgrade or accept reduced target

---

### 4. Environment Variables Provisioning (30 minutes)

#### Required Variables (from `docs/infra/environment-variables.md`)
- [ ] `TELEMETRY_DB_CONNECTION` - Set to staging database connection string
- [ ] `TELEMETRY_MQTT_BROKER_URL` - Set to broker endpoint
- [ ] `TELEMETRY_MQTT_USERNAME` - Set (if required)
- [ ] `TELEMETRY_MQTT_PASSWORD` - Store in secrets manager, reference in env
- [ ] `TELEMETRY_MAX_BATCH_SIZE` - Set to `5000`
- [ ] `TELEMETRY_COPY_BATCH_BYTES` - Set to `1048576` (1 MiB)
- [ ] `TELEMETRY_ALERT_EVALUATION_INTERVAL` - Set to `30` seconds
- [ ] `TELEMETRY_ALERT_COOLDOWN_MINUTES` - Set to `15`
- [ ] `TELEMETRY_SIGNALR_ALLOWED_ORIGINS` - Set to staging frontend URL
- [ ] `TELEMETRY_WAL_SLOT_NAME` - Set to `telemetry_wal_slot`

#### Secrets Management
- [ ] Store MQTT credentials in secrets manager (AWS Secrets Manager, Vault, etc.)
- [ ] Test secret retrieval from application
- [ ] Document secret names and access policies

---

### 5. SignalR/WebSocket Configuration (30 minutes)

- [ ] Configure CORS for SignalR hub
- [ ] Set allowed origins in `TELEMETRY_SIGNALR_ALLOWED_ORIGINS`
- [ ] Configure WebSocket transport settings
- [ ] Set connection limits (e.g., 1000 concurrent connections)
- [ ] Test WebSocket connection from browser console
- [ ] Document configuration in `docs/telemetry/signalr-configuration.md`

---

### 6. Load Test Environment Preparation (1 hour)

#### Environment Provisioning
- [ ] Provision staging environment with production-like specs:
  - Database: 4-8 CPU, 16-32 GB RAM
  - API: 2-4 instances, 2-4 CPU each, 8-16 GB RAM each
  - MQTT Broker: Dedicated instance or managed service
- [ ] Configure auto-scaling (if available)
- [ ] Set up monitoring (Prometheus, Grafana)

#### Test Data Preparation
- [ ] Create 10-20 test sensor streams in database
- [ ] Document stream IDs in `tests/load/test-streams.json`
- [ ] Verify k6 load test script: `tests/load/telemetry-ingest-load.js`
- [ ] Run smoke test (10 VUs for 1 minute)
- [ ] **Expected:** p95 < 1.0s, error rate < 1%

#### Baseline Performance Test
- [ ] Run k6 test: `k6 run --vus 50 --duration 5m tests/load/telemetry-ingest-load.js`
- [ ] Document baseline results:
  - Throughput (requests/sec)
  - p95 latency (ms)
  - p99 latency (ms)
  - Error rate (%)
- [ ] **If p95 > 1.0s:** Identify bottlenecks; optimize or scale infrastructure

---

### 7. Monitoring & Alerting Setup (30 minutes)

#### Prometheus Metrics
- [ ] Configure scrape endpoints for telemetry service
- [ ] Verify metrics exposed: `/metrics`
- [ ] Document custom metrics:
  - `telemetry_ingest_total`
  - `telemetry_ingest_duration_seconds`
  - `telemetry_ingest_errors_total`
  - `telemetry_queue_depth`
  - `telemetry_alert_evaluations_total`

#### Grafana Dashboards
- [ ] Create "Telemetry Ingest" dashboard with panels:
  - Ingest throughput (req/s)
  - Ingest latency (p50/p95/p99)
  - Error rate (%)
  - Queue depth
- [ ] Create "Telemetry Performance" dashboard with panels:
  - Database write latency
  - MQTT broker latency
  - SignalR connection count
  - Alert evaluation latency

#### Alerts
- [ ] Configure alert rules:
  - Ingest p95 > 1.0s for 5 minutes → warning
  - Ingest p95 > 2.5s for 5 minutes → critical
  - Error rate > 1% for 5 minutes → warning
  - Queue depth > 1000 for 10 minutes → warning
- [ ] Test alert firing and notification delivery

---

### 8. Documentation & Coordination (30 minutes)

#### Day 0 Results Document
- [ ] Create `docs/FRP05_DAY_ZERO_RESULTS.md` with:
  - TimescaleDB validation results (pass/fail for each feature)
  - Logical replication status (available/unavailable)
  - MQTT broker configuration (URL, topics, auth, throughput)
  - Environment variables provisioned (list)
  - Load test baseline results (throughput, latency)
  - Identified limitations and fallback strategies

#### Stakeholder Communication
- [ ] Share results with DevOps Lead
- [ ] Share results with Database Team
- [ ] Share results with Sensors Team
- [ ] Share results with Telemetry & Controls Squad Lead
- [ ] Get sign-off to proceed to Slice 1

#### Fallback Strategies (if needed)
- [ ] **If logical replication unavailable:** Document polling-based real-time push design (acceptable 2-5s latency)
- [ ] **If MQTT broker unavailable:** Document HTTP-only ingest for Phase 1, MQTT in future phase
- [ ] **If compression/retention unavailable:** Document manual cleanup job design
- [ ] **If load test performance below target:** Document infrastructure scaling plan or reduced target

---

## 🚦 GO/NO-GO DECISION

### ✅ GO Criteria (Must meet ALL):
- [ ] TimescaleDB extension enabled and hypertables working
- [ ] Continuous aggregates working (or application-level rollup fallback designed)
- [ ] MQTT broker configured and tested OR HTTP-only fallback agreed
- [ ] Environment variables provisioned and tested
- [ ] Load test baseline demonstrates feasible path to target (even if infrastructure needs scaling)
- [ ] Monitoring and alerting operational

### 🛑 NO-GO Criteria (Any of these):
- [ ] TimescaleDB extension cannot be enabled (no workaround)
- [ ] Database performance incapable of supporting target (no scaling path)
- [ ] MQTT broker unavailable AND no HTTP-only fallback agreed
- [ ] Critical secrets not provisioned (MQTT credentials, etc.)

---

## 📋 DELIVERABLES

After Day Zero completion:

1. **Day Zero Results Document** (`docs/FRP05_DAY_ZERO_RESULTS.md`)
2. **MQTT Configuration Doc** (`docs/telemetry/mqtt-configuration.md`)
3. **SignalR Configuration Doc** (`docs/telemetry/signalr-configuration.md`)
4. **Load Test Baseline Report** (metrics + screenshots)
5. **Fallback Strategies Doc** (if limitations found)
6. **Stakeholder Sign-Off** (email or Slack thread)

---

## 📞 CONTACTS

- **DevOps Lead:** [Name/Contact]
- **Database Team:** [Name/Contact]
- **Sensors Team:** [Name/Contact]
- **Telemetry & Controls Squad Lead:** [Name/Contact]

---

**Status:** ⏳ Ready to Execute  
**Next Action:** Schedule Day Zero session with DevOps and Telemetry & Controls Squad  
**Estimated Completion:** 4-6 hours (can be split across multiple sessions if needed)

