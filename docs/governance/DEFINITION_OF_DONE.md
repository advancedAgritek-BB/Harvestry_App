# Definition of Done (DoD)

**Track A Governance** - Quality gates for shipping production-ready features

## 🎯 Purpose

The Definition of Done ensures that work is truly complete—tested, documented, deployed, and ready for production use without technical debt or operational burden.

**Principle**: A story is "done" when it delivers value to users with confidence and no follow-up work required.

---

## ✅ Universal DoD (All Stories)

Every story must meet these criteria before closing:

- [ ] **Acceptance criteria met** - All conditions in story satisfied
- [ ] **Code reviewed** - Minimum 2 approvals from squad members
- [ ] **Tests passing** - All CI checks green (lint, test, build, security)
- [ ] **Merged to main** - PR approved and merged
- [ ] **Documentation updated** - README, API docs, or runbooks current
- [ ] **No linter warnings** - Zero ESLint, Pylint, or Roslyn warnings
- [ ] **No known bugs** - Feature works as intended in all tested scenarios

---

## 🗄️ Database Changes DoD

If the story involved database migrations:

- [ ] **Migrations applied** - Successfully run on dev, staging, and production
- [ ] **Rollback tested** - Down migration verified in staging
- [ ] **RLS policies active** - Row-level security enforced and tested
- [ ] **Indexes optimized** - Query performance meets SLO targets
- [ ] **Data integrity verified** - No orphaned records or constraint violations
- [ ] **Backup tested** - PITR (Point-in-Time Recovery) validated

**Validation**: Run test queries to ensure data is accessible and performant.

---

## 📊 Observability DoD

For features that impact system behavior:

- [ ] **Metrics instrumented** - Prometheus metrics exposed and scraped
- [ ] **Dashboards updated** - Grafana panels show new metrics
- [ ] **Alerts configured** - Burn-rate or threshold alerts wired to Slack/PagerDuty
- [ ] **Traces added** - OpenTelemetry spans capture end-to-end flow
- [ ] **Logs structured** - JSON logs with trace IDs and contextual fields
- [ ] **SLOs validated** - Feature meets latency/error rate targets

**SLO Targets (Track A)**:
- Ingest p95 < 1.0s
- Store→Client p95 < 1.5s
- Command p95 < 800ms
- Task/Messaging p95 < 300ms
- Site-Day Report p95 < 90s

---

## 🚀 Deployment DoD

All code changes must be deployed and verified:

- [ ] **Auto-deployed to staging** - PR to main triggers staging deployment
- [ ] **Smoke tests passing** - Post-deployment health checks green
- [ ] **Contract tests passing** - API contracts validated
- [ ] **Health dashboards green** - All services healthy for 15 minutes post-deploy
- [ ] **Rollback tested** - Verified rollback procedure works
- [ ] **Production ready** - Approved for production release (if not already deployed)

**Staging Validation Window**: 15 minutes of green health checks before production release.

---

## 🧪 Testing DoD

Test coverage requirements:

- [ ] **Unit tests added** - New code covered at 80%+ (critical paths 90%+)
- [ ] **Integration tests added** - API endpoints tested with database
- [ ] **E2E tests updated** - Critical user flows automated (if UI changes)
- [ ] **Performance tests passing** - Load tests meet SLO targets
- [ ] **Security tests passing** - RLS/ABAC validation, no SQL injection
- [ ] **Manual testing complete** - Feature validated in staging by PO or QA

**Test Pyramid**:
- Many unit tests (fast, isolated)
- Some integration tests (database, APIs)
- Few E2E tests (full user flows)

---

## 🔐 Security DoD

Security validation required:

- [ ] **RLS policies applied** - Site-scoped data isolation enforced
- [ ] **ABAC gates tested** - High-risk operations gated by role
- [ ] **Audit trail active** - Changes logged to authorization_audit table
- [ ] **Secrets secured** - No hardcoded credentials, using KMS/Vault
- [ ] **SAST clean** - CodeQL/Snyk scans pass with zero high/critical issues
- [ ] **Dependency audit clean** - No vulnerable dependencies

**Security Review Triggers**:
- Authentication/authorization changes
- PII/PHI data handling
- Financial transactions
- Compliance-critical operations

---

## 🚩 Feature Flag DoD

If feature flag was used:

- [ ] **Flag enabled** - Feature active for intended sites/users
- [ ] **Monitoring active** - Flag state changes logged and alerted
- [ ] **Rollback validated** - Disabling flag reverts behavior immediately
- [ ] **Promotion checklist** - Shadow → Staged → Enabled workflow completed (for risky flags)
- [ ] **Flag documented** - Purpose and rollback procedure in runbook

**Flag Lifecycle**:
- Shadow mode: 14+ days
- Staged rollout: 7+ days
- Full enable: After VP Product + SRE sign-off

---

## 📐 API Contract DoD

If API contracts changed:

