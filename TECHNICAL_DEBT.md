# Technical Debt Register

**Last Updated**: 2025-09-29  
**Review Frequency**: End of each sprint / After major feature completion

## 🎯 Purpose

This register tracks technical debt items that were intentionally deferred or discovered during development. Each item should be reviewed and prioritized for future sprints.

**Debt Principle**: When adding debt, always ask:
- Why did we take this shortcut?
- What's the impact if we don't fix it?
- When should we address it?

---

## 📊 Debt Summary

| Priority | Count | Est. Effort |
|----------|-------|-------------|
| 🔴 High | 2 | 5 days |
| 🟡 Medium | 5 | 10 days |
| 🟢 Low | 0 | 0 days |
| **Total** | **7** | **15 days** |

---

## 🔴 High Priority Debt

### DEBT-001: OpenTelemetry Instrumentation Missing
**Category**: Observability  
**Created**: 2025-09-29  
**Squad**: DevOps-Squad  
**Effort**: 3 days  

**Context**:
Track A completed all observability infrastructure (Prometheus, Grafana, Loki, Tempo, OTel Collector) but actual service instrumentation is pending.

**Why Deferred**:
- Requires actual .NET microservices to be implemented first
- Infrastructure was prioritized to be ready when services are built

**Impact if Not Fixed**:
- No distributed tracing across services
- Limited performance insights
- Difficult to debug cross-service issues

**Action Required**:
1. Add `OpenTelemetry.Extensions.Hosting` NuGet package to each service
2. Configure OTel SDK in `Program.cs` with OTLP exporter
3. Add custom spans for critical operations
4. Verify traces appear in Jaeger UI

**Code Locations**:
- `src/backend/services/*/Program.cs` (instrumentation setup)
- `src/infrastructure/observability/otel-collector-config.yml` (already configured)

**Acceptance Criteria**:
- [ ] All services export traces to OTel Collector
- [ ] Service map visible in Jaeger
- [ ] Custom spans for business operations (>95% coverage)
- [ ] p95 latency metrics accurate in Grafana

**Related**:
- Track A Item: track-a-15
- Documentation: `src/infrastructure/observability/README.md`

---

### DEBT-002: WAL-Fanout Service Implementation
**Category**: Real-time Features  
**Created**: 2025-09-29  
**Squad**: Telemetry-Controls-Squad  
**Effort**: 2 days  

**Context**:
Real-time push architecture designed (WAL logical decoding → WebSocket/SSE), but service implementation incomplete.

**Why Deferred**:
- Database infrastructure (hypertables, outbox, RLS) prioritized first
- Service requires PostgreSQL logical replication slot setup

**Impact if Not Fixed**:
- No real-time sensor data push to frontend
- Clients must poll for updates (higher latency)
- Cannot meet p95 < 1.5s store→client SLO

**Action Required**:
1. Create `src/backend/services/realtime-subscriptions/` service
2. Configure PostgreSQL logical replication (pgoutput plugin)
3. Implement subscription management with RLS filtering
4. Add WebSocket handler for client connections
5. Create subscription channel routing (sensor_readings, alerts, tasks)

**Code Locations**:
- `src/backend/services/realtime-subscriptions/` (create)
- `src/database/scripts/setup-logical-replication.sql` (create)

**Acceptance Criteria**:
- [ ] Clients can subscribe to site-specific channels
- [ ] Real-time updates pushed within 1.5s of DB write
- [ ] RLS policies enforced on subscriptions
- [ ] Load test validates 1000+ concurrent connections
- [ ] Graceful reconnection handling

**Related**:
- Track A Item: track-a-14
- Load Test: `tests/load/realtime-push-load.js`

---

## 🟡 Medium Priority Debt

### DEBT-003: E2E Test Coverage for Critical Paths
**Category**: Testing  
**Created**: 2025-09-29  
**Squad**: QA-Squad  
**Effort**: 5 days  

**Context**:
Contract tests and load tests complete, but E2E tests for user flows not implemented.

**Why Deferred**:
- Requires actual UI and API endpoints to be live
- Infrastructure testing prioritized first

**Impact if Not Fixed**:
- Manual regression testing required before releases
- Risk of breaking user flows undetected
- Slower deployment velocity

**Action Required**:
1. Setup Playwright or Cypress framework
2. Implement tests for critical flows:
   - User login → Dashboard → Task creation → Assignment
   - Sensor data viewing → Alert triggering → Resolution
   - Inventory lot creation → COA upload → Destruction approval
3. Integrate into CI pipeline (run on staging deploy)

