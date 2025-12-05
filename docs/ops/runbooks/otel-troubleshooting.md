# OpenTelemetry Troubleshooting Runbook

## Overview

This runbook provides guidance for troubleshooting OpenTelemetry instrumentation issues across Harvestry services.

## Quick Reference

| Component | Port | Endpoint |
|-----------|------|----------|
| OTel Collector OTLP gRPC | 4317 | http://otel-collector:4317 |
| OTel Collector OTLP HTTP | 4318 | http://otel-collector:4318 |
| Jaeger Query | 16686 | http://jaeger:16686 |
| Prometheus | 9090 | http://prometheus:9090 |
| Grafana | 3000 | http://grafana:3000 |

---

## Common Issues

### Issue 1: No Traces Appearing in Jaeger

**Symptoms:**
- Service is running but no traces visible in Jaeger UI
- No spans in trace search

**Diagnosis Steps:**

1. **Verify OTel configuration in service:**
   ```bash
   # Check appsettings.json or environment variables
   grep -r "OpenTelemetry" src/backend/services/*/API/appsettings*.json
   ```

2. **Verify collector is running:**
   ```bash
   docker ps | grep otel-collector
   curl -v http://localhost:4317/health
   ```

3. **Check service logs for OTel initialization:**
   ```bash
   docker logs harvestry-identity-api 2>&1 | grep -i "opentelemetry\|otel\|trace"
   ```

4. **Verify endpoint connectivity:**
   ```bash
   curl -v telnet://otel-collector:4317
   ```

**Resolution:**
- Ensure `OpenTelemetry:Endpoint` is correctly configured
- Verify network connectivity between service and collector
- Check for firewall rules blocking port 4317

---

### Issue 2: Missing Custom Spans

**Symptoms:**
- Automatic ASP.NET Core spans appear but custom service spans are missing

**Diagnosis Steps:**

1. **Verify ActivitySource registration:**
   ```csharp
   // Check that service uses correct ActivitySource name
   services.AddOpenTelemetry()
       .WithTracing(tracing => tracing
           .AddSource("Harvestry.*")  // Wildcard must match
           .AddSource(ActivitySources.IdentityServiceName));
   ```

2. **Check span creation code:**
   ```csharp
   // Ensure using correctly named ActivitySource
   using var activity = ActivitySources.Identity.StartActivity("OperationName");
   ```

**Resolution:**
- Ensure ActivitySource name matches the pattern in `AddSource()`
- Verify `using` statement properly disposes the Activity

---

### Issue 3: High Trace Latency / Volume

**Symptoms:**
- Service performance degraded
- High memory usage in OTel collector

**Diagnosis Steps:**

1. **Check trace sampling configuration:**
   ```yaml
   # otel-collector-config.yaml
   processors:
     probabilistic_sampler:
       sampling_percentage: 10  # 10% sampling
   ```

2. **Monitor collector metrics:**
   ```bash
   curl http://localhost:8888/metrics | grep otelcol
   ```

**Resolution:**
- Implement sampling (10% for normal traffic, 100% for errors)
- Filter health check endpoints from tracing
- Increase collector resources if needed

---

## Metrics Troubleshooting

### Issue: Metrics Not Appearing in Prometheus

**Diagnosis:**

1. **Verify metrics endpoint:**
   ```bash
   curl http://localhost:5000/metrics
   ```

2. **Check Prometheus targets:**
   - Navigate to Prometheus UI → Status → Targets
   - Verify service endpoint is "UP"

3. **Query metrics directly:**
   ```promql
   harvestry_telemetry_ingest_count_total
   ```

**Resolution:**
- Ensure OTLP metrics exporter is configured
- Verify Prometheus scrape config includes service

---

## Useful Commands

### View Active Spans
```bash
# Query Jaeger for recent traces
curl "http://localhost:16686/api/traces?service=Harvestry.Identity&limit=10"
```

### Export Trace Data
```bash
# Export traces for a specific trace ID
curl "http://localhost:16686/api/traces/{trace-id}" > trace-export.json
```

### Check OTel Collector Health
```bash
curl http://localhost:13133/health
curl http://localhost:8888/metrics
```

### View Service Metrics
```bash
# Query Prometheus for Harvestry metrics
curl 'http://localhost:9090/api/v1/query?query=harvestry_auth_badge_login_success_total'
```

---

## Escalation

If issues persist after following this runbook:

1. Collect diagnostic information:
   - Service logs (last 1 hour)
   - OTel collector logs
   - `docker inspect` output for networking issues

2. Escalate to Platform Engineering with:
   - Time range of issue
   - Affected services
   - Steps already attempted

---

## Related Resources

- [OpenTelemetry .NET SDK Documentation](https://opentelemetry.io/docs/instrumentation/net/)
- [Jaeger Troubleshooting Guide](https://www.jaegertracing.io/docs/latest/troubleshooting/)
- Internal: `/docs/infra/otel-collector-config.yaml`

