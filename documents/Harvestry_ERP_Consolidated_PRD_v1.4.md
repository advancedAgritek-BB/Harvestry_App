# Harvestry ERP & Control - Enterprise PRD (Consolidated v1.4)

**Doc ID:** HVRY-ERP-PRD-v1.4  
**Date:** 2025-09-29  
**Owners:** Product (primary), Engineering, Compliance, SRE, Finance  
**Audience:** Engineering, Firmware, QA, DevOps, Field Ops, Finance  
**Status:** Ready for stakeholder sign-off

---

## Executive Summary
- Canonical PRD: merged legacy CanopyLogic and newer Harvestry content, scrubbed naming, and aligned on one product story that keeps the proven backbone (Timescale plus ClickHouse triggers, RLS and ABAC, DR, Slack, QBO, METRC) and the new Autosteer, Copilot, Sustainability, PdM scope.
- Release sequencing: clarified MVP to Phase 2 to Phase 3 with enablement gates so Engineering and GTM know the real launch blockers; table lives in Section 8.
- Operations focus: added Compliance and QBO runbooks with alert ownership, DLQ cadence, recon schedules, incident response, and a Cutover Wizard backed by the state exceptions matrix (Section 9).
- Intent first: moved implementation snippets (Next.js, SciChart, CSS tokens) to a companion Architecture and Design Guide while keeping acceptance criteria and SLOs here.
- Safety guardrails: tightened Autosteer policies beyond the 7 day pilot, defined rolling observation windows, automatic rollback and disable rules, and preserved closed loop promotion gates (Section 5.7).
- Finance clarity: reaffirmed QBO item level sync cadence and GL summary timing plus approval rules; Finance Admin owns JE approval with optional two person sign-off (Section 5.12).

---

## Change Log (what changed in v1.4)
- Single source of truth. Merged the earlier CanopyLogic PRD content with the newer Harvestry draft into one canonical document. All legacy "CanopyLogic" labels removed or annotated; the product is Harvestry throughout.
- Release sequencing clarified. Added an explicit MVP to Phase 2 to Phase 3 table with enablement gates and prerequisites for Copilot, Autosteer, Sustainability, and Marketplace.
- Ops and runbooks added. Compliance (METRC and BioTrack) plus QuickBooks (QBO) runbooks: alert ownership, DLQ triage cadence, reconciliation schedule, incident response, and cutover wizard.
- Implementation snippets moved. Next.js, SciChart, CSS tokens, and code samples are moved to a companion Architecture and Design Guide so the PRD remains product intent first.
- Autosteer governance tightened. Defined observation windows, rollbacks, and disable rules beyond the initial 7 day pilot.
- QBO GL summary clarified. Default frequency and approvals specified; item level versus GL summary scope re-stated.
- Training and SOP gating remediation. Added an emergency override path with two person approval and forced remediation before next shift.

---

## 1) Vision and Objectives
A modern, enterprise grade Cultivation OS that unifies ERP plus compliance plus prescriptive control: real time monitoring, safe fertigation and climate or lighting control, regulator grade traceability, and auditable financials, with site scoped security, DR, and full observability.

**Primary objectives**
- Operational excellence: deterministic blueprint tasking and safe automations.
- Data driven cultivation: real time environment or substrate telemetry, precision irrigation, crop steering (closed loop and MPC), anomaly or yield analytics.
- Compliance and auditability: COA gating, destruction controls, jurisdictional label rules, regulator ready exports.
- Financial integrity: QBO integration (item level plus GL summary) with WIP or COGS clarity.
- Enterprise NFRs: performance SLOs, DR, observability, RLS or ABAC, cost guardrails.

**Non-goals (v1)**: retail POS or e-commerce; non-QBO accounting; lab APIs beyond PDF or CSV until Phase 2.

---

