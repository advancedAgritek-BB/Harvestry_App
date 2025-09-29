# Release Checklist (Blue/Green + Flags)

- [ ] Migrations expand/backfill complete; flip guarded by feature flag.
- [ ] CI: unit/contract/E2E/HIL/chaos all green.
- [ ] Observability: dashboards updated; alerts tuned.
- [ ] Docs: acceptance + runbooks updated; user docs refreshed.
- [ ] Canary: 5% → 25% → 50% → 100% with SLOs holding.
- [ ] Rollback plan rehearsed; owners on-call.
