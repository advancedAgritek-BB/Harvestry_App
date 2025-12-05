# FRP-05 Load Test Plan — Telemetry Ingest & Real-Time

**Scope:** Validate ingest throughput/latency and realtime fan-out performance for FRP‑05  
**Owner:** Telemetry & Controls + DevOps  
**Environments:** Staging (primary), Local (smoke), Prod (canary only)

---

## Objectives

- Ingest 10k messages/sec sustained for 15 minutes with p95 < 1000 ms
- Maintain error rate < 1% during sustained load
- Continuous aggregate freshness under 60 seconds
- Realtime push (WebSocket) p95 < 1.5s under representative subscription counts

---

## Prerequisites

- TimescaleDB schema and policies applied (`src/database/migrations/telemetry/001-006`)
- Telemetry API deployed with COPY bulk insert enabled
- MQTT broker credentials (if testing MQTT path)
- k6 installed on load driver nodes

---

## Test Artifacts

- k6 script: `tests/load/telemetry-ingest-load.js`
- Diagnostics endpoint: `GET /api/v1/realtime/subscriptions`
- DB views: `timescaledb_information.*` (freshness checks)

---

## Execution

1) Baseline (smoke)

```bash
# Ensure k6 installed
k6 version

# Baseline helper (outputs JSON into logs/)
BASE_URL=https://staging.example.com \
SITE_ID=$SITE_ID EQUIPMENT_ID=$EQUIPMENT_ID \
./scripts/load/run-k6-baseline.sh
```

Target: p95 < 1000 ms, error rate < 1%

2) Sustained gate (primary)

```bash
# Sustained helper (outputs JSON into logs/)
BASE_URL=https://staging.example.com \
SITE_ID=$SITE_ID EQUIPMENT_ID=$EQUIPMENT_ID \
./scripts/load/run-k6-sustained.sh
```

Target: 10k msg/s, p95 < 1000 ms, error < 1%

3) Realtime latency

Procedure:
- Open N WebSocket subscriptions (N = 100, 500, 1000) to `/hubs/telemetry`
- Inject updates → measure client receive timestamps vs. insert time

Acceptance: p95 < 1.5s at N ≤ 500; document scaling characteristics at N=1000

---

## Rollup Freshness Check

```sql
-- Check last refresh times for CAGGs
SELECT view_name, last_successful_finish
FROM timescaledb_information.continuous_aggregates
ORDER BY view_name;
```

Acceptance: Max(now() - last_successful_finish) < 60 seconds during sustained load

---

## Reporting

- Export k6 summary (JSON) and attach to `docs/FRP05_DAY_ZERO_RESULTS.md`
- Capture DB metrics: WAL lag, chunk creation/compression counts
- Record `/api/v1/realtime/subscriptions` snapshots before/after test

---

## Remediation Playbook (if failing)

- Ingest p95 > 1s: tune COPY batch size/bytes, increase pool size, add CPU
- Error rate > 1%: investigate duplicate keys/validation rejects; relax limits prudently
- CAGG freshness > 60s: increase refresh cadence or window; scale DB
- Realtime p95 > 1.5s: reduce fan-out frequency, scale hub instances, consider partitioning by site