## 2) Scope, Assumptions, and Locked Decisions
- Multi site orgs; site scoped permissions (RLS) with ABAC overlays for high risk actions.
- Unified location tree (Site -> Room -> Zone -> Rack -> Bin), Vault as a distinct type.
- Clones as individuals and batches; tray or cell granularity (site configurable).
- Lifecycle: rule based auto transitions with override; AI suggestions may auto apply behind flags.
- Blueprint matrix by strain x phase x room class.
- Environment targets precedence: strain -> phase -> room -> zone -> rack (alert thresholds separate).
- Returns create a new lot linked to origin and placed on HOLD.
- Templated processes with per run overrides; labor logs by user and team.
- Closed loop EC or pH and MPC Autosteer are feature flagged and promote shadow -> staged or A-B -> enable; one click revert to open loop.
- Telemetry: Postgres plus Timescale hot store with triggered ClickHouse sidecar for long horizon analytics; S3 or Parquet archive.
- Observability: OpenTelemetry, Prometheus, Grafana, Loki, Tempo; Sentry for errors; burn rate alerts.
- DR: warm cross region replica, RPO <= 5 min, RTO <= 30 min; PITR with WAL archiving.
- Alerts: Slack and email for all severities; SMS for Critical (site policy).

---

## 3) Personas (primary)
- Head of Cultivation
- Processing Manager
- Compliance Officer
- Facilities or Ops
- Finance
- Executives
- Operators or Techs

---

## 4) System Overview (capabilities)
Spatial model and equipment; genetics or propagation; lifecycle or tasking or messaging (Slack); monitoring and control (climate or substrate or irrigation, closed loop EC or pH); inventory or labels; processing; compliance (METRC or BioTrack), COA gating or holds; accounting (QBO); AI (anomaly, yield, ET0); Autosteer MPC and Copilot Ask-to-Act; sustainability; PdM; analytics or reports; enterprise NFRs (SLOs or DR or observability or security).

---

## 5) Functional Requirements (by module)

### 5.1 Identity, Access, Roles, Training, and Badge Login
Must: orgs and sites, custom roles, RLS plus ABAC, badge credentials with revocation, SOPs or training with quizzes and digital sign-off; task gating on SOP or training.  
Acceptance: blocked tasks show explicit gating reason; revoking a badge ends device sessions immediately.

### 5.2 Organization, Sites, Spatial Model, and Equipment
Must: rooms or zones or racks or bins; Vault type; equipment registry (controllers, sensors, actuators, injectors, pumps, valves, meters, EC or pH controllers, mix tanks) with calibration logs and device health.  
Acceptance: map valves to zones; calibration and faults are reportable.

### 5.3 Genetics, Phenotypes, Mothers, Cloning, and Propagation
Must: genetics and phenotypes; mother health and cut counts; clones tracked as individuals and batches; tray or cell logging; rooting percent, failure reasons, transplant events; regulator compatible reporting.  
Acceptance: tray or cell toggled per site; propagation performance reports.

### 5.4 Lifecycle, Tasking, Messaging, and Slack
Must: Clone->Veg->Flower->Harvest->Cure lifecycle with rule based auto advance and override; blueprint matrix by strain x phase x room class; tasks with dependencies, SLA, approvals (single or two person), evidence, delegation; Slack two way mirroring (or notify only), slash commands, and buttons.  
Acceptance: SLA escalations fire; Slack round trip < 2 s p95; lifecycle and delegation audited.

### 5.5 Environment Monitoring (Growlink or AROYA parity)
Must: Air or canopy (T, RH, VPD, CO2, PPFD or DLI, airflow or pressure), substrate (VWC, EC, pH, temp), drain volume or EC or pH; targets vs alert thresholds; 1 min or 5 min or 1 hour rollups with conformance views.  
Acceptance: deviations beyond thresholds generate alerts; rollups validated vs raw.

