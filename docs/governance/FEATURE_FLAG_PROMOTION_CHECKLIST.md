# Feature Flag Promotion Checklist

**Track A Governance** - Shadow → Staged → Enable promotion workflow for risky features

## 🎯 Purpose

This checklist ensures safe, gradual rollout of high-risk features (closed-loop control, AI auto-apply, ET₀ steering) with proper validation at each stage.

---

## 📋 Feature Information

| Field | Value |
|-------|-------|
| **Feature Flag Name** | `_______________________` |
| **Site ID** | `_______________________` |
| **Requested By** | `_______________________` |
| **VP Product Approval** | `_______________________` |
| **SRE Approval** | `_______________________` |
| **Target Enable Date** | `_______________________` |

---

## 🔄 Stage 1: Shadow Mode (14+ Days)

Enable flag with `mode: shadow` - Feature logic runs but doesn't affect actual operations.

### Prerequisites
- [ ] **PDD (Product Design Document)** linked and reviewed
- [ ] **Runbook** created and reviewed by SRE
- [ ] **Rollback plan** documented (one-click revert)
- [ ] **Feature flag** created in Unleash with `shadow` mode
- [ ] **Monitoring dashboards** configured for feature metrics
- [ ] **Alerts** configured for anomaly detection
- [ ] **On-call team** briefed on feature and rollback procedure

### Shadow Mode Execution
- [ ] **Enable date**: `___________` 
- [ ] **Duration**: Minimum 14 days
- [ ] **Data collection**: Shadow decisions logged alongside actual operations

### Shadow Mode Validation Criteria

#### For Control Features (Closed-Loop, ET₀ Steering)
- [ ] **Median correction delta** ≤ 5% from manual operations
- [ ] **No safety interlock violations** during shadow period
- [ ] **System stability maintained** (no degraded SLOs)
- [ ] **Command latency** within SLO (p95 < 800ms)

#### For AI Features (Auto-Apply, Anomaly Detection)
- [ ] **Prediction accuracy** meets baseline threshold
- [ ] **False positive rate** < 5%
- [ ] **Confidence scores** calibrated and validated
- [ ] **No bias detected** in recommendations

#### Common Validation
- [ ] **Zero unhandled exceptions** in shadow mode
- [ ] **Dashboards show expected metrics**
- [ ] **Performance impact** negligible (< 5% latency increase)
- [ ] **Resource utilization** within capacity guardrails

### Shadow Mode Results
**Median Correction Delta**: `_____% ` (Target: ≤5%)  
**Safety Violations**: `_____` (Target: 0)  
**Unhandled Exceptions**: `_____` (Target: 0)  
**P95 Latency Impact**: `_____ms` (Target: < 40ms increase)  

**Summary**: `_______________________________________________________`

---

## 🧪 Stage 2: Staged Rollout (A/B Test)

Enable flag for subset of operations (e.g., 10% of irrigation runs, specific zones).

### Prerequisites
- [ ] **Shadow mode validated** - All criteria met for 14+ consecutive days
- [ ] **Staged rollout plan** documented (percentages, duration, success criteria)
- [ ] **Comparison metrics** defined (staged vs. control group)
- [ ] **Statistical significance** target defined (p < 0.05)
- [ ] **Emergency stop procedure** tested and validated

### Staged Execution
- [ ] **Enable date**: `___________`
- [ ] **Rollout percentage**: `_____%` (typically 10-25%)
- [ ] **Duration**: Minimum 7 days
- [ ] **Control group maintained** for comparison

### Staged Validation Criteria
- [ ] **Performance delta** within acceptable range (≤ ±3% from shadow predictions)
- [ ] **User feedback** positive (no operational complaints)
- [ ] **SLOs maintained** across all services
- [ ] **No regression** in control group metrics
- [ ] **Statistical significance** achieved for key metrics

### Staged Results
**Performance vs Shadow**: `_____% delta` (Target: ≤ ±3%)  
**User Complaints**: `_____` (Target: 0)  
**SLO Breaches**: `_____` (Target: 0)  
**Statistical Significance**: `p = _____` (Target: < 0.05)  

**Summary**: `_______________________________________________________`

---

## ✅ Stage 3: Full Enable

Enable flag for all operations at the site.

### Prerequisites
- [ ] **Staged rollout validated** - All criteria met for 7+ consecutive days
- [ ] **VP Product sign-off** obtained
- [ ] **SRE sign-off** obtained
- [ ] **On-call briefing** completed (within 24h of enable)
- [ ] **Rollback rehearsal** completed successfully
- [ ] **Communication plan** executed (notify affected users)

### Enable Execution
- [ ] **Enable date**: `___________`
- [ ] **Enable time**: `___________` (prefer low-traffic window)
- [ ] **Enabled by**: `___________` (name + role)
- [ ] **Rollback deadline**: `___________` (if issues detected)

### Post-Enable Monitoring (First 48 Hours)
- [ ] **Hour 1**: Check dashboards, no alerts fired
- [ ] **Hour 4**: Validate metrics within expected ranges
- [ ] **Hour 12**: Review user feedback, no complaints
- [ ] **Hour 24**: SLO compliance verified
- [ ] **Hour 48**: Feature considered stable

### Post-Enable Validation
- [ ] **Performance maintained** (no degradation vs. staged)
- [ ] **SLOs met** for 48 consecutive hours
- [ ] **Zero critical incidents** related to feature
- [ ] **User satisfaction** maintained (via feedback/surveys)

---

## 🚨 Rollback Criteria

**Immediate Rollback Required If:**
- Safety interlock violations detected
- SLO breach > 10% error budget consumed in < 1 hour
- Unhandled exceptions > 5 in 15 minutes
- User-reported critical incident (crop damage, regulatory risk)
- VP Product or SRE directs rollback

**Rollback Procedure:**
1. Set feature flag to `disabled` in Unleash (< 2 minutes)
2. Verify rollback via dashboard (< 5 minutes)
3. Post incident report in #incidents Slack channel
4. Schedule post-mortem within 24 hours

---

## 📊 Metrics Tracking

| Metric | Shadow | Staged | Enabled | Target |
|--------|--------|--------|---------|--------|
| Median Correction Delta | _____% | _____% | _____% | ≤5% |
| P95 Command Latency | _____ms | _____ms | _____ms | <800ms |
| Safety Violations | _____ | _____ | _____ | 0 |
| Unhandled Exceptions | _____ | _____ | _____ | 0 |
| User Complaints | _____ | _____ | _____ | 0 |
| SLO Compliance | _____% | _____% | _____% | 99.9% |

---

## ✍️ Sign-Off

### Shadow Mode Completion
- **Validated By**: `___________` (Engineering Lead)
- **Date**: `___________`
- **Signature**: `___________`

### Staged Rollout Completion
- **Validated By**: `___________` (Engineering Lead)
- **Date**: `___________`
- **Signature**: `___________`

### Full Enable Approval
- **VP Product**: `___________` Date: `___________`
- **SRE Lead**: `___________` Date: `___________`
- **Engineering Lead**: `___________` Date: `___________`

---

## 📚 Supporting Documents

- **PDD**: `[link to Product Design Document]`
- **Runbook**: `[link to operational runbook]`
- **ADR**: `[link to Architecture Decision Record if applicable]`
- **Dashboard**: `[link to Grafana dashboard]`
- **Incident Response**: `[link to incident playbook]`

---

## 📝 Notes & Observations

```
[Add any relevant notes, edge cases discovered, or lessons learned during promotion]









```

---

**✅ Track A Objective:** Safe, data-driven feature promotion with explicit validation gates and rollback readiness.
