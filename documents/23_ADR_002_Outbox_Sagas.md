# ADR-002 — Outbox pattern & Sagas for side effects and orchestration
**Date:** 2025-09-29
## Context
Device commands, Slack/QBO/METRC integrations require reliability and idempotency across service boundaries.
## Decision
Adopt transactional outbox for all side effects; use sagas for multi-step orchestrations; idempotent acks.
## Consequences
Exactly-once semantics at the boundary; predictable retries; improved observability.
## Alternatives
Best-effort events; direct calls without queues.
## Review
Audit DLQ <0.1% weekly; 429s <1% due to adaptive throttling.