- [ ] **OpenAPI spec updated** - Swagger/Postman collections current
- [ ] **Contract tests passing** - Pact/Spring Cloud Contract validated
- [ ] **Backward compatible** - No breaking changes (or version bumped)
- [ ] **Client teams notified** - Frontend/mobile/integration partners informed
- [ ] **Deprecation scheduled** - Old endpoints marked with sunset date (if replacing)

**Breaking Change Process**:
1. Announce deprecation 6 months in advance
2. Add new endpoint (v2)
3. Mark old endpoint deprecated
4. Monitor usage, migrate clients
5. Remove old endpoint after sunset

---

## 📚 Documentation DoD

Documentation must be current:

- [ ] **Code comments added** - Complex logic explained
- [ ] **README updated** - Setup instructions accurate
- [ ] **API docs updated** - OpenAPI/GraphQL schema reflects changes
- [ ] **Runbook updated** - Operational procedures current (if service changes)
- [ ] **User guide updated** - End-user documentation (if UI changes)
- [ ] **ADR created** - Architecture Decision Record (if architectural change)
- [ ] **Changelog updated** - Release notes drafted

**Documentation Audience**:
- Code comments: Future developers
- README: New team members
- API docs: API consumers
- Runbooks: On-call engineers
- User guides: End users

---

## 🔄 Staging Validation DoD (Track A Specific)

Track A requires extended validation on staging:

- [ ] **SLO compliance** - All latency/error targets met for 7 consecutive days
- [ ] **Zero critical errors** - Sentry shows no P0/P1 issues
- [ ] **Burn-rate alerts** - No false positives, no missed alerts
- [ ] **Load tested** - Synthetic load simulates production traffic
- [ ] **Dashboard reviewed** - Squad lead reviews metrics daily
- [ ] **Runbook validated** - On-call team confirms runbook accuracy

**Staging Validation Window**: 7 days of stable operation before production release.

---

## 🚨 Production Release DoD

Before marking story as "Done" in production:

- [ ] **Deployed to production** - Blue/green or canary rollout complete
- [ ] **Health checks passing** - All services healthy for 1 hour post-deploy
- [ ] **SLOs maintained** - No degradation in availability or latency
- [ ] **User validation** - Feature tested by real users (pilot or beta)
- [ ] **No incidents** - Zero P0/P1 incidents in first 48 hours
- [ ] **Metrics normal** - Dashboard shows expected behavior

**Production Validation Window**: 48 hours of stable operation.

---

## 🚫 Common DoD Failures

**Story is NOT done if:**

❌ "Works on my machine" (not deployed to staging)  
❌ Tests skipped ("we'll add them later")  
❌ Documentation missing ("it's self-explanatory")  
❌ Feature flag disabled ("waiting for approval")  
❌ Metrics missing ("we can add telemetry later")  
❌ Runbook outdated ("ops knows how it works")  
❌ Performance untested ("seems fast enough")  

---

## ✍️ DoD Checklist by Story Type

### Feature Story
- [ ] Universal DoD ✅
- [ ] Testing DoD ✅
- [ ] Observability DoD ✅
- [ ] Security DoD ✅
- [ ] Deployment DoD ✅
- [ ] Staging validation (7 days) ✅
- [ ] Production release DoD ✅

### Bug Fix
- [ ] Universal DoD ✅
- [ ] Regression test added ✅
- [ ] Root cause documented ✅
- [ ] Hot-fixed if P0/P1 ✅

### Database Migration
- [ ] Universal DoD ✅
- [ ] Database changes DoD ✅
- [ ] Zero-downtime validated ✅
- [ ] Rollback tested ✅

### Technical Debt
- [ ] Universal DoD ✅
- [ ] Code quality improved ✅
- [ ] Tests refactored ✅
- [ ] No new tech debt introduced ✅

---

## 🔄 DoD Review Process

1. **Developer** self-reviews against DoD checklist
2. **Peer Review** - 2 squad members approve PR
3. **CI/CD** - Automated checks validate tests, lint, security
4. **Staging Deploy** - Auto-deploy to staging on merge to main
5. **7-Day Validation** - Staging metrics reviewed daily
6. **Production Deploy** - Blue/green rollout with health checks
7. **48-Hour Monitoring** - Post-production validation
8. **Story Closed** - Marked as "Done" after validation complete

---

## 📊 DoD Metrics

Track DoD compliance to improve quality:

- **% Stories Meeting DoD First Time** (Target: 95%+)
- **Average Staging Time** (Target: 7 days)
- **Production Incidents Post-Release** (Target: < 2%)
- **Rollback Rate** (Target: < 1%)

---

## 🛠️ Tools for DoD Validation

| Tool | Purpose |
|------|---------|
| **GitHub Actions** | Automated CI checks |
| **SonarQube** | Code coverage tracking |
| **CodeQL / Snyk** | Security scanning |
| **Sentry** | Error tracking |
| **Grafana** | Metrics visualization |
| **Prometheus** | SLO monitoring |
| **k6 / Locust** | Load testing |
| **Postman / Pact** | Contract testing |

---

**✅ Track A Objective:** Ship production-ready features with confidence, zero technical debt, and operational excellence.
