# ADR-003 — Site-scoped feature flags with Unleash
**Date:** 2025-09-29
## Context
High-risk features (closed-loop, AI auto-apply, Critical SMS) must roll out safely per site.
## Decision
Use Unleash for site-scoped flags; tie promotion to checklists & metrics.
## Consequences
Granular rollouts; fast rollback; auditable toggles.
## Alternatives
Env-based toggles; custom flag system.
## Review
Quarterly audit of flag usage & promotion outcomes.
