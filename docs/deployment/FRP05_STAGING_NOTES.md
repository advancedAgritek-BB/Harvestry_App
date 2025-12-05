# FRP-05 Staging Deployment Notes — Telemetry Ingest & Rollups

**Purpose:** Run Telemetry API in staging with safe defaults (no WAL, optional MQTT) and prepare for performance validation.

---

## Environment

- `ASPNETCORE_ENVIRONMENT=Staging` (loads `appsettings.Staging.json`)
- Database: TimescaleDB-enabled PostgreSQL
- Real-time: Polling fallback (WAL disabled)
- MQTT: Optional; start HTTP-only first

---

## Configuration

The staging override is included at:

- `src/backend/services/telemetry-controls/telemetry/API/appsettings.Staging.json`

Defaults:
- `Telemetry:WalReplication:Enabled = false` (use polling fallback)
- `Telemetry:Mqtt:Enabled = false` (start HTTP-only)
- `Telemetry:Subscriptions:SnapshotIntervalSeconds = 10`

Recommended env vars:

```bash
export ASPNETCORE_ENVIRONMENT=Staging
export TELEMETRY_DB_CONNECTION="postgresql://user:pass@host:55432/harvestry_dev"
# Optional: enable MQTT later
# export Telemetry__Mqtt__Enabled=true
# export Telemetry__Mqtt__Host=broker.example.com
# export Telemetry__Mqtt__Port=1883
```

---

## Run API (local or staging host)

```bash
# From the Telemetry API project directory
cd src/backend/services/telemetry-controls/telemetry/API

# Verify environment
echo $ASPNETCORE_ENVIRONMENT  # should be Staging

# Run
dotnet run
```

Key endpoints:
- `POST /api/v1/telemetry/sites/{siteId}/equipment/{equipmentId}/ingest` (HTTP ingest)
- `GET  /api/v1/realtime/subscriptions` (diagnostics)

---

## Performance Validation (baseline)

```bash
# Ensure k6 installed
k6 version

# Baseline (5 minutes, 50 VUs)
k6 run --vus 50 --duration 5m tests/load/telemetry-ingest-load.js \
  -e BASE_URL=https://<staging-host> \
  -e SITE_ID=<guid> -e EQUIPMENT_ID=<guid>

# Targets: p95 < 1000 ms; error rate < 1%
```

For the sustained gate and realtime tests, see `docs/testing/FRP05_LOAD_TEST_PLAN.md`.

---

## Enable MQTT (optional, later)

```bash
export Telemetry__Mqtt__Enabled=true
export Telemetry__Mqtt__Host=<broker-host>
export Telemetry__Mqtt__Port=1883
# If authentication is required
export Telemetry__Mqtt__Username=<user>
export Telemetry__Mqtt__Password=<pass>
```

Sanity check:
```bash
mosquitto_pub -h <broker-host> -p 1883 -t test/topic -m hello
mosquitto_sub -h <broker-host> -p 1883 -t test/topic
```

---

## Notes

- WAL fan-out can be re-enabled later if the DB supports logical replication (set `Telemetry:WalReplication:Enabled=true`).
- Keep HTTP ingest enabled for initial validation; enable MQTT once broker connectivity and ACLs are confirmed.