### 5.6 Irrigation and Fertigation (Programs, Recipes, Interlocks)
Must: irrigation groups; programs (time or volume, cycle or soak, windows, flush); schedules (time or sensor or hybrid); recipes (versions), stock solutions, injector channels, mix tanks; EC or pH targets and tolerances; closed loop EC or pH behind site flag with shadow then enable; safe interlocks (tank level, EC or pH bounds, CO2 exhaust lockout, curfews, e-stop); flow or pressure verification; dryback calculation; day or night targets; nutrient consumption logs and cost snapshots.  
Acceptance: program steps monitored; interlocks fail safe; closed loop promoted only after successful shadow; consumption and costs reconcile.

### 5.7 Autosteer MPC (joint irrigation plus climate plus lighting control)
Purpose: Jointly optimize irrigation (shot size or frequency or dryback or leachate percent, EC targets), climate (VPD via T or RH, CO2 setpoint or range, dehumid or heat hysteresis), and lighting (PPFD or DLI ramps).  
Inputs: sensor rollups, ET0, cultivar response curves, equipment capabilities, policy guardrails, schedule or blueprint context.  
Planner or Executor: room or zone level MPC with cost function (yield proxy, stress avoidance, resource costs); staged deltas preview -> A or B -> full with explainability; auto rollback on risk or guardrail trip.  
Enablement gates: `autosteer_mpc_enabled` site flag; promotion requires shadow >= 14 days with median correction delta <= 5 percent, then A or B block success.  
Acceptance: plan compliance >= 85 percent over 7 days at pilot; 0 guardrail breaches in enabled mode; revert <= 60 s on trip; explainability records for every applied plan.

Observation and rollback policy (post enable):  
- Continuous 28 day observation window with KPIs (compliance percent, breaches, rollbacks).  
- Auto disable Autosteer and revert to open loop if: (a) >=1 guardrail breach in enabled mode, (b) >=3 auto rollbacks within 24 h, or (c) plan compliance < 80 percent for >6 h.  
- Re-enable requires two person approval and a new 7 day A or B block without breaches.

### 5.8 Harvestry Copilot (Ask-to-Act)
Must: natural language intents produce proposed changes across EC or shot timing or DLI or CO2 or VPD with expected deltas and risk; human-in-the-loop approvals by default; staged rollout (A or B first), auto rollback on anomaly or risk; 100 percent actions are explainable; Slack interactive actions supported.  
Flags: `ask_to_act_enabled` controls visibility; auto apply tied to `ai_auto_apply_enabled` and site threshold.  
Acceptance: explanation record on 100 percent of actions; A or B on first application; rollback meets <=60 s; Slack action mirrored in <=2 s p95; audit intact.

### 5.9 Inventory, Warehouse, GS1 or UDI, and Scanning
Must: unified locations (vault or room or zone or rack or bin or truck or customer or staging or lab); lots with QA status or expiration; balances per location; movements; adjustments; returns -> child lot on HOLD; conversions or rework; UoM conversions; GS1 or UDI labels per jurisdiction.  
Acceptance: accurate balances across splits; FEFO and scans update movements; GS1 labels render correctly.

### 5.10 Processing and Manufacturing
Must: process definitions with per run overrides; WIP; yields; operator loss rates; team or user labor; waste events; rework paths.  
Acceptance: input or output reconcile with inventory; labor and waste appear in cost rollups; lineage retained on rework.

### 5.11 Compliance (METRC or BioTrack), Testing, and COA
Must: per site credentials; realtime vs scheduled sync; retry queue; event logs and replays; destruction with optional two person signoff; COA orders and results (PDF or CSV), pass or fail; HOLD or gating; label rules by jurisdiction; preview and enforcement; regulator ready audit exports.  
Acceptance: retries and rate limits handled; COA failure enforces HOLD if policy enabled; destruction logs exportable.

