# Harvestry ERP — **Sequential Build Backlog** (Epics → Stories)

**Version:** 2025-09-29  
**Owner:** Product / Engineering  
**Scope:** Full application (Foundations → MVP → Phase 2 → Phase 3) with enterprise NFRs.  
**Traceability:** Epics/stories map to the CanopyLogic/Harvestry PRD (see Harvestry_ERP_Consolidated_PRD_v1.4.md) and Edge Hardware specs v3.3.   

> **Legend:**  
> Tags — `[MVP]` must-ship for first pilot; `[P2]` Phase‑2; `[P3]` Phase‑3; `[ENT]` enterprise/NFR; `[HW]` edge/hardware; `[FLAG]` shipped behind feature flag.  
> DoD — Definition of Done. DoR — Definition of Ready.

---

## Milestones (sequential)
1. **M0 Foundations** — Repo/CI/CD, Identity & Sites, Spatial Model, Telemetry Ingest.  
2. **M1 Core Ops** — Tasks/Messaging, Lifecycle/Blueprints, Irrigation (open‑loop), Inventory, Processing.  
3. **M2 Compliance & Finance** — METRC/BioTrack framework + COA, QBO item‑level, Observability/DR, Security.  
4. **M3 Pilot Go‑Live (MVP)** — Production pilot with open‑loop irrigation, Slack notify‑only, reporting, SLOs met.  
5. **M4 Advanced Control & UX** — Slack 2‑way, Closed‑loop (shadow→enable) `[FLAG]`, ET₀ recommendations `[FLAG]`, Sustainability dashboards.  
6. **M5 AI & Scale** — Anomaly/Yield, PdM, GL Summary, advanced labels/SOP gating, ClickHouse triggers as needed.  
7. **M6 GA Hardening** — Cost guardrails, docs, runbooks, support readiness.

---

## Backlog (ordered by **sequential importance**)

### EPIC FND‑00 — Platform Bootstrapping & CI/CD `[MVP][ENT]`
**Goal:** Enable trunk‑based delivery with security scans, environments, feature flags, and observability baseline.  
**Stories**
- FND‑00‑S1: Create repos, branch protections, CI pipelines (lint/tests/scan), container registry.  
- FND‑00‑S2: Provision Postgres 15 + Timescale, staging env, PITR enabled.  
- FND‑00‑S3: Install OpenTelemetry SDKs; stand up Prometheus, Grafana, Loki, Tempo; Sentry project.  
- FND‑00‑S4: Secrets via KMS/Vault; seed Unleash flags (site‑scope).  
- FND‑00‑S5: Slack app (notify‑only) & QBO sandbox tenants configured.  
**DoD:** CI runs on PR; auto‑deploy to staging; health dashboards green.  
**Traceability:** PRD §11, §8, §10, §9. 

---

### EPIC ID‑01 — Identity, Access, Roles, Training & Badge Login `[MVP]`
**Goal:** Multi‑tenant orgs & site‑scoped security (RLS) with ABAC overlays and training gates.  
**Stories**
- ID‑01‑S1: Migrations for organizations, sites, users, user_sites, user_roles.  
- ID‑01‑S2: RLS policies + current_user_permissions views; ABAC hooks for high‑risk actions.  
- ID‑01‑S3: Badge credentials, device sessions; revocation flow.  
- ID‑01‑S4: SOPs/training (assignments, quizzes, digital sign‑off).  
- ID‑01‑S5: Admin APIs/UI to invite users and assign to sites/roles.  
**DoD:** Cross‑site access blocked; gated tasks blocked with explicit reason.  
**Traceability:** PRD §5.1, §7 (Security). 

---

### EPIC SPAT‑02 — Organization, Spatial Model & Equipment Registry `[MVP]`
**Goal:** Model sites→rooms→zones→racks→bins and register equipment.  
**Stories**
- SPAT‑02‑S1: Migrations for rooms, zones, racks, bins, inventory_locations (vault type).  
- SPAT‑02‑S2: Equipment registry, links, calibration logs, device health.  
- SPAT‑02‑S3: Map valves to zones; seed reason codes.  
**DoD:** Device heartbeat and calibration visible; RLS applied.  
**Traceability:** PRD §5.2. 

