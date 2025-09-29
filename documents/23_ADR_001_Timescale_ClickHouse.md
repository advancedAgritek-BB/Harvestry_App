# ADR-001 — TimescaleDB hot store with ClickHouse sidecar on triggers
**Date:** 2025-09-29
## Context
High-rate telemetry needs hot relational + time-series with low-latency aggregates; long-horizon analytics require columnar OLAP.
## Decision
Use PostgreSQL 15 + TimescaleDB for hot store; enable ClickHouse sidecar when ingest/query triggers trip.
## Consequences
Great OLTP/real-time performance; scalable analytics when needed; cost guardrails via triggers.
## Alternatives
Plain Postgres partitions; fully-managed time-series DB; data lake only.
## Review
Revisit when ingest >200k metrics/min sustained or multi-site 180-day queries exceed p95 300 ms under load.