Operations or Runbooks (new):  
- Alert ownership: Integrations on-call owns METRC or BioTrack or QBO queue health; Compliance or Finance notified on sustained errors.  
- Reconciliation cadence: METRC or BioTrack hourly DLQ sweep plus daily at 02:00 site TZ full recon; QBO daily item level recon plus period close GL summary recon.  
- Incident response: Sev-1 = movement write failures or compliance export gaps; auto page; rollback local transactions via sagas; resume with idempotent replay.  
- Cutover wizard: staged switch with backfill, conflict resolver, and jurisdiction exceptions (see matrix below).  
- Jurisdiction exceptions matrix: per state overrides (for example escort required manifests, timing or notice windows, label phrases). Updated monthly; versioned and auditable.

### 5.12 Accounting - QuickBooks Online (QBO)
Modes: (1) Item level: POs, Bills, Inventory Adjustments, Invoices, Payments; (2) GL summary: periodic JEs for WIP->FG and COGS.  
Idempotency and resilience: Request-ID semantics, adaptive throttling, webhooks verification, queues and DLQ with recon reports.

Frequency and approvals (clarified):  
- Item level runs continuously.  
- GL summary: monthly by default (per close); optional weekly accruals (site policy) and on-demand by Finance.  
- Approvals: JE preview requires Finance Admin (two person optional). Post only if variance within threshold; otherwise rerun recon or adjust mappings.

Acceptance: receiving creates QBO Bill correctly; period close JE reconciles WIP->FG and COGS; DLQ < 0.1 percent over 7 days.

### 5.13 Notifications and Escalations
In-app and email; Slack for all severities; SMS for Critical (site policy). Subscriptions by role or site or module; quiet hours; escalation chain; SLO burn rate alerts.  
Acceptance: Critical alerts page via SMS and Slack; acknowledgements logged; escalations trigger at configured intervals.

### 5.14 AI - Anomaly and Yield; ET0 Aware Steering
Predictions (anomaly, yield) with explicit or implicit feedback; ET0 recommendations per strain or phase (Kc) -> start as recommendations, then optional auto apply behind flags.  
Acceptance: auto apply only at or above site confidence threshold; ET0 moisture trajectory RMSE within bounds before auto apply.

### 5.15 Sustainability and ESG
Compute WUE, NUE, runoff ratio, recirculation rate, kWh per gram, CO2 intensity with regional factors; per site or room or zone or batch rollups; scheduled compliance reports.  
Acceptance: weekly WUE or NUE or kWh per gram report reconciles within +/-2 percent vs meters or inventory.

### 5.16 Predictive Maintenance (PdM)
Failure or drift probabilities for pumps, injectors, valves, sensors, HVAC; create work orders above thresholds.  
Acceptance: AUC >= 0.75 (>=6 months data) and >=20 percent downtime reduction across pilot sites over 90 days.

### 5.17 Analytics and Reporting
KPIs (yield, gram per watt, throughput, labor, COGS), irrigation KPIs, environment conformance, alert MTTR, sustainability KPIs; saved reports; scheduled CSV or XLSX or PDF; materialized views or OLAP sidecar; optional read only GraphQL with persisted queries and complexity limits.  
Acceptance: scheduled exports run on cron with site TZ; dashboards meet p95 targets.

---

## 6) Non-Functional Requirements (Enterprise)

**Performance SLOs**  
- Telemetry ingest device->store p95 < 1.0 s (p99 < 2.5 s)  
- Realtime push store->client p95 < 1.5 s  
- Command dispatch enqueue->ack p95 < 800 ms (p99 < 1.8 s)  
- Task or messaging round trip p95 < 300 ms  
- Site-day report generation p95 < 90 s

**Availability and DR**  
>=99.9 percent monthly; warm cross region replica; RPO <= 5 min / RTO <= 30 min; quarterly failover drills; weekly backup restore verification.

**Data quality and accuracy**  
Unit normalization; physical range checks; clock drift correction; idempotent writes (device_id, metric, sequence or timestamp).

