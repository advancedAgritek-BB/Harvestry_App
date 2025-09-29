# Definition of Ready (DoR)

**Track A Governance** - Quality gates before work begins

## 🎯 Purpose

The Definition of Ready ensures that user stories and tasks have all necessary information, context, and preparation before a squad commits to delivering them in a sprint.

**Principle**: A story is "ready" when the team can confidently estimate effort and start work immediately without blockers.

---

## ✅ Universal DoR (All Stories)

Every story must have:

- [ ] **Clear acceptance criteria** - Testable conditions for "done"
- [ ] **User story format** - "As a [persona], I want [goal], so that [benefit]"
- [ ] **Squad assignment** - Owned by a single squad (no shared ownership)
- [ ] **Priority set** - P0 (blocker), P1 (critical), P2 (important), P3 (nice-to-have)
- [ ] **Dependencies identified** - Blocked by or blocks other stories
- [ ] **Estimated** - Story points or T-shirt sizing agreed by team
- [ ] **Refinement complete** - Story reviewed in backlog refinement session

---

## 🗄️ Database Changes

If the story involves database schema changes:

- [ ] **Migration drafted** - SQL migration script or EF Core migration created
- [ ] **Zero-downtime strategy** - Expand/contract pattern documented
- [ ] **Rollback plan** - Down migration tested
- [ ] **RLS policies noted** - Row-level security impact assessed
- [ ] **Index strategy** - Query performance impact analyzed
- [ ] **Data volume considered** - Migration tested with production-scale data

**Example**: Adding a column?

- Phase 1 (Expand): Add nullable column
- Phase 2 (Deploy): Application uses new column
- Phase 3 (Contract): Backfill data, add NOT NULL constraint

---

## 📊 Observability Requirements

If the story involves new features or significant logic changes:

- [ ] **Metrics defined** - What metrics will track success?
- [ ] **Dashboard planned** - Which Grafana dashboard will display metrics?
- [ ] **Alerts planned** - What conditions trigger alerts?
- [ ] **Traces instrumented** - OpenTelemetry spans defined
- [ ] **Logs structured** - JSON logging with contextual fields
- [ ] **SLO impact assessed** - Will this affect existing SLO targets?

**Questions to answer**:

- What latency is acceptable? (p95, p99)
- What error rate triggers investigation?
- What capacity limits exist?

---

## 🚩 Feature Flags

If the story introduces new behavior that could be risky:

- [ ] **Flag strategy defined** - Site-scoped, user-scoped, or global?
- [ ] **Default state** - Disabled by default in production
- [ ] **Rollout plan** - Shadow → Staged → Enabled
- [ ] **Rollback plan** - One-click disable mechanism
- [ ] **Promotion checklist** - Required if risky flag (see FEATURE_FLAG_PROMOTION_CHECKLIST.md)

**Risky features requiring flags**:

- Closed-loop control
- AI auto-apply
- Bulk operations
- Financial transactions
- Compliance-critical operations

---

## 🔐 Security & Compliance

If the story touches sensitive data or regulatory requirements:

- [ ] **RLS policies** - Site-scoped access enforced
- [ ] **ABAC gates** - High-risk operations gated by role
- [ ] **Audit trail** - Changes logged to audit table
- [ ] **PII handling** - Personal data encrypted/anonymized
- [ ] **Compliance reviewed** - Regulatory officer sign-off if needed
- [ ] **Security review** - Penetration test scope if needed

**Questions to answer**:

- Does this handle PII/PHI?
- Could this violate METRC/state regulations?
- Does this enable financial fraud?

---

## 🧪 Testing Requirements

All stories must define test strategy:

- [ ] **Unit tests** - Core logic tested in isolation
- [ ] **Integration tests** - API endpoints or database interactions tested
- [ ] **E2E tests** - Critical user flows automated (if applicable)
- [ ] **Performance tests** - Load/stress tests for high-throughput features
- [ ] **Security tests** - RLS/ABAC validation, injection attack prevention

**Test coverage targets**:

- Unit: 80% coverage
- Integration: Critical paths covered
- E2E: MVP user flows automated

---

## 📐 API Contract Changes

If the story changes API contracts (REST, WebSocket, GraphQL):

- [ ] **OpenAPI spec updated** - Swagger/OpenAPI YAML reflects changes
- [ ] **Backward compatibility** - No breaking changes without version bump
- [ ] **Contract tests** - Pact/Spring Cloud Contract tests updated
- [ ] **Client impact assessed** - Frontend/mobile teams notified
- [ ] **Deprecation plan** - Old endpoints marked deprecated with sunset date

**API versioning**:

- Add new endpoint: `/api/v2/resource`
- Deprecate old endpoint: `/api/v1/resource` (6-month sunset)

---

## 🏗️ Infrastructure Changes

If the story requires infrastructure changes:

- [ ] **IaC updated** - Terraform/Helm charts modified
- [ ] **Cost impact** - Estimated cloud cost increase/decrease
- [ ] **Capacity planned** - Resource limits defined (CPU, memory, disk)
- [ ] **DR impact** - Disaster recovery tested with new infrastructure
- [ ] **Secrets managed** - KMS/Vault integration for new credentials

---

## 📚 Documentation

Documentation requirements vary by story type:

- [ ] **Code comments** - Complex logic explained inline
- [ ] **README updated** - Setup instructions current
- [ ] **API docs** - OpenAPI spec or GraphQL schema updated
- [ ] **Runbook** - Operational procedures documented (if new service/feature)
- [ ] **User guide** - End-user documentation (if UI changes)
- [ ] **ADR** - Architecture Decision Record (if architectural change)

---

## 🚫 Common DoR Failures

**Story is NOT ready if:**

❌ Acceptance criteria are vague ("make it better")  
❌ Dependencies are unclear ("probably need API team")  
❌ No one understands the technical approach  
❌ Migration strategy is "we'll figure it out"  
❌ Security impact is "probably fine"  
❌ Testing strategy is "manual testing should work"  

---

## ✍️ DoR Checklist by Story Type

### Feature Story

- [ ] Universal DoR ✅
- [ ] Feature flag strategy ✅
- [ ] Observability plan ✅
- [ ] Testing strategy ✅
- [ ] Security review ✅

### Bug Fix

- [ ] Universal DoR ✅
- [ ] Root cause identified ✅
- [ ] Regression test added ✅
- [ ] Affected users/sites identified ✅

### Technical Debt

- [ ] Universal DoR ✅
- [ ] Business value articulated ✅
- [ ] Risk of NOT doing work ✅
- [ ] Refactoring scope limited ✅

### Database Migration

- [ ] Universal DoR ✅
- [ ] Database changes DoR ✅
- [ ] Zero-downtime strategy ✅
- [ ] Rollback tested ✅

---

## 🔄 DoR Review Process

1. **Product Owner** drafts story with acceptance criteria
2. **Squad Lead** reviews for technical feasibility
3. **Backlog Refinement** (weekly) - Team discusses and fills gaps
4. **DoR Check** - Scrum Master validates all checkboxes
5. **Sprint Planning** - Only "Ready" stories pulled into sprint

**DoR Review Frequency**: Weekly in backlog refinement

---

## 📊 DoR Metrics

Track DoR compliance to improve process:

- **% Stories Ready at Sprint Planning** (Target: 100%)
- **Stories Returned to Backlog Mid-Sprint** (Target: < 5%)
- **Average Time to Ready** (Target: < 1 week from creation)

---

**✅ Track A Objective:** Eliminate mid-sprint blockers and scope creep by ensuring stories are fully prepared before commitment.
