# RACI & Squad Structure

## Squads
1) **Core Platform** — Auth/RLS/Orgs/Sites/Inventory/Processing
2) **Telemetry & Controls** — Sensors, Rollups, Irrigation/Fertigation, Interlocks
3) **Workflow & Messaging** — Lifecycle, Tasks, Slack bridge
4) **Integrations** — Compliance (METRC/BioTrack), QBO, Labeling
5) **Data & AI** — Timescale/ClickHouse, Analytics, AI (Anomaly/Yield, ET₀), Sustainability
6) **DevOps/SRE/Security** — Observability, CI/CD, DR, Security

## RACI (excerpts)
| Area | R | A | C | I |
| --- | --- | --- | --- | --- |
| Identity/RLS/ABAC | Core Platform Eng Lead | VP Product | Security, SRE | Execs |
| Telemetry ingest & rollups | Telemetry & Controls Eng Lead | Chief Architect | SRE | Execs |
| Irrigation orchestration & interlocks | Telemetry & Controls | VP Product | Firmware/EE, SRE | Field Ops |
| Slack bridge | Workflow & Messaging | VP Product | Security | Execs |
| Compliance (METRC/BioTrack) | Integrations | VP Product | Compliance Officer | Finance |
| QBO (item-level & GL-summary) | Integrations | VP Product | Finance | Execs |
| Closed-loop enablement (per site) | Telemetry & Controls | VP Product | SRE, Field Ops | Execs |
| DR drills | SRE | VP Product | Platform, Security | Execs |
