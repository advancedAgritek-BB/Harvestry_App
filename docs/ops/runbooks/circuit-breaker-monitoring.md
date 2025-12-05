# Circuit Breaker Monitoring Runbook

## Overview

This runbook provides guidance for monitoring and troubleshooting Polly circuit breakers used in Harvestry services for external API resilience.

## Circuit Breaker Configurations

### Standard Resilience Pattern
Used for most external HTTP clients.

| Setting | Value | Description |
|---------|-------|-------------|
| Max Retries | 3 | Maximum retry attempts |
| Initial Delay | 500ms | Delay before first retry |
| Backoff | Exponential | Delay doubles each retry |
| Failure Ratio | 50% | Ratio to trip breaker |
| Min Throughput | 10 | Minimum calls before evaluation |
| Sampling Duration | 30s | Window for failure ratio |
| Break Duration | 30s | Time circuit stays open |
| Timeout | 10s | Per-request timeout |

### High-Throughput Pattern (Slack)
Used for high-volume integrations.

| Setting | Value |
|---------|-------|
| Max Retries | 2 |
| Initial Delay | 200ms |
| Failure Ratio | 30% |
| Break Duration | 15s |
| Timeout | 5s |

### Critical Operations Pattern
Used for compliance-critical integrations (METRC).

| Setting | Value |
|---------|-------|
| Max Retries | 5 |
| Initial Delay | 1s |
| Failure Ratio | 70% |
| Break Duration | 60s |
| Timeout | 30s |

---

## Monitoring Circuit Breaker State

### Prometheus Metrics

```promql
# Circuit breaker trip count
harvestry_integrations_circuit_breaker_trips_total

# External API call success rate
rate(harvestry_integrations_external_api_calls_total{success="true"}[5m])
/ rate(harvestry_integrations_external_api_calls_total[5m])

# Slack message delivery failure rate
rate(harvestry_integrations_slack_messages_failed_total[5m])
/ rate(harvestry_integrations_slack_messages_sent_total[5m])
```

### Grafana Alerts (Recommended)

```yaml
# Alert when circuit breaker trips
- alert: CircuitBreakerTripped
  expr: increase(harvestry_integrations_circuit_breaker_trips_total[5m]) > 0
  for: 0m
  labels:
    severity: warning
  annotations:
    summary: "Circuit breaker tripped for {{ $labels.integration }}"
    description: "The circuit breaker for {{ $labels.integration }} has tripped. External service may be unhealthy."

# Alert on sustained high failure rate
- alert: HighExternalApiFailureRate
  expr: |
    rate(harvestry_integrations_external_api_calls_total{success="false"}[5m])
    / rate(harvestry_integrations_external_api_calls_total[5m]) > 0.2
  for: 5m
  labels:
    severity: critical
  annotations:
    summary: "High failure rate for {{ $labels.integration }}"
```

---

## Troubleshooting Scenarios

### Scenario 1: Circuit Breaker Constantly Tripping

**Symptoms:**
- Frequent circuit breaker trips in logs
- Integration functionality degraded
- `harvestry_integrations_circuit_breaker_trips_total` increasing

**Diagnosis Steps:**

1. **Check external service health:**
   ```bash
   # For Slack
   curl -I https://slack.com/api/api.test
   
   # For METRC
   curl -I https://api-co.metrc.com/health
   ```

2. **Review recent integration errors:**
   ```bash
   docker logs harvestry-tasks-api 2>&1 | grep -i "slack\|circuit\|timeout"
   ```

3. **Check timeout configuration:**
   - Verify timeouts are appropriate for external service latency
   - Check if external service is experiencing slowdowns

**Resolution:**
- If external service is down: Wait for recovery, circuit will auto-heal
- If latency issues: Consider increasing timeout thresholds
- If persistent: Check network connectivity and DNS resolution

---

### Scenario 2: False Positive Trips

**Symptoms:**
- Circuit breaking despite external service being healthy
- Low actual failure rate but circuit trips frequently

**Diagnosis Steps:**

1. **Check failure ratio configuration:**
   - 50% failure ratio with min throughput 10 means 5+ failures in 30s trips

2. **Review what counts as failure:**
   - Network errors (HttpRequestException)
   - 5xx status codes
   - Timeouts

3. **Check for rate limiting (429):**
   ```bash
   docker logs harvestry-tasks-api 2>&1 | grep "429\|TooManyRequests"
   ```

**Resolution:**
- Increase `MinimumThroughput` to require more samples
- Increase `FailureRatio` threshold
- Add `Retry-After` handling for 429 responses

---

### Scenario 3: Circuit Breaker Not Closing

**Symptoms:**
- Circuit stays open longer than expected
- External service is recovered but requests still failing

**Diagnosis Steps:**

1. **Verify break duration:**
   - Default is 30 seconds
   - Check if configuration was overridden

2. **Check for continued failures during half-open state:**
   - After break duration, one probe request is allowed
   - If that fails, circuit reopens

**Resolution:**
- Ensure probe request targets a stable endpoint
- Consider implementing a dedicated health check endpoint for probing

---

## Manual Intervention

### Force Circuit State (Development Only)

In development/testing, you can restart the service to reset circuit state:

```bash
docker restart harvestry-tasks-api
```

**Note:** This should not be used in production. Let the circuit breaker self-heal.

---

## Logging Reference

### Log Messages to Monitor

```
[WARN] Circuit breaker 'SlackApiClient' is now OPEN due to failure threshold
[INFO] Circuit breaker 'SlackApiClient' transitioning to HALF-OPEN
[INFO] Circuit breaker 'SlackApiClient' is now CLOSED
[ERROR] Request to Slack API failed after 3 retries
```

### Structured Log Fields

| Field | Description |
|-------|-------------|
| `CircuitState` | Current circuit state (Closed, Open, HalfOpen) |
| `FailureCount` | Number of failures in current window |
| `LastException` | Exception type that caused failure |
| `RetryCount` | Number of retries attempted |

---

## Escalation

If circuit breaker issues persist:

1. Collect data:
   - Time range of issue
   - Circuit breaker metrics
   - External service status page

2. Check external service status pages:
   - Slack: https://status.slack.com
   - METRC: Contact METRC support

3. Escalate to Platform Engineering with collected data

---

## Configuration Reference

To modify circuit breaker settings, update the service's `appsettings.json`:

```json
{
  "Resilience": {
    "StandardPolicy": {
      "RetryCount": 3,
      "RetryDelay": 500,
      "FailureRatio": 0.5,
      "BreakDuration": 30,
      "Timeout": 10
    }
  }
}
```

Or use the shared utilities extension:

```csharp
builder.Services.AddHttpClient("SlackApi")
    .AddHarvestryResilience();
```

