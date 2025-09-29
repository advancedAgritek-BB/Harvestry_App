# Runbook: [Service/Feature Name]

**Track A Operational Documentation**

| Field | Value |
|-------|-------|
| **Service** | [Service Name] |
| **Squad** | [Owning Squad] |
| **On-Call Primary** | [Slack Channel / PagerDuty] |
| **Last Updated** | [YYYY-MM-DD] |
| **SLO Target** | [e.g., p95 < 800ms, 99.9% availability] |

---

## 🎯 Service Overview

### Purpose
[What does this service do? What business value does it provide?]

### Architecture
[Brief architecture description - dependencies, data flow, external integrations]

### Key Components
- **Component 1**: [Description]
- **Component 2**: [Description]
- **Component 3**: [Description]

---

## 📊 Monitoring & Dashboards

### Primary Dashboard
**URL**: [Link to Grafana dashboard]

### Key Metrics
| Metric | SLO Target | Alert Threshold | Dashboard Panel |
|--------|------------|-----------------|-----------------|
| Request Latency (p95) | < 800ms | > 1.2s for 5min | [Panel Name] |
| Error Rate | < 0.1% | > 1% for 5min | [Panel Name] |
| Queue Depth | < 1000 | > 5000 | [Panel Name] |
| Replication Lag | < 500ms | > 2s | [Panel Name] |

### Health Check Endpoints
- **Liveness**: `GET /health/live` (Should return 200)
- **Readiness**: `GET /health/ready` (Should return 200)
- **Metrics**: `GET /metrics` (Prometheus format)

---

## 🚨 Common Alerts & Resolution

### Alert: [Alert Name]

**Severity**: [Critical / Warning / Info]  
**Trigger**: [Condition that fires alert]  
**Impact**: [What users experience]

**Immediate Actions (< 5 minutes)**:
1. [Action 1]
2. [Action 2]
3. [Action 3]

**Investigation Steps**:
1. Check dashboard: [Link]
2. Query logs: `kubectl logs -f deployment/[service-name] | grep [pattern]`
3. Check dependencies: [List dependent services]

**Resolution**:
- **Common Cause 1**: [Description] → [Fix]
- **Common Cause 2**: [Description] → [Fix]
- **Common Cause 3**: [Description] → [Fix]

**Rollback Procedure** (if needed):
```bash
# Commands to rollback to previous stable version
kubectl rollout undo deployment/[service-name]
# Or
./scripts/deploy/rollback.sh --service=[service-name] --version=[stable-version]
```

**Post-Resolution**:
- [ ] Update incident tracker
- [ ] Post in #incidents Slack channel
- [ ] Schedule post-mortem if P0/P1

---

### Alert: [Alert Name 2]

[Repeat above structure for each common alert]

---

## 🔧 Common Operations

### Restart Service

```bash
# Graceful restart (zero-downtime)
kubectl rollout restart deployment/[service-name]

# Verify restart
kubectl rollout status deployment/[service-name]

# Check pods are healthy
kubectl get pods -l app=[service-name]
```

### Scale Service

```bash
# Scale up
kubectl scale deployment/[service-name] --replicas=5

# Scale down
kubectl scale deployment/[service-name] --replicas=2

# Horizontal Pod Autoscaler (if configured)
kubectl get hpa [service-name]
```

### Clear Queue / Backlog

```bash
# Connect to database
psql -h [db-host] -U [username] -d [database]

# Query outbox queue depth
SELECT destination, COUNT(*) as pending_count
FROM outbox_messages
WHERE status = 'pending'
GROUP BY destination;

# Clear stuck messages (use with caution!)
UPDATE outbox_messages
SET status = 'failed', error_message = 'Manually cleared by [your-name]'
WHERE status = 'processing'
  AND updated_at < NOW() - INTERVAL '1 hour';
```

### Enable/Disable Feature Flag

```bash
# Via Unleash UI
# 1. Navigate to: http://unleash.harvestry.com/projects/default/features
# 2. Find feature: [feature-flag-name]
# 3. Toggle environment: [dev/staging/production]

# Via API (use with caution!)
curl -X POST http://unleash.harvestry.com/api/admin/projects/default/features/[flag-name]/environments/production/on \
  -H "Authorization: [api-token]"
```

### View Recent Logs

```bash
# Last 100 lines
kubectl logs deployment/[service-name] --tail=100

# Follow logs (live)
kubectl logs -f deployment/[service-name]

# Filter by error level
kubectl logs deployment/[service-name] | grep -i "error"

# Query via Loki
curl -G "http://loki:3100/loki/api/v1/query_range" \
  --data-urlencode 'query={service_name="[service-name]"} |= "ERROR"' \
  --data-urlencode 'start=[timestamp]' \
  --data-urlencode 'end=[timestamp]'
```

### Check Database Connection Pool

```bash
# Connect to PostgreSQL
psql -h [db-host] -U [username] -d [database]

# Check active connections
SELECT 
    datname,
    usename,
    client_addr,
    state,
    COUNT(*)
FROM pg_stat_activity
GROUP BY datname, usename, client_addr, state;

# Check connection pool saturation
SELECT 
    max_conn,
    used,
    res_for_super,
    max_conn - used - res_for_super AS available
FROM (
    SELECT 
        (SELECT setting::int FROM pg_settings WHERE name = 'max_connections') AS max_conn,
        (SELECT setting::int FROM pg_settings WHERE name = 'superuser_reserved_connections') AS res_for_super,
        (SELECT COUNT(*) FROM pg_stat_activity) AS used
) t;
```

