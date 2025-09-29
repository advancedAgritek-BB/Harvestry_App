# Feature Flags Configuration

**Track A Implementation** - Site-scoped feature flags with Unleash

## 🎯 Purpose

This directory contains feature flag definitions for Harvestry ERP. Flags enable safe, gradual rollout of high-risk features with explicit promotion workflows.

---

## 🚩 Gated Flags (Require Promotion Checklist)

These flags require shadow mode, staged rollout, VP Product + SRE sign-off, and full governance artifacts per CI policy:

### `closed_loop_ecph_enabled`
**Purpose**: Enable closed-loop EC/pH control  
**Risk Level**: HIGH (regulatory, crop safety)  
**Promotion**: Shadow 14d → Staged 7d → Enable  
**Rollback**: One-click disable returns to open-loop  
**Runbook**: `/docs/ops/runbooks/Runbook_Closed_Loop_Control.md`

### `ai_auto_apply_enabled`
**Purpose**: AI predictions automatically applied (no human confirmation)  
**Risk Level**: HIGH (operational impact)  
**Promotion**: Shadow 14d → Staged 7d → Enable  
**Rollback**: Disable returns to recommendation-only mode  
**Runbook**: `/docs/ops/runbooks/Runbook_AI_Auto_Apply.md`

### `et0_steering_enabled`
**Purpose**: ET₀-based irrigation steering  
**Risk Level**: MEDIUM (irrigation efficiency)  
**Promotion**: Shadow 14d → Staged 7d → Enable  
**Rollback**: Disable returns to manual scheduling  
**Runbook**: `/docs/ops/runbooks/Runbook_ET0_Steering.md`

### `autosteer_mpc_enabled`
**Purpose**: Model Predictive Control for climate/lighting  
**Risk Level**: HIGH (crop health, energy costs)  
**Promotion**: Shadow 14d → Staged 7d → Enable  
**Rollback**: Disable returns to setpoint control  
**Runbook**: `/docs/ops/runbooks/Runbook_Autosteer_MPC.md`

### `slack_mirror_mode`
**Purpose**: Full two-way Slack synchronization  
**Risk Level**: GATED (integration reliability)  
**Promotion**: Shadow 14d → Staged 7d → Enable  
**Rollback**: Disable returns to notify-only mode  
**Note**: Requires full promotion checklist and governance artifacts (see CI policy below)

### `sms_critical_enabled`
**Purpose**: SMS notifications for critical alerts  
**Risk Level**: GATED (critical alerting channel)  
**Promotion**: Shadow 14d → Staged 7d → Enable  
**Rollback**: Disable stops SMS, keeps Slack/email  
**Note**: Requires full promotion checklist and governance artifacts (see CI policy below)

### `predictive_maintenance_auto_wo`
**Purpose**: Auto-create work orders from PdM predictions  
**Risk Level**: GATED (automated operational workflow)  
**Promotion**: Shadow 14d → Staged 7d → Enable  
**Rollback**: Disable requires manual work order creation  
**Note**: Requires full promotion checklist and governance artifacts (see CI policy below)

---

## 📋 Standard Flags

These flags can be toggled without extended promotion workflow:

### `clickhouse_olap_enabled`
**Purpose**: Enable ClickHouse for OLAP queries  
**Risk Level**: LOW (performance optimization)  
**Rollback**: Disable uses TimescaleDB for all queries

---

## 🔧 Usage

### Import Flags to Unleash

```bash
# Via Unleash UI
1. Navigate to http://localhost:4242/projects/default/features
2. Click "Import features"
3. Upload config/feature-flags/unleash-flags.json

# Via API
curl -X POST http://localhost:4242/api/admin/features-batch/import \
  -H "Authorization: *:*.unleash-insecure-api-token" \
  -H "Content-Type: application/json" \
  -d @config/feature-flags/unleash-flags.json
```

### Enable Flag for Specific Site

