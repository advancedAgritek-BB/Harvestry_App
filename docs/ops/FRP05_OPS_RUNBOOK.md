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

## Load Testing & SLO Validation

### Running k6 Load Tests

**Prerequisites:**
- k6 installed (`brew install k6` or see https://k6.io/docs/get-started/installation/)
- Target environment running and healthy
- Valid API token for authentication

**Baseline Test (2-minute warm-up):**
```bash
BASE_URL=https://staging.example.com \
SITE_ID=<your-site-guid> \
EQUIPMENT_ID=<your-equipment-guid> \
API_TOKEN=<your-token> \
./scripts/load/run-k6-baseline.sh
```

**Sustained Load Test (15-minute gate):**
```bash
BASE_URL=https://staging.example.com \
SITE_ID=<your-site-guid> \
EQUIPMENT_ID=<your-equipment-guid> \
API_TOKEN=<your-token> \
VUS=800 \
DURATION=15m \
./scripts/load/run-k6-sustained.sh
```

### SLO Targets

| Metric | Target | Measurement |
|--------|--------|-------------|
| Ingest p95 | < 1.0s | k6 `http_req_duration{endpoint:ingest}` |
| Ingest p99 | < 2.5s | k6 `http_req_duration{endpoint:ingest}` |
| Error rate | < 1% | k6 `http_req_failed` |
| Rollup freshness | < 60s | SQL query on `timescaledb_information.continuous_aggregates` |
| Real-time push p95 | < 1.5s | SignalR client latency measurement |
| Sustained throughput | 10k msg/s | k6 with 800 VUs, 15 min duration |

### Validating SLOs

After load test completion:

1. **Review k6 summary output:**
   ```bash
   ./scripts/load/summarize-k6-results.sh --latest
   ```

2. **Check rollup freshness:**
   ```sql
   SELECT view_name, 
          now() - last_successful_finish AS age,
          CASE WHEN now() - last_successful_finish < interval '60 seconds' 
               THEN 'PASS' ELSE 'FAIL' END AS status
   FROM timescaledb_information.continuous_aggregates
   ORDER BY age DESC;
   ```

3. **Check ingestion error rate:**
   ```sql
   SELECT 
     date_trunc('minute', created_at) AS minute,
     COUNT(*) AS errors,
     COUNT(*) * 100.0 / NULLIF((
       SELECT COUNT(*) FROM sensor_readings 
       WHERE ingestion_timestamp > now() - interval '15 minutes'
     ), 0) AS error_pct
   FROM ingestion_errors
   WHERE created_at > now() - interval '15 minutes'
   GROUP BY 1
   ORDER BY 1 DESC;
   ```

4. **Check subscription health:**
   ```bash
   curl -s https://staging.example.com/api/v1/realtime/subscriptions | jq .
   ```

### Load Test Results Archive

Results are stored in `logs/k6-sustained-{timestamp}.json`. Archive important runs:
```bash
cp logs/k6-sustained-*.json ./load-test-archive/
```

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

- P95 ingest latency (last hour):
  ```sql
  SELECT 
    percentile_cont(0.95) WITHIN GROUP (ORDER BY 
      EXTRACT(EPOCH FROM (ingestion_timestamp - recorded_at)) * 1000
    ) AS p95_ms
  FROM sensor_readings
  WHERE ingestion_timestamp > now() - interval '1 hour'
    AND recorded_at IS NOT NULL;
  ```

- Top subscribed streams
  - Use `/api/v1/realtime/subscriptions` and sort by subscriber count

- Stream health check:
  ```sql
  SELECT 
    ss.name,
    ss.stream_type,
    COUNT(sr.*) AS reading_count_1h,
    MAX(sr.recorded_at) AS last_reading
  FROM sensor_streams ss
  LEFT JOIN sensor_readings sr ON sr.stream_id = ss.id 
    AND sr.recorded_at > now() - interval '1 hour'
  GROUP BY ss.id, ss.name, ss.stream_type
  ORDER BY reading_count_1h DESC
  LIMIT 20;
  ```

---

## Sign-Off Checklist

Before marking FRP-05 complete, verify:

- [ ] k6 sustained load test passes all thresholds
- [ ] Rollup freshness < 60s during sustained load
- [ ] No error rate spikes above 1%
- [ ] Real-time subscription endpoint healthy
- [ ] All integration tests passing
- [ ] Runbook reviewed by SRE team