**Code Locations**:
- `tests/e2e/` (create framework)
- `.github/workflows/e2e-tests.yml` (create)

**Acceptance Criteria**:
- [ ] 5+ critical user flows automated
- [ ] Tests run on staging after deployment
- [ ] Failed tests block production promotion
- [ ] Video recordings on failure

**Related**:
- DoD: `docs/governance/DEFINITION_OF_DONE.md` (E2E test section)

---

### DEBT-004: Alert Routing Configuration
**Category**: Observability  
**Created**: 2025-09-29  
**Squad**: DevOps-Squad  
**Effort**: 1 day  

**Context**:
Alert rules defined in Prometheus, but Slack/PagerDuty integrations not configured.

**Why Deferred**:
- Requires Slack workspace and PagerDuty account provisioning
- Documentation provided for setup

**Impact if Not Fixed**:
- Alerts only visible in Prometheus UI
- No proactive incident response
- On-call team not notified of critical issues

**Action Required**:
1. Create Slack workspace bot (or use existing)
2. Configure Alertmanager with Slack webhook
3. Setup PagerDuty integration for critical alerts
4. Test alert routing with synthetic incidents

**Code Locations**:
- `src/infrastructure/monitoring/alertmanager/` (create config)
- `src/infrastructure/monitoring/prometheus/alerts/` (rules exist)

**Acceptance Criteria**:
- [ ] SLO burn-rate alerts → Slack #alerts channel
- [ ] Critical errors (P0/P1) → PagerDuty
- [ ] Alert formatting includes runbook links
- [ ] Test alert received within 1 minute

**Related**:
- Documentation: `docs/ops/runbooks/RUNBOOK_TEMPLATE.md`
- Prometheus Alerts: `src/infrastructure/monitoring/prometheus/alerts/slo-burn-rate.yml`

---

### DEBT-005: Unit Test Coverage for Business Logic
**Category**: Testing  
**Created**: 2025-09-29  
**Squad**: All Squads (service-specific)  
**Effort**: 2 days per service  

**Context**:
Testing infrastructure complete (frameworks, CI), but service-specific unit tests not written.

**Why Deferred**:
- Requires actual service implementation
- Test infrastructure prioritized first

**Impact if Not Fixed**:
- Increased bug rate in production
- Difficult to refactor with confidence
- Longer debugging cycles

**Action Required**:
Per service:
1. Achieve 80% code coverage for business logic
2. 90% coverage for critical paths (e.g., financial, compliance)
3. Mock external dependencies (DB, APIs)
4. Fast execution (< 5 seconds per test suite)

**Code Locations**:
- `src/backend/services/*/tests/` (create per service)
- `.github/workflows/ci-dotnet.yml` (coverage reporting exists)

**Acceptance Criteria**:
- [ ] Each service: 80%+ coverage
- [ ] Critical services: 90%+ coverage
- [ ] Coverage reported in PRs
- [ ] Failed coverage gates block merge

**Related**:
- DoR: `docs/governance/DEFINITION_OF_READY.md` (testing section)
- CI Workflow: `.github/workflows/ci-dotnet.yml`

---

### DEBT-006: Structured Logging for Identity Service
**Category**: Observability  
**Created**: 2025-09-29  
**Squad**: Core Platform Squad  
**Effort**: 1 day  

**Context**:
`Program.cs` currently ships with comments referencing Serilog, but no structured logging is configured. Identity service logs are limited to default console output which lacks correlation IDs and JSON formatting expected by the central logging stack.

**Why Deferred**:
- Serilog package references and configuration were deferred while repository wiring was prioritized.
- Awaiting confirmation on shared logging package layout across services.

**Impact if Not Fixed**:
- Logs won't include structured fields for correlation or ingestion by Loki/Splunk.
- Harder to trace badge authentication and policy evaluation issues.
- Inconsistent logging patterns across microservices.

**Action Required**:
1. Add Serilog dependencies (core, console, Seq/Elasticsearch sinks as required) to the identity service project.
2. Configure `builder.Host.UseSerilog(...)` in `Program.cs`, reading from configuration/environment.
3. Ensure request/response logging and enrichers (trace/span IDs, correlation IDs) are registered.
4. Update service configuration files with logging settings and document rollout.

**Code Locations**:
- `src/backend/services/core-platform/identity/API/Program.cs`
- `config/identity/appsettings.*.json` (create/update logging sections)

**Acceptance Criteria**:
- [ ] Identity service emits structured JSON logs with correlation IDs.
- [ ] Logs ingested successfully by centralized logging stack.
- [ ] Logging configuration documented for other Track B services.