```bash
# Example: Enable closed-loop for site-123 in shadow mode
curl -X POST http://localhost:4242/api/admin/projects/default/features/closed_loop_ecph_enabled/environments/development/strategies \
  -H "Authorization: *:*.unleash-insecure-api-token" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "flexibleRollout",
    "constraints": [
      {
        "contextName": "siteId",
        "operator": "IN",
        "values": ["site-123"]
      }
    ],
    "parameters": {
      "rollout": "100",
      "stickiness": "default",
      "groupId": "closed_loop_ecph_enabled"
    },
    "variants": [
      {
        "name": "shadow",
        "weight": 1000,
        "stickiness": "default",
        "payload": {
          "type": "json",
          "value": "{\"mode\":\"shadow\"}"
        }
      }
    ]
  }'
```

### Check Flag Status (Application Code)

```csharp
// .NET SDK example
var isEnabled = await _unleashClient.IsEnabledAsync(
    "closed_loop_ecph_enabled",
    new UnleashContext
    {
        UserId = currentUserId.ToString(),
        Properties = new Dictionary<string, string>
        {
            { "siteId", siteId.ToString() },
            { "environment", "production" }
        }
    }
);

// Get variant (e.g., shadow mode)
var variant = await _unleashClient.GetVariantAsync("closed_loop_ecph_enabled", context);
if (variant.Name == "shadow")
{
    // Run in shadow mode (log decisions, don't apply)
    _logger.LogInformation("Closed-loop running in shadow mode");
}
```

---

## 🔐 Security & Governance

### CI/CD Gate

The `feature-flag-policy.yml` workflow (`.github/workflows/feature-flag-policy.yml`) enforces:
- ✅ Gated flags in production require PDD + Runbook links
- ✅ PR must reference promotion checklist
- ✅ Auto-comment with governance requirements

**Authoritative Source**: The CI policy workflow at `.github/workflows/feature-flag-policy.yml` (lines 40-48) defines the complete list of gated flags requiring full promotion artifacts. When enabling any gated flag for production, the CI gate will validate that your PR includes:
- PDD (Product Design Document) link
- Operational Runbook link
- Promotion Checklist (recommended)
- Shadow Mode Results (recommended)

### Audit Trail

All flag changes are logged to `authorization_audit` table:

```sql
SELECT 
    user_id,
    action,
    resource_type,
    granted,
    occurred_at
FROM authorization_audit
WHERE resource_type = 'feature_flag'
ORDER BY occurred_at DESC
LIMIT 50;
```

---

## 📊 Monitoring

### Flag Usage Metrics

```promql
# Flag evaluation rate
rate(feature_flag_evaluations_total[5m])

# Flag enabled by name
feature_flag_enabled{flag_name="closed_loop_ecph_enabled"}

# Variant distribution
feature_flag_variant_count{flag_name="closed_loop_ecph_enabled",variant="shadow"}
```

### Unleash Dashboard

- **URL**: http://localhost:4242
- **Metrics**: View flag impressions, evaluations, variants
- **Audit**: Track all flag changes with user attribution

---

## 🚨 Emergency Disable

If a feature causes production issues:

1. **Immediate**: Disable via Unleash UI (< 2 minutes)
2. **Verify**: Check dashboards for rollback confirmation
3. **Notify**: Post in #incidents Slack channel
4. **Post-Mortem**: Schedule within 24 hours

**Rollback Verification**:
```bash
# Check flag status
curl http://localhost:4242/api/admin/projects/default/features/[flag-name] \
  -H "Authorization: *:*.unleash-insecure-api-token"

# Expected: "enabled": false
```

---

## 📚 Related Documentation

- [Feature Flag Promotion Checklist](../../docs/governance/FEATURE_FLAG_PROMOTION_CHECKLIST.md)
- [Feature Flag Policy CI Gate](../../.github/workflows/feature-flag-policy.yml)
- [Definition of Ready (DoR)](../../docs/governance/DEFINITION_OF_READY.md)
- [Definition of Done (DoD)](../../docs/governance/DEFINITION_OF_DONE.md)

---

**✅ Track A Objective:** Enable safe, site-scoped feature rollout with explicit governance and one-click rollback.
