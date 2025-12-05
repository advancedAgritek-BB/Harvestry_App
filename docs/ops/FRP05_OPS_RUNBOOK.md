# FRP-05 Ops Runbook — Telemetry Ingest & Real‑Time

**Audience:** SRE/On‑Call + Telemetry & Controls  
**Scope:** Ingest health, realtime delivery, rollups, and fallbacks

---

## Quick Links

- Health endpoints
  - `GET /api/v1/realtime/subscriptions` — connection/subscription snapshot
- Logs/metrics (suggested)
  - Ingest throughput (req/s), p95 latency, error rate
  - Polling backlog size (if wal disabled), worker loop duration
  - Timescale metrics: chunk creation/compression, CAGG refresh age

---

## Run Modes

- WAL Fan‑Out (preferred): requires logical replication; sub‑second realtime
- Polling Fallback (default in Staging): no special DB perms; 2–5s latency OK
  - appsettings.Staging.json sets `Telemetry:WalReplication:Enabled=false`
  - Worker: `TelemetryRealtimePollingWorker`

---

## Common Procedures

### A. Switch to Polling Fallback

- Set `Telemetry:WalReplication:Enabled=false` (env: `Telemetry__WalReplication__Enabled=false`)
- Ensure `TelemetryRealtimePollingWorker` is enabled (default)
- Observe subscription snapshot and ingest metrics for 10 minutes

### B. Re‑enable WAL Fan‑Out

- Confirm DB grants: `wal_level=logical`, free replication slot, user has REPLICATION
- Set `Telemetry:WalReplication:Enabled=true`
- Monitor WAL lag and SignalR delivery p95

### C. MQTT Reconnect Tuning

- Options: `Telemetry:Mqtt:ReconnectIntervalSeconds`, TLS toggle, topic filter
- Validate broker reachability with `mosquitto_pub/sub`
- Watch ingest error logs for auth/connect failures

### D. Rollup Freshness

- Check
  ```sql
  SELECT view_name, now() - last_successful_finish AS age
  FROM timescaledb_information.continuous_aggregates
  ORDER BY age DESC;
  ```
- Target: max age < 60s under sustained load
- Remediate: increase policy cadence, scale DB, shrink refresh window

---

## Triage Playbooks

- High ingest p95 (>1s)
  - Tune COPY batch bytes/rows; increase API instances; verify DB CPU
- Error rate >1%
  - Inspect ingestion_errors; check dedupe collisions or validation rejects
- Realtime p95 >1.5s
  - Reduce fan‑out frequency or batch notifications; scale hub instances
- Polling backlog growth
  - Decrease poll interval; cap per‑iteration read; scale API instances

---

## Change Management

- Feature flags
  - `Telemetry:WalReplication:Enabled`
  - `Telemetry:Mqtt:Enabled`
- Config changes require rolling restart
- Keep a 30‑minute observation window post‑change

---

## Incident Response

- Severity mapping
  - S2: ingest p95 >1s for >10m or error rate >5%
  - S3: realtime p95 >3s for >10m
- Initial actions
  - Switch to polling fallback if WAL regressions
  - Reduce VU/ingest rate or enable backpressure
  - Page DB team if CAGGs stall >5m

---

## Appendix — Useful Queries

- Ingest rate (approx)
  ```sql
  SELECT date_trunc('minute', ingestion_timestamp) AS minute, count(*)
  FROM sensor_readings
  WHERE ingestion_timestamp > now() - interval '1 hour'
  GROUP BY 1
  ORDER BY 1 DESC
  LIMIT 10;
  ```
- Top subscribed streams
  - Use `/api/v1/realtime/subscriptions` and sort by subscriber count

