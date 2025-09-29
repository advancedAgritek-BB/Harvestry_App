# Harvestry ERP & Control — Project Charter
**Date:** 2025-09-29  •  **Owner:** VP Product  •  **Version:** 1.0

## Purpose
Deliver an enterprise-grade, automation-first Cultivation OS that unifies ERP (seed-to-sale),
facility monitoring & fertigation control (open + closed-loop), compliance (METRC/BioTrack),
and financial integrity (QuickBooks Online), with site-scoped security and real-time performance.

## Goals (outcome-oriented)
- **Operational excellence:** deterministic tasking, safe automations, and rapid exception handling.
- **Data-driven cultivation:** precise telemetry, irrigation programs, and crop steering with guardrails.
- **Compliance & auditability:** regulator-ready traceability, COA gating, destruction controls, label rules.
- **Financial integrity:** QBO item-level & GL-summary pathways, reconciliation reports.
- **Enterprise non-functionals:** performance SLOs, DR, observability, RLS/ABAC security, cost guardrails.

## Non-Goals (v1)
- Retail POS/e-commerce; lab API ingestion beyond PDF/CSV until Phase 2.

## Success Metrics (representative)
- SLOs hold for staging/prod (ingest p95<1.0s; realtime push p95<1.5s; command enqueue→ack p95<800ms).
- Pilot sites: irrigation on-time ≥95%; planning time ↓≥35%; error rate ↓≥30%; COA gating blocks non-compliant movement.
- DR drill: RPO ≤5m, RTO ≤30m; security scans clean; <0.1% DLQ on compliance/accounting queues (7d).

## Scope & Phasing
- **MVP (Foundations + Core Ops/Compliance):** Identity/RLS, Sites & Spatial, Genetics/Cloning, Batch lifecycle & tasks (Slack notify-only), Telemetry + rollups, Irrigation (open-loop), Inventory & labels, Processing basics, Compliance queues, COA ingestion & holds, QBO item-level, Reporting, Observability.
- **Phase 2:** Closed-loop EC/pH (flagged + shadow), interlocks & dryback auto-shots, Slack two-way, AI Yield, GL-summary JE, sustainability dashboards, two-person destruction.
- **Phase 3:** Lab APIs, additional controllers, PdM at scale, BI exports, mobile offline, GraphQL read layer.

## Stakeholders & RACI (high level)
- **Accountable:** CEO/Head of Product, VP Product
- **Responsible:** PMs by module, Eng Leads, Firmware/EE, SRE/Security
- **Consulted:** Compliance Officer, Finance, Field Ops/Installers
- **Informed:** Executives, Design Partners

## Risks (snapshot)
- Device heterogeneity; regulatory variance; integration throttling; telemetry surge; closed-loop safety.
See `03_Risk_Register.csv` for owners and mitigations.

## Milestones
- Sprint 0–3 Foundations; Sprint 4–8 Workflow/Inventory/Processing; Sprint 9–12 Compliance/Accounting/Observability + Pilot; Sprint 13–18 Advanced features & GA readiness.

## Governance
- Feature flags for high-risk features (closed loop, AI auto-apply, ET₀ steering, Critical SMS).
- DoR/DoD enforced in CI; all releases blue/green with rollback plan.