**Security and governance**  
RLS everywhere (site scope); ABAC for high risk actions (destruction, overrides, closed loop enable); tamper evident audit hash chain with nightly verification and WORM anchor; secrets in KMS or Vault; token rotation jobs; DPIA or PIA for PII features; least privilege IAM with just-in-time elevation.

---

## 7) Architecture and Tooling (summary)
Data stores: PostgreSQL 15 plus TimescaleDB (hypertables and continuous aggregates); ClickHouse sidecar on triggers; S3 or Parquet archive with lifecycle (hot or warm or cold, WORM optional).  
Ingest and command planes: MQTT or HTTP or SDI-12 adapters -> normalizer -> queue (NATS or Kafka) -> storage writers; realtime fan-out; outbox for side-effects (controllers, Slack, QBO, METRC) and sagas for multi-step operations; safe aborts and interlocks.  
Observability: OpenTelemetry, Prometheus, Grafana, Loki, Tempo; Sentry; burn rate alerts (1h or 6h).  
Integrations: Slack (mirror or notify only), QBO (item plus GL summary), METRC or BioTrack (rate aware workers, idempotent, DLQ plus replay).  
Feature flags (site level): `closed_loop_ecph_enabled`, `autosteer_mpc_enabled`, `ai_auto_apply_enabled`, `et0_steering_enabled`, `sms_critical_enabled`, `slack_mirror_mode`.

---

## 8) Release Sequencing and Enablement Gates

| Phase | Scope (high level) | Prerequisites and enablement gates |
| --- | --- | --- |
| MVP | Identity or RLS; spatial and equipment; genetics or propagation; batches and blueprint tasks; messaging (Slack notify only); telemetry and rollups; irrigation open loop plus interlocks; inventory and labels; processing basics; compliance queues plus COA or holds; QBO item level; reporting; observability or DR. | SLOs met in staging; DR drill passed; queues DLQ <0.5 percent over 7 days; on-call ready. |
| Phase 2 | Closed loop EC or pH (shadow->enable); dryback auto shots; Slack two way; SOP gating in production; AI yield; GL summary; sustainability dashboards; ET0 recommendations; Driver Studio; importers; MSO roll-ups; Edge Brain HA. | Closed loop: shadow >=14 days, median correction delta <=5 percent; interlocks validated; emergency stop drill; sustainability reconciliation +/-2 percent. |
| Phase 3 | Autosteer MPC (climate plus lighting live); Copilot Ask-to-Act; Vision baseline; Cultivar Cards and Recipe Marketplace; PdM at scale; lab APIs; mobile offline; BI exports. | Autosteer: A or B success (>=85 percent compliance, 0 breaches), 30+ days of telemetry; Copilot: human-in-the-loop, audit and rollback verified; Marketplace: payments and moderation policies live, legal review; PdM: >=6 months data per asset class. |

---

## 9) Operations and Runbooks (selected)

**Integrations - METRC or BioTrack**  
- Cadence: hourly DLQ sweep; daily recon 02:00 site TZ.  
- Ownership: Integrations On-call; Compliance notified on sustained errors > 2h.  
- Incident: Fail closed with HOLD; replay idempotently; provide remediation tips.  
- Exceptions matrix: state-by-state rule overrides (escort required manifests, etc.). Monthly review and versioning.

**Integrations - QuickBooks Online**  
- Cadence: continuous item level; daily recon; monthly GL summary JEs (weekly accrual option).  
- Ownership: Finance Admin approves JEs (optional two person); thresholds enforce variance.  
- Incident: throttle storms -> adaptive backoff; triage DLQ; reconcile; re-emit idempotently.

**Training or SOP gating - remediation and emergency override**  
- If a critical task is blocked (operator failed a quiz mid shift):  
  1) Assign an alternate qualified operator or  
  2) Use Emergency Override (two person approval; time boxed window; reason required).  
- Overrides create follow-up training tasks due before next shift; repeat failures escalate to manager and HR policy.

---