**Related**:
- Track B Checklist item: FRP-01 Dependency Injection – Serilog task

---

### DEBT-007: Database Health Check Endpoint
**Category**: Reliability  
**Created**: 2025-09-29  
**Squad**: Core Platform Squad  
**Effort**: 1 day  

**Context**:
Identity service currently exposes `/healthz` returning a static payload. No connectivity test is performed against PostgreSQL, so Kubernetes/ingress probes cannot detect database outages or RLS misconfiguration.

**Why Deferred**:
- Basic endpoint was added to unblock smoke testing; full health check requires additional packages and configuration.

**Impact if Not Fixed**:
- Service may report healthy while database connections fail, leading to runtime errors for clients.
- Deployment rollouts cannot rely on health probes to gate traffic.

**Action Required**:
1. Add ASP.NET Core health checks (e.g., `Microsoft.Extensions.Diagnostics.HealthChecks.Npgsql`).
2. Register a DB health check that executes a lightweight query via `IdentityDbContext` with RLS context.
3. Expose `/healthz` (or `/health/ready`) wired to the health check pipeline and update Kubernetes probe definitions.
4. Add unit/integration coverage ensuring health endpoint fails when DB is unreachable.

**Code Locations**:
- `src/backend/services/core-platform/identity/API/Program.cs`
- `deploy/k8s/identity/*.yaml` (update readiness/liveness probes)

**Acceptance Criteria**:
- [ ] Health endpoint returns unhealthy when PostgreSQL connectivity or RLS setup fails.
- [ ] Kubernetes readiness probe uses the new endpoint.
- [ ] Documentation updated for operations runbook.

**Related**:
- Track B Checklist item: FRP-01 Dependency Injection – DB health check

---

## 🟢 Low Priority Debt

_(No items currently)_

---

## 📝 Debt Item Template

When adding new debt, copy this template:

```markdown
### DEBT-XXX: [Title]
**Category**: [Observability | Testing | Performance | Security | Code Quality]  
**Created**: YYYY-MM-DD  
**Squad**: [Squad-Name]  
**Effort**: [X days]  

**Context**:
[Why does this debt exist? What was the original decision?]

**Why Deferred**:
[Why didn't we do it properly the first time?]

**Impact if Not Fixed**:
[What happens if we never address this?]

**Action Required**:
1. [Step 1]
2. [Step 2]
3. [Step 3]

**Code Locations**:
- `path/to/file.ext` (what needs to change)

**Acceptance Criteria**:
- [ ] Criteria 1
- [ ] Criteria 2

**Related**:
- Issue: #123
- PR: #456
```

---

## 🔄 Debt Review Process

### When to Review

1. **End of Sprint** - Review all debt, reprioritize
2. **After Major Feature** - Check if new debt introduced
3. **Before Production Release** - Assess high-priority debt risk
4. **Quarterly** - Deep dive on medium/low debt

### Review Checklist

- [ ] Is the context still accurate?
- [ ] Has priority changed?
- [ ] Can we address it in next sprint?
- [ ] Should we create a backlog item?
- [ ] Can we delete it (no longer relevant)?

### Escalation Criteria

Escalate to Engineering Manager if:
- High-priority debt > 30 days old
- Total debt effort > 2 sprint capacity
- Debt causing production incidents

---

## 📈 Debt Metrics

Track these metrics over time:

| Month | Total Items | High Priority | Effort (days) | Items Resolved |
|-------|-------------|---------------|---------------|----------------|
| Sep 2025 | 7 | 2 | 15 | 0 |
| Oct 2025 | - | - | - | - |
| Nov 2025 | - | - | - | - |

**Target**: Resolve 2-3 debt items per sprint.

---

## 🚫 What's NOT Technical Debt

Don't add these to this register:

- **Product Features** - Use product backlog
- **Bugs** - Use bug tracker / GitHub Issues
- **Infrastructure Improvements** - Use separate Infrastructure Roadmap
- **"Nice to Have" Features** - Use feature backlog

**Technical Debt** = Code/architecture that works but could be better for maintainability, scalability, or reliability.

---

## ✅ Completed Debt (Archive)

_(Move resolved items here for reference)_

### DEBT-000: Example Resolved Debt
**Resolved**: 2025-09-15  
**Resolution**: PR #123 - Refactored authentication to use JWT  
**Actual Effort**: 2 days (estimated 3 days)

---

**Last Review**: 2025-09-29  
**Next Review**: End of Sprint 1  
**Owner**: Engineering Manager