---

### EPIC TEL‑03 — Telemetry Ingest & Rollups `[MVP]`
**Goal:** Ingest climate/substrate streams with 1m/5m/1h rollups; realtime fan‑out.  
**Stories**
- TEL‑03‑S1: sensor_streams/readings hypertables; continuous aggregates.  
- TEL‑03‑S2: MQTT/HTTP/SDI‑12 adapters; normalization, unit coercion, idempotency.  
- TEL‑03‑S3: Realtime push (WS/SSE) and Grafana starter dashboards.  
- TEL‑03‑S4: Alert scaffolding; device offline/fault detection.  
**DoD:** p95 device→store < 1.0s; rollups freshness < 60s under staging load.  
**Traceability:** PRD §5.5, §6.1–6.3, §8. 

---

### EPIC WFL‑04 — Tasks, Messaging & Slack (notify‑only) `[MVP]`
**Goal:** Core task engine, conversations, Slack notifications.  
**Stories**
- WFL‑04‑S1: tasks/* tables; watchers, dependencies, SLA scaffolding.  
- WFL‑04‑S2: conversations/messages; attachments; mentions.  
- WFL‑04‑S3: Slack notify‑only bridge (outbox pattern).  
**DoD:** Task events notify Slack p95 < 2s; blocked reasons explicit.  
**Traceability:** PRD §5.4, §9 (Slack). 

---

### EPIC LIFECYC‑05 — Batch Lifecycle, Blueprints & Movements `[MVP]`
**Goal:** Rule‑based Clone→Veg→Flower→Harvest→Cure with blueprint‑generated task sets.  
**Stories**
- LIFECYC‑05‑S1: batches (phase, dates), task_blueprints (strain×phase×room class).  
- LIFECYC‑05‑S2: Auto‑generate tasks on phase enter; plant_movements with from/to required.  
- LIFECYC‑05‑S3: Observations on batch/task/room.  
**DoD:** Auto‑phase change generates next tasks; all audited.  
**Traceability:** PRD §5.3–5.4. 

---

### EPIC CTRL‑06 — Irrigation Orchestrator (Open‑Loop) & Interlocks `[MVP]`
**Goal:** Groups/programs/schedules, run orchestrator, interlocks & safe aborts.  
**Stories**
- CTRL‑06‑S1: irrigation_groups/programs/schedules, runs/step_runs.  
- CTRL‑06‑S2: Command queue + idempotency; manual override request+approval.  
- CTRL‑06‑S3: Interlocks (tank level, EC/pH bounds, CO₂ exhaust lockout, max runtime).  
**DoD:** enqueue→ack p95 < 800ms; abort closes valves safely; audit trail present.  
**Traceability:** PRD §5.6, §10 (APIs & services). 

---

### EPIC INV‑07 — Inventory, Warehouse, GS1/UDI & Scanning `[MVP]`
**Goal:** Unified locations tree; lots/balances/movements; returns→HOLD; GS1/labels.  
**Stories**
- INV‑07‑S1: inventory_lots/balances/movements/adjustments; lot_relationships.  
- INV‑07‑S2: UoM definitions & conversions; barcode settings; label templates.  
- INV‑07‑S3: Movements APIs; returns create child lot on HOLD; GS1 label render.  
**DoD:** Balances reconcile after split/merge/move; labels meet GS1 when configured.  
**Traceability:** PRD §5.7. 

---

### EPIC PROC‑08 — Processing & Manufacturing `[MVP]`
**Goal:** Process definitions, WIP/yields, labor & waste capture.  
**Stories**
- PROC‑08‑S1: process_definitions/runs/steps; labor_logs; waste_events.  
- PROC‑08‑S2: Inventory hooks for input/output transformations.  
**DoD:** Yields reconcile with inventory; labor & waste appear in costs.  
**Traceability:** PRD §5.8. 

---

### EPIC CMP‑09 — Compliance Framework & COA (PDF/CSV) `[MVP]`
**Goal:** METRC/BioTrack sync framework; queues/retries; COA ingestion and gating.  
**Stories**
- CMP‑09‑S1: compliance_integrations; sync_queue/events; workers with backoff.  
- CMP‑09‑S2: labs/lab_orders/lab_results ingestion; holds & gating policies per site.  
- CMP‑09‑S3: Exportable audit trails; error surfacing & remediation.  
**DoD:** Retry/backoff verified; failing COA → HOLD & task; regulator export stub.  
**Traceability:** PRD §5.9, §9 (Integrations). 

---

### EPIC QBO‑10 — QuickBooks Online (Item‑Level) `[MVP]`
**Goal:** OAuth2, mapping tables, item‑level POs/Bills/Invoices, idempotency & throttling.  
**Stories**
- QBO‑10‑S1: accounting_integrations; qbo_*_map; accounting_queue/events.  
- QBO‑10‑S2: POs→Receiving→Bills; Invoices & Payments; Request‑ID idempotency.  
- QBO‑10‑S3: Adaptive concurrency; DLQ + reconciliation report.  
**DoD:** Receive PO → QBO Bill; DLQ < 0.5% in staging load.  
**Traceability:** PRD §5.14, §9 (QBO). 

---

### EPIC SRE‑11 — Observability, SLOs, DR Drill & Security Hardening `[MVP][ENT]`
**Goal:** Golden signals, burn‑rate alerts, DR drill; token rotation; RLS fuzz tests.  
**Stories**
- SRE‑11‑S1: Dashboards — ingest lag, queue depth, rollup freshness, API p95/99, command errors, replication lag.  
- SRE‑11‑S2: Alert rules w/ Slack+Email (+SMS Critical by site policy).  
- SRE‑11‑S3: DR runbook; warm cross‑region replica; execute failover drill (RPO ≤5m, RTO ≤30m).  
- SRE‑11‑S4: Security — token rotation, SAST clean, RLS fuzz tests.  
**DoD:** Burn‑rate alerts verified; failover ≤30m; SAST clean; secrets rotated.  
**Traceability:** PRD §7, §8, §12; Notifications §5.15. 

---

### EPIC PILOT‑12 — Pilot Readiness & UAT `[MVP]`
**Goal:** UAT with pilot site; training; cutover; on‑call & support ready.  
**Stories**
- PILOT‑12‑S1: Seed data; training materials; operator manuals.  
- PILOT‑12‑S2: Go/No‑Go checklist; cutover plan; support rotation.  
- PILOT‑12‑S3: MVP production for pilot; incident playbooks tested.  
**DoD:** Pilot site live; acceptance sign‑off; incident playbooks validated.  
**Traceability:** PRD §13 (Roadmap) & §12 (Acceptance). 

---

### EPIC SLACK‑13 — Slack Two‑Way Mirroring & Task Actions `[P2]`
**Goal:** Full Slack bridge with edits/deletes; interactive task actions.  
**Stories**
- SLACK‑13‑S1: message_bridge_log mapping; /cl slash commands.  
- SLACK‑13‑S2: Interactive buttons (Start/Complete/Approve/Log Time).  
- SLACK‑13‑S3: Outage reconciliation worker (idempotent).  
**DoD:** p95 round‑trip < 2s; no dupes after Slack outage.  
**Traceability:** PRD §5.4, §9 (Slack). 

---

### EPIC LOOP‑14 — Closed‑Loop EC/pH (Shadow → Enable) `[P2][FLAG]`
**Goal:** Run closed‑loop in shadow, then enable per‑site after promotion checklist.  
**Stories**
- LOOP‑14‑S1: control_loops config; EC/pH controller drivers; correction deltas compute.  
- LOOP‑14‑S2: Shadow mode UI; promotion checklist; interlocks validation.  
- LOOP‑14‑S3: Site feature flag; enable/disable with audit; emergency stop drill.  
**DoD:** Median correction delta ≤5% over 14 days before enable; safe revert path proven.  
**Traceability:** PRD §5.6, §12 (3); Edge controller power & safety constraints.  

---

### EPIC ET0‑15 — ET₀‑Aware Irrigation Recommendations & Sustainability Dashboards `[P2][FLAG]`
**Goal:** ET₀ computation with Kc profiles; recommendation mode; WUE/NUE/kWh/CO₂ dashboards.  
**Stories**
- ET0‑15‑S1: crop_coefficients, et0_inputs; daily ET₀ calc job.  
- ET0‑15‑S2: Recommendation panel; moisture trajectory visual; policy windows.  
- ET0‑15‑S3: Energy meters/readings; emissions factors; weekly sustainability report.  
**DoD:** Sustainability reconciliation error < ±2%; ET₀ recs plotted vs trajectory.  
**Traceability:** PRD §5.10–5.11, §13 (Phase 2). 

---

### EPIC AI‑16 — AI v1 (Anomaly & Yield) + Feedback `[P2][FLAG]`
**Goal:** Predictions with confidence; feedback loops; optional auto‑apply per site.  
**Stories**
- AI‑16‑S1: ai_predictions/ai_feedback/feature_logs pipelines.  
- AI‑16‑S2: Model training+scoring jobs; acceptance thresholds; UI surfacing.  
- AI‑16‑S3: Auto‑apply gating via feature flag; audit trail.  
**DoD:** Baseline precision met; no auto‑apply below threshold; feedback captured.  
**Traceability:** PRD §5.10 (AI), §13. 

---

### EPIC PDM‑17 — Predictive Maintenance (PdM) Baseline `[P3]`
**Goal:** Failure/drift prediction with work orders; downtime reduction.  
**Stories**
- PDM‑17‑S1: equipment_usage_counters/failures/maintenance_predictions/work_orders.  
- PDM‑17‑S2: Feature engineering (cycles, hours, faults, drift).  
- PDM‑17‑S3: Auto‑create work orders above threshold.  
**DoD:** AUC ≥0.75 on historical validation; early pilot signals fewer emergency repairs.  
**Traceability:** PRD §5.10 (PdM), §13. 

---

### EPIC FIN‑18 — GL‑Summary Journal Entries `[P2]`
**Goal:** Periodic JEs for WIP→FG→COGS; run alongside item‑level.  
**Stories**
- FIN‑18‑S1: GL summary mode with mapping; JE generation & reconciliation.  
- FIN‑18‑S2: Variance thresholds & reports.  
**DoD:** Month‑close JE matches variance bounds; runs with item‑level mode.  
**Traceability:** PRD §5.14. 

---

### EPIC CMP‑19 — Advanced Compliance (Labels, Two‑Person Destruction, SOP Gating in Prod) `[P2]`
**Goal:** Jurisdiction label engine; two‑person destruction; enforce SOP gating.  
**Stories**
- CMP‑19‑S1: Label rules per jurisdiction; preview & enforcement.  
- CMP‑19‑S2: Two‑person destruction sign‑off; audit exports.  
- CMP‑19‑S3: Enforce SOP/training gating in production.  
**DoD:** Labels render per jurisdiction rules; destruction requires dual sign‑off; tasks blocked when training missing.  
**Traceability:** PRD §5.9, §5.15, §13. 

---

### EPIC DATA‑20 — Analytics, Reporting & (Optional) Read‑Only GraphQL `[MVP→P3]`
**Goal:** KPIs/dashboards; saved reports; scheduled CSV/XLSX/PDF; optional GraphQL read layer.  
**Stories**
- DATA‑20‑S1: Saved reports & email schedules; materialized views. `[MVP]`  
- DATA‑20‑S2: KPI dashboards (yield, gram/watt, irrigation, alert MTTR, sustainability). `[MVP→P2]`  
- DATA‑20‑S3: GraphQL read layer with persisted queries & complexity limits. `[P3]`  
**DoD:** Scheduled exports deliver on cron (site TZ) and meet p95 targets.  
**Traceability:** PRD §5.12, §9 (Optional GraphQL). 

---

### EPIC SCALE‑21 — ClickHouse Sidecar & Retention Controller `[P2/P3][ENT]`
**Goal:** Enable OLAP sidecar when triggers hit; dynamic raw retention.  
**Stories**
- SCALE‑21‑S1: ClickHouse PoC & triggers (ingest & query thresholds).  
- SCALE‑21‑S2: Retention controller to tighten raw retention under heavy ingest.  
**DoD:** Trigger doc & switch plan; retention auto‑adjusts under surge.  
**Traceability:** PRD §6.1 (Hot/OLAP/Archive), triggers list. 

---

### EPIC B2B‑22 — Sales, Fulfillment & B2B (Optional) `[Opt]`
**Goal:** POs/receiving; wholesale orders → pick/pack/ship → manifest → invoice.  
**Stories**
- B2B‑22‑S1: Partner master (customer/vendor/lab/carrier).  
- B2B‑22‑S2: FEFO allocation; compliant manifests & labels; invoice to QBO.  
**DoD:** End‑to‑end demo with FEFO & QBO invoice.  
**Traceability:** PRD §5.13. 

---

## Parallel (Edge) — Hardware‑Aware Control Readiness (run alongside software epics)
> For sites using Harvestry Edge hardware, align with v3.3 requirements and manufacturing SOW. `[HW]`  

- HW‑A: **Device drivers & command plane** — TrolMaster/Agrowtek first; SDI‑12 adapter; MQTT/HTTP/Modbus; emergency stop & interlocks; offline‑first behavior. `[MVP]` (PRD §9; Edge v3.3 §§3–5, 13)  
- HW‑B: **Dual‑source power & safety FSM** — PoE+ logic only; 24 VAC loads; ESTOP/door hard‑gated; no mains on PCB (contactors only). `[MVP→P2]` (Edge v3.3 §§3–4, 12)  
- HW‑C: **Factory test & acceptance** — PoE Class 4; isolation; concurrency caps (INT‑100/150 VA); 72h soak; MFG‑TEST image & logs returned. `[Pilot]` (Mfg Req §§8–9)  
- HW‑D: **Closed‑loop enablement** — Shadow→enable per site; promotion checklist enforced. `[P2][FLAG]` (PRD §12.3)  

---

## Governance (apply to every epic)
- **DoR:** Acceptance tests (PRD language); flags declared; RLS/ABAC & audit impacts; observability plan.  
- **DoD:** Code + tests merged; migrations applied; dashboards/alerts updated; runbooks & user docs updated; staging demo meets SLOs for 7 consecutive days under prod‑like load. *(Scope gates can be tuned by change risk during iteration planning.)*  
- **SLO Targets:** Ingest p95 < 1.0s; store→client p95 < 1.5s; command enqueue→ack p95 < 800ms; task/messaging p95 < 300ms; site‑day report p95 < 90s.  
**Traceability:** PRD §11 (CI/CD & Quality), §8 (SLOs), §12 (Acceptance). 

---

## Notes on Sequencing
- **Foundations first** (ID/SPAT/TEL) unblock all downstream modules.  
- **Workflow + Irrigation (open‑loop)** precede Inventory/Processing to deliver immediate operator value.  
- **Compliance/QBO & Observability/DR** complete the MVP envelope for a regulated pilot.  
- **Advanced control (Slack 2‑way, Closed‑loop) & AI/ET₀** build on stable telemetry and orchestrator.  
- **Scale concerns (ClickHouse)** only when triggers trip.  
- **B2B Sales** is optional and should not block Core Ops.  
**Traceability:** PRD §13 (Roadmap & Phased Rollout). 
