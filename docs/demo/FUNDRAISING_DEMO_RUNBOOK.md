# Fundraising Demo Runbook

**Version:** 1.0  
**Date:** December 2024  
**Audience:** Demo facilitators and stakeholders

---

## Executive Summary

This runbook provides step-by-step instructions for demonstrating Harvestry's capabilities during fundraising presentations. Each demo scenario aligns with specific prospectus claims.

---

## Pre-Demo Checklist

### Environment Setup
- [ ] Staging environment is running and healthy
- [ ] Demo data has been seeded (plants, packages, sensors)
- [ ] METRC sandbox credentials are configured
- [ ] Telemetry streams are actively ingesting data
- [ ] Grafana dashboards are accessible

### Access Verification
- [ ] Demo user account created with appropriate permissions
- [ ] API endpoints are responding (health check)
- [ ] WebSocket/SignalR connection is stable
- [ ] Network connectivity confirmed for all demo components

---

## Demo Scenario 1: Real-Time Telemetry (p95 < 1.0s)

**Prospectus Claim:** "Telemetry Ingest (device → store) p95 < 1.0s"

### Steps

1. **Navigate to Live Dashboard**
   ```
   URL: https://staging.harvestry.io/dashboard/telemetry
   ```

2. **Show Real-Time Data Stream**
   - Point to live updating sensor values
   - Highlight refresh rates and timestamps
   - Show multiple sensor types (temp, humidity, EC, pH)

3. **Demonstrate SLO Metrics**
   ```
   URL: https://staging.harvestry.io/grafana/d/telemetry-slo
   ```
   - Show p95 latency graph (target: < 1000ms)
   - Show ingest throughput (target: 10k msg/s sustained)
   - Show error rate (target: < 1%)

4. **Talking Points**
   - "Data from edge devices hits our database in under one second"
   - "This enables real-time decision making and alerting"
   - "We've validated this at 10,000 messages per second sustained load"

---

## Demo Scenario 2: METRC Compliance Sync

**Prospectus Claim:** "99% uptime auto-sync with METRC"

### Steps

1. **Navigate to Compliance Dashboard**
   ```
   URL: https://staging.harvestry.io/compliance/metrc
   ```

2. **Show License Configuration**
   - Display configured license (CO-DEMO-12345)
   - Show sync status and last successful sync
   - Highlight auto-sync interval setting

3. **Trigger Manual Sync (Live Demo)**
   ```bash
   # API call to start sync
   POST /api/v1/metrc/sync/start
   {
     "siteId": "demo-site-uuid",
     "licenseNumber": "CO-DEMO-12345",
     "direction": "Bidirectional"
   }
   ```

4. **Show Sync Progress**
   - Navigate to sync job details
   - Show items being processed
   - Highlight retry logic and error handling

5. **Demonstrate Reconciliation**
   ```
   URL: https://staging.harvestry.io/compliance/metrc/reconciliation
   ```
   - Show comparison between Harvestry and METRC data
   - Highlight any discrepancies found
   - Show recommended actions

6. **Talking Points**
   - "We sync automatically every 15 minutes, or on-demand"
   - "Our outbox pattern ensures no data is lost, even during API outages"
   - "Reconciliation catches discrepancies before they become compliance issues"

---

## Demo Scenario 3: Irrigation Safety Interlocks

**Prospectus Claim:** "Precision irrigation with safety-first automation"

### Steps

1. **Navigate to Irrigation Dashboard**
   ```
   URL: https://staging.harvestry.io/irrigation/groups
   ```

2. **Show Irrigation Group Configuration**
   - Display zones and valve assignments
   - Show max concurrent valve setting
   - Highlight pump association

3. **Demonstrate Interlock System**
   ```
   URL: https://staging.harvestry.io/irrigation/interlocks
   ```
   - Show 7 interlock types:
     - E-Stop
     - Door Open
     - Tank Level Low
     - EC Out of Bounds
     - pH Out of Bounds
     - CO₂ Lockout
     - Max Runtime

4. **Trigger Test Interlock (Optional)**
   - Simulate door open condition
   - Show irrigation run being blocked
   - Show interlock event recorded

5. **Talking Points**
   - "Safety is our top priority - 7 hardware interlocks prevent unsafe operation"
   - "Every interlock trip is logged for compliance and analysis"
   - "The system fails safe - valves close automatically on any interlock"

---

## Demo Scenario 4: Anomaly Detection

**Prospectus Claim:** "AI-powered anomaly detection with actionable recommendations"

### Steps

1. **Navigate to Anomaly Dashboard**
   ```
   URL: https://staging.harvestry.io/analytics/anomalies
   ```

2. **Generate Site Report**
   ```bash
   GET /api/v1/telemetry/anomalies/site/{siteId}/report
   ```

3. **Show Detected Anomalies**
   - Temperature spikes
   - Humidity drifts
   - EC variability

4. **Highlight Recommendations**
   - Show actionable recommendations
   - Demonstrate severity prioritization
   - Show impact assessments

5. **Talking Points**
   - "Our statistical anomaly detection catches problems before they become disasters"
   - "Each anomaly comes with a specific, actionable recommendation"
   - "This reduces crop loss by catching issues early"

---

## Demo Scenario 5: QuickBooks Integration (Stretch)

**Prospectus Claim:** "Real-time financial sync with QuickBooks"

### Steps

1. **Show OAuth Connection Flow**
   - Navigate to integrations settings
   - Show connected QuickBooks account
   - Display sync status

2. **Demonstrate Item Sync**
   - Show Harvestry item mapped to QuickBooks item
   - Display sync timestamps

3. **Talking Points**
   - "Financial data flows automatically to QuickBooks"
   - "No manual data entry means fewer errors and faster books close"

---

## Troubleshooting

### Common Issues

| Issue | Resolution |
|-------|------------|
| Telemetry not updating | Check WebSocket connection in browser dev tools |
| METRC sync failing | Verify sandbox credentials haven't expired |
| Grafana not loading | Check Grafana service status and VPN connection |
| Slow dashboard | Clear browser cache and reload |

### Emergency Contacts

- **Technical Lead:** [Contact Info]
- **DevOps On-Call:** [Contact Info]

---

## Post-Demo Checklist

- [ ] Answer any questions from stakeholders
- [ ] Note any feedback or concerns raised
- [ ] Document any issues encountered
- [ ] Reset demo data if needed for next presentation

---

## Appendix: API Quick Reference

### Health Check
```bash
curl https://staging.harvestry.io/api/health
```

### Telemetry Ingest (Manual Test)
```bash
curl -X POST https://staging.harvestry.io/api/v1/telemetry/ingest \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"readings": [...]}'
```

### METRC Sync Status
```bash
curl https://staging.harvestry.io/api/v1/metrc/sync/status/{licenseId} \
  -H "Authorization: Bearer $TOKEN"
```

### Anomaly Report
```bash
curl https://staging.harvestry.io/api/v1/telemetry/anomalies/site/{siteId}/report \
  -H "Authorization: Bearer $TOKEN"
```