---

## 🔄 Deployment Procedures

### Standard Deployment (Blue/Green)

```bash
# 1. Deploy to staging first
./scripts/deploy/deploy.sh --env=staging --service=[service-name]

# 2. Verify staging health (wait 15 minutes)
./scripts/deploy/health-check.sh --env=staging --service=[service-name]

# 3. Deploy to production (auto blue/green)
./scripts/deploy/deploy.sh --env=production --service=[service-name]

# 4. Monitor for 1 hour
watch -n 30 'kubectl get pods -l app=[service-name]'
```

### Emergency Hotfix

```bash
# 1. Create hotfix branch
git checkout -b hotfix/[issue-name] main

# 2. Make minimal fix, commit
git commit -m "fix: [description]"

# 3. Push and create PR (expedited review)
git push origin hotfix/[issue-name]

# 4. After approval, deploy directly to production
./scripts/deploy/hotfix.sh --service=[service-name]

# 5. Monitor closely for 2 hours
```

### Rollback

```bash
# Option 1: Kubernetes rollback (last deployment)
kubectl rollout undo deployment/[service-name]

# Option 2: Deploy specific version
./scripts/deploy/deploy.sh --env=production --service=[service-name] --version=[stable-version]

# Option 3: Feature flag disable (if feature-gated)
# Disable via Unleash UI or API
```

---

## 🗄️ Database Operations

### Run Migration

```bash
# Dry-run (validate without applying)
./scripts/db/migrate.sh --env=production --dry-run

# Apply migration
./scripts/db/migrate.sh --env=production

# Verify migration applied
psql -h [db-host] -U [username] -d [database] -c "SELECT * FROM __EFMigrationsHistory ORDER BY applied_at DESC LIMIT 5;"
```

### Rollback Migration

```bash
# Rollback last migration
./scripts/db/rollback.sh --env=production

# Rollback to specific migration
./scripts/db/rollback.sh --env=production --to=[migration-name]
```

### Check Replication Lag

```bash
# Connect to primary database
psql -h [primary-db-host] -U [username] -d [database]

# Check WAL lag
SELECT 
    client_addr,
    state,
    sync_state,
    replay_lag,
    write_lag,
    flush_lag
FROM pg_stat_replication;

# Alert threshold: replay_lag > 2 seconds
```

---

## 🔐 Security Operations

### Rotate Credentials

```bash
# 1. Generate new credentials in KMS/Vault
./scripts/security/rotate-secrets.sh --service=[service-name]

# 2. Update Kubernetes secrets
kubectl create secret generic [service-name]-secrets \
  --from-literal=DB_PASSWORD=[new-password] \
  --dry-run=client -o yaml | kubectl apply -f -

# 3. Restart service to pick up new secrets
kubectl rollout restart deployment/[service-name]

# 4. Verify service health
kubectl get pods -l app=[service-name]
```

### Review Audit Logs

```bash
# Query authorization audit
psql -h [db-host] -U [username] -d [database]

SELECT 
    user_id,
    action,
    resource_type,
    granted,
    occurred_at
FROM authorization_audit
WHERE granted = FALSE
  AND occurred_at > NOW() - INTERVAL '1 hour'
ORDER BY occurred_at DESC
LIMIT 50;
```

---

## 📞 Escalation Path

| Level | Contact | Response Time SLA |
|-------|---------|-------------------|
| **L1** | On-call engineer (PagerDuty) | 5 minutes |
| **L2** | Squad lead (#[squad]-alerts) | 15 minutes |
| **L3** | Engineering manager | 30 minutes |
| **L4** | VP Engineering / CTO | 1 hour (P0 only) |

**Escalation Triggers**:
- P0 incident (site-wide outage, data loss risk)
- P1 incident not resolved in 1 hour
- Regulatory violation risk
- Security breach suspected

---

## 📚 Related Documentation

- **Architecture Diagram**: [Link to diagram]
- **API Documentation**: [Link to OpenAPI/Swagger]
- **ADR**: [Link to Architecture Decision Records]
- **Feature Flag Runbook**: [Link if feature-flagged]
- **Disaster Recovery Plan**: [Link to DR runbook]

---

## 🧪 Testing & Validation

### Synthetic Monitoring

```bash
# Run synthetic load test
k6 run tests/load/[service-name]-load.js

# Expected p95: < 800ms
# Expected error rate: < 0.1%
```

### Contract Tests

```bash
# Run contract tests
npm run test:contract -- --service=[service-name]

# Verify API contracts unchanged
```

---

## 📝 Change Log

| Date | Change | Author |
|------|--------|--------|
| 2025-09-29 | Initial runbook created | [Your Name] |
| | | |
| | | |

---

## ✅ Runbook Validation Checklist

Before marking this runbook as complete:

- [ ] All links tested and working
- [ ] All commands tested in staging
- [ ] Dashboard screenshots/links current
- [ ] On-call team reviewed and approved
- [ ] Escalation path validated
- [ ] Rollback procedure tested
- [ ] Security review completed (if credentials mentioned)

---

**✅ Track A Objective:** Empower on-call engineers with clear, actionable operational procedures for rapid incident resolution.
