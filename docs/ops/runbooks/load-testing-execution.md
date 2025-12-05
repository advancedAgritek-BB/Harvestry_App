# Load Testing Execution Runbook

## Overview

This runbook provides step-by-step instructions for executing load tests against Harvestry services using k6.

## Prerequisites

- k6 installed: `brew install k6` (macOS) or [k6.io/docs/getting-started/installation](https://k6.io/docs/getting-started/installation/)
- Target environment running and accessible
- Database seeded with test data (`./scripts/seed/run-seed.sh dev`)
- Access to monitoring dashboards (Grafana/Prometheus)

---

## Quick Start

```bash
# Navigate to load tests directory
cd tests/load

# Run baseline test (5 min, low load)
./run-load-tests.sh baseline

# Run sustained test (15 min, target load)
./run-load-tests.sh sustained --api-url http://localhost:5000

# Run stress test (increasing load until failure)
./run-load-tests.sh stress
```

---

## Test Types

### 1. Baseline Test
**Purpose:** Validate basic performance and establish baseline metrics.

| Parameter | Value |
|-----------|-------|
| Duration | 5 minutes |
| Virtual Users | 10 |
| Target | System functional under light load |

```bash
./run-load-tests.sh baseline
```

**Expected Results:**
- p95 latency < 500ms
- Error rate < 0.1%
- All requests complete successfully

---

### 2. Sustained Load Test
**Purpose:** Validate SLO compliance under expected production load.

| Parameter | Value |
|-----------|-------|
| Duration | 15 minutes |
| Virtual Users | 50 |
| Target | p95 < 1000ms, 10k msg/s |

```bash
./run-load-tests.sh sustained
```

**Expected Results:**
- p95 latency < 1000ms
- Error rate < 0.5%
- Consistent throughput throughout test

---

### 3. Stress Test
**Purpose:** Find the breaking point and validate graceful degradation.

| Parameter | Value |
|-----------|-------|
| Ramp up | 2 min to 50 VUs |
| Hold 1 | 5 min at 100 VUs |
| Ramp 2 | 2 min to 150 VUs |
| Hold 2 | 5 min at 150 VUs |
| Ramp down | 2 min to 0 VUs |

```bash
./run-load-tests.sh stress
```

**Expected Results:**
- System degrades gracefully under overload
- No crashes or data corruption
- Recovery when load decreases

---

## Pre-Test Checklist

- [ ] Target environment is stable (no ongoing deployments)
- [ ] Database has sufficient seed data
- [ ] Monitoring dashboards are accessible
- [ ] Alert thresholds are adjusted for test traffic
- [ ] Team is notified of upcoming load test

---

## Test Execution Procedure

### Step 1: Environment Preparation

```bash
# 1. Verify target services are running
curl http://localhost:5000/health/ready

# 2. Check database connectivity
psql $DATABASE_URL -c "SELECT COUNT(*) FROM sensor_streams;"

# 3. Verify seed data exists
psql $DATABASE_URL -c "SELECT COUNT(*) FROM sites;"
```

### Step 2: Start Monitoring

1. Open Grafana dashboard: http://localhost:3000
2. Navigate to "Harvestry Overview" dashboard
3. Set time range to last 30 minutes with 5s refresh

### Step 3: Execute Test

```bash
# Set environment variables
export API_URL=http://localhost:5000
export API_TOKEN=your-test-token

# Run the test
./run-load-tests.sh sustained --api-url $API_URL
```

### Step 4: Monitor During Test

Watch these key metrics:

| Metric | Target | Action if Exceeded |
|--------|--------|-------------------|
| HTTP p95 | < 1000ms | Investigate slow queries |
| Error rate | < 0.5% | Check error logs |
| CPU usage | < 80% | Scale horizontally |
| Memory | < 85% | Check for leaks |
| DB connections | < pool max | Increase pool size |

### Step 5: Post-Test Analysis

```bash
# Results are saved to tests/load/results/
ls -la tests/load/results/

# View latest summary
cat tests/load/results/*_summary.json | jq

# Generate report
jq -r '.metrics | to_entries[] | "\(.key): \(.value.values)"' \
  tests/load/results/*_summary.json
```

---

## Analyzing Results

### Key Metrics to Review

| Metric | Description | SLO |
|--------|-------------|-----|
| `http_req_duration{p95}` | 95th percentile latency | < 1000ms |
| `http_req_failed` | Request failure rate | < 0.5% |
| `iterations` | Total completed iterations | Target throughput |
| `vus` | Virtual users active | As configured |

### Sample Analysis Query

```bash
# Parse summary JSON
jq -r '
  "HTTP Request Duration (p95): " + (.metrics.http_req_duration.values."p(95)" | tostring) + "ms",
  "HTTP Request Failed Rate: " + (.metrics.http_req_failed.values.rate | tostring),
  "Total Iterations: " + (.metrics.iterations.values.count | tostring)
' tests/load/results/*_summary.json
```

---

## Common Issues

### Issue: Rate Limiting Triggered

**Symptoms:**
- High 429 response rate
- Lower throughput than expected

**Resolution:**
- This is expected behavior - system is protecting itself
- Adjust test to stay within rate limits
- Or temporarily increase limits for testing

### Issue: Connection Pool Exhaustion

**Symptoms:**
- Increasing response times
- Connection timeout errors

**Resolution:**
- Increase database connection pool size
- Check for connection leaks
- Add connection pool metrics monitoring

### Issue: Memory Growth During Test

**Symptoms:**
- Memory usage steadily increasing
- OOM errors at high load

**Resolution:**
- Check for memory leaks in application
- Review buffer/queue sizes
- Analyze memory dumps if persistent

---

## SLO Validation

After test completion, validate against SLOs:

```bash
#!/bin/bash
# validate-slo.sh

SUMMARY_FILE=$(ls -t tests/load/results/*_summary.json | head -1)

P95=$(jq -r '.metrics.http_req_duration.values."p(95)"' "$SUMMARY_FILE")
FAIL_RATE=$(jq -r '.metrics.http_req_failed.values.rate' "$SUMMARY_FILE")

echo "=== SLO Validation ==="

if (( $(echo "$P95 < 1000" | bc -l) )); then
  echo "✓ PASS: p95 latency ($P95 ms) < 1000ms"
else
  echo "✗ FAIL: p95 latency ($P95 ms) >= 1000ms"
fi

if (( $(echo "$FAIL_RATE < 0.005" | bc -l) )); then
  echo "✓ PASS: Error rate ($FAIL_RATE) < 0.5%"
else
  echo "✗ FAIL: Error rate ($FAIL_RATE) >= 0.5%"
fi
```

---

## Environment-Specific Settings

### Development
```bash
./run-load-tests.sh baseline --api-url http://localhost:5000
```

### Staging
```bash
./run-load-tests.sh sustained --api-url https://staging-api.harvestry.io
```

### Production (Canary Only)
```bash
# CAUTION: Production load tests require approval
./run-load-tests.sh baseline --api-url https://canary.harvestry.io
```

---

## Escalation

If load test reveals performance issues:

1. Document findings:
   - Test type and configuration
   - Key metrics that failed SLO
   - Relevant dashboard screenshots

2. File performance issue with:
   - Test results summary
   - Affected services
   - Recommended investigation areas

3. Schedule performance review meeting if critical