## 10) Acceptance Criteria (Enterprise Readiness)
1. SLOs met (7 consecutive staging days under prod-like load).
2. DR drill: promote replica <= 30 min; data loss <= 5 min.
3. Closed loop promotion: shadow >= 14 days; median correction delta <= 5 percent; safe revert path.
4. Audit chain: nightly verify; WORM anchor stored; weekly spot checks pass.
5. Compliance or QBO queues: DLQ < 0.1 percent over 7 days; 429s < 1 percent with adaptive throttling.
6. Sustainability: weekly WUE or NUE or kWh per gram within +/-2 percent.
7. PdM: AUC >= 0.75 and >=20 percent downtime reduction across 3 pilot sites over 90 days.
8. Security: RLS fuzz tests pass; token rotation verified; SAST clean.
9. On-call: burn rate alerts and escalations reach duty engineer; runbooks executed.

---

## 11) Appendices

### A) Feature Flags (site scope)
`closed_loop_ecph_enabled`, `autosteer_mpc_enabled`, `ai_auto_apply_enabled`, `et0_steering_enabled`, `sms_critical_enabled`, `slack_mirror_mode`.

### B) Go or No-Go Gates
- Pilot Go-Live: RLS verified; telemetry stable; open loop irrigation stable; Compliance or QBO DLQ <0.5 percent (staging); DR drill passed; on-call ready.
- Closed Loop Enablement (site): shadow >=14 days, median correction delta <=5 percent; interlocks tested; emergency stop drill.
- GA: MVP criteria plus advanced flags proven safe (at least in recommendation mode); cost guardrails set; support runbooks validated.

### C) Data Model (deltas, high level)
Additions for Autosteer (`autosteer_plans`, `autosteer_actions`, `autosteer_explanations`, `ab_blocks`, `policy_guardrails`), Copilot (`ask_intents`, `ask_action_plans`, `ask_action_steps`, `approvals`, `staged_rollouts`, `rollbacks`, `risk_signals`, `copilot_feedback`), Cultivar or Marketplace (`cultivar_cards`, `cultivar_response_curves`, `recipe_blueprints`, `recipe_versions`, `marketplace_offers`, `marketplace_transactions`, `payouts`, `ratings`, `moderation_events`, `eulas`). All rows site scoped with audit chain.

### D) Responses to Open Questions (from feedback)
1. Branding: Retitle and scrub to Harvestry. Legacy "CanopyLogic" appears only in change history for traceability.
2. QBO GL summary frequency and approvals: Monthly per close (default), optional weekly accruals and on-demand runs (site policy). Finance Admin approves JE (two person optional); post only within variance thresholds; reconcile report attached.
3. Autosteer beyond the first 7 day pilot: Maintain a rolling 28 day observation window; 0 breaches accepted in enabled mode. Auto rollback on any breach; auto disable after 1 breach or 3 rollbacks per 24h; require fresh 7 day A or B block and two person approval to re-enable. Promotion pre-reqs remain shadow >=14 days, median correction delta <=5 percent, A or B pass.
4. State onboarding for METRC or BioTrack nuances: Maintain a state-by-state exceptions matrix (escort required manifests, label text, timing windows). Embed into the Cutover Wizard with policy previews, soft checks, and remediation guidance. Monthly compliance review; versioned changes.
5. Training or SOP gating remediation: Provide an Emergency Override (two person approval, time boxed) plus alternate operator routing. All overrides auto create remediation tasks due before next shift; repeated failures escalate.

### E) Out-of-Doc Artifacts
A separate Architecture and Design Guide (Next.js or SciChart patterns, tokens, client-only hydration, performance budgets) accompanies this PRD to keep product intent front and center.

---

## Hardware Safety Alignment (Reference)
Controller or edge guardrails remain unchanged: PoE+ for logic only, loads are 24 VAC, pumps via contactors, full galvanic isolation, E-STOP or door interlocks with fail safe OFF, OTA or zero touch enrollment, per hardware specs.
