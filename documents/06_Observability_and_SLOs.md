# Observability & SLOs

## SLO Targets
- Telemetry ingest (device→store): p95 < 1.0 s (p99 < 2.5 s)
- Realtime push (store→client): p95 < 1.5 s
- Command dispatch (enqueue→ack): p95 < 800 ms (p99 < 1.8 s)
- Task/messaging round-trip: p95 < 300 ms
- Site-day report generation: p95 < 90 s

## Signals & Tooling
- OpenTelemetry → Prometheus + Grafana + Loki + Tempo; Sentry for errors.
- Golden signals: ingest lag, queue depth, rollup freshness, command errors, controller health, replication lag, API p99, disk IOPS.
- Burn-rate alerting (fast 1h, slow 6h) per SLOs.

## Dashboards & Alerts
- Telemetry ingest freshness, command queue latency, controller health, compliance/finance queue DLQ %.
