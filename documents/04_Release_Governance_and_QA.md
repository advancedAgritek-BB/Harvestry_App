# Release Governance & Quality Gates

## Definitions

- **Definition of Ready (DoR):** Acceptance tests drafted; feature flags declared; RLS/ABAC & audit impacts noted; observability plan in place.
- **Definition of Done (DoD):** Code + tests merged; migrations applied; dashboards/alerts updated; runbooks & user docs updated; staging demo meets SLOs.

## CI Gates (enforced)

- Static analysis (TS/Go/SQL), SAST/secret scan.
- Unit, integration & contract tests (Slack, QBO, METRC/BioTrack).
- E2E tests; property-based tests for UoM/dosing; hardware-in-the-loop irrigation sims.
- Chaos drills for telemetry & controllers.
- Zero-downtime migrations; blue/green deploys; canary drivers.
- Release gating follows FRP-00.1 matrix (heavy vs light); evidence lives in docs/ops/runbooks/release_gating_sro.md prior to production cutovers.

## Feature Flags (site scope)

- `closed_loop_ecph_enabled`, `ai_auto_apply_enabled`, `et0_steering_enabled`, `sms_critical_enabled`, `slack_mirror_mode` (mirror|notify_only).
