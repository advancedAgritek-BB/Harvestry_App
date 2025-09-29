# Harvestry ERP - Docker Development Environment

**Track A Implementation** - Local development infrastructure with full observability stack

## 🎯 Overview

This Docker Compose environment provides all the infrastructure services needed for local Harvestry ERP development:

- **PostgreSQL 15 + TimescaleDB** - Primary datastore with time-series extensions
- **Redis** - Caching and session store
- **Unleash** - Feature flag management (site-scoped toggles)
- **Prometheus** - Metrics collection and SLO monitoring
- **Grafana** - Dashboards and visualization
- **Loki** - Log aggregation
- **Tempo** - Distributed tracing
- **Jaeger** - Trace visualization
- **OpenTelemetry Collector** - Unified telemetry pipeline

---

## 🚀 Quick Start

### Prerequisites

- Docker 20.10+
- Docker Compose 2.0+
- At least 8GB RAM allocated to Docker
- 20GB free disk space

### Start All Services

```bash
# Start entire stack
docker compose up -d

# Check service health
docker compose ps

# View logs
docker compose logs -f

# View specific service logs
docker compose logs -f postgres
docker compose logs -f grafana
```

### Stop Services

```bash
# Stop all services
docker compose down

# Stop and remove volumes (⚠️ deletes all data)
docker compose down -v
```

---

## 📊 Service Access

Once running, access services at:

| Service | URL | Credentials |
|---------|-----|-------------|
| **Grafana** | <http://localhost:3001> | admin / admin |
| **Prometheus** | <http://localhost:9090> | N/A |
| **Unleash** | <http://localhost:4242> | admin / unleash4all |
| **Jaeger** | <http://localhost:16686> | N/A |
| **PostgreSQL** | localhost:5432 | harvestry_user / harvestry_dev_password |
| **Redis** | localhost:6379 | N/A |

---

## 🗄️ Database Setup

### Connect to PostgreSQL

```bash
# Using psql (if installed locally)
psql -h localhost -p 5432 -U harvestry_user -d harvestry_dev

# Using Docker
docker compose exec postgres psql -U harvestry_user -d harvestry_dev
```

### Run Migrations

```bash
# Apply all migrations
./scripts/db/migrate.sh

# Seed development data
./scripts/db/seed.sh
```

### Verify TimescaleDB

```sql
-- Check TimescaleDB version
SELECT extversion FROM pg_extension WHERE extname = 'timescaledb';

-- List hypertables (after migrations)
SELECT * FROM timescaledb_information.hypertables;

-- List continuous aggregates
SELECT * FROM timescaledb_information.continuous_aggregates;
```

---

## 📈 Observability Stack

### Prometheus

**Metrics Collection & Alerting**

- Access: <http://localhost:9090>
- Configuration: `src/infrastructure/monitoring/prometheus/prometheus.yml`
- Alerts: `src/infrastructure/monitoring/prometheus/alerts/`

**Useful Queries:**

```promql
# API request rate
rate(http_requests_total[5m])

# P95 latency
histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))

# Error rate
rate(http_request_errors_total[5m]) / rate(http_requests_total[5m])
```

### Grafana

**Dashboards & Visualization**

- Access: <http://localhost:3001>
- Default credentials: admin / admin
- Dashboards location: `src/infrastructure/monitoring/grafana/dashboards/`

**Pre-configured Dashboards:**

- API Gateway Golden Signals
- Telemetry Ingest Performance
- Database Performance (TimescaleDB)
- SLO Burn Rate Monitoring
- Queue Depth & Outbox Status

### Loki

**Log Aggregation**

- Access: <http://localhost:3100>
- Query via Grafana Explore
- Configuration: `src/infrastructure/logging/loki/loki-config.yml`

**Query Examples (LogQL):**

```logql
# All logs for a service
{service_name="api-gateway"}

# Errors only
{service_name="api-gateway"} |= "ERROR"

# Slow queries
{service_name="api-gateway"} | json | duration > 1s
```

### Tempo

**Distributed Tracing**

- Access: <http://localhost:3200>
- View traces via Grafana or Jaeger
- Configuration: `src/infrastructure/tracing/tempo/tempo-config.yml`

### Jaeger

**Trace Visualization**

- Access: <http://localhost:16686>
- View detailed trace spans
- Search by service, operation, tags

---

## 🚩 Feature Flags (Unleash)

### Access Unleash

- URL: <http://localhost:4242>
- Username: admin
- Password: unleash4all

### Create Site-Scoped Flags

```bash
# Example: Enable closed-loop control for a site
curl -X POST http://localhost:4242/api/admin/projects/default/features \
  -H "Authorization: *:*.unleash-insecure-api-token" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "closed_loop_ecph_enabled",
    "description": "Enable closed-loop EC/pH control",
    "type": "release",
    "impressionData": true
  }'

# Add site-specific strategy
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
    }
  }'
```

### Risky Feature Flags (Require Promotion Checklist)

Per Track A design, these flags require PDD + Runbook links in PRs:

- `closed_loop_ecph_enabled`
- `ai_auto_apply_enabled`
- `et0_steering_enabled`
- `sms_critical_enabled`
- `slack_mirror_mode`

---

## 🔧 Troubleshooting

### Services Won't Start

```bash
# Check Docker resources
docker system df

# Check logs for errors
docker compose logs

# Restart specific service
docker compose restart postgres
```

### Database Connection Issues

```bash
# Check PostgreSQL is healthy
docker compose exec postgres pg_isready -U harvestry_user

# View PostgreSQL logs
docker compose logs postgres

# Reset database (⚠️ deletes all data)
# Option 1: Stop PostgreSQL service and remove its volumes
docker compose stop postgres
docker compose rm -s -v postgres
docker compose up -d postgres

# Option 2: Bring down entire stack and remove all volumes
# docker compose down -v
```

### Prometheus Not Scraping Metrics

```bash
# Check Prometheus targets
curl http://localhost:9090/api/v1/targets

# View Prometheus logs
docker compose logs prometheus

# Verify service is exposing /metrics endpoint
curl http://localhost:5000/metrics
```

### Grafana Dashboards Not Loading

```bash
# Check Grafana logs
docker compose logs grafana

# Verify Prometheus datasource
curl -u admin:admin http://localhost:3001/api/datasources

# Re-provision dashboards
docker compose restart grafana
```

---

## 🧪 Testing SLO Monitoring

### Simulate Fast Burn

```bash
# Generate errors to trigger fast burn alert
for i in {1..1000}; do
  curl -X GET http://localhost:5000/api/test/error
  sleep 0.1
done

# Check Prometheus alerts
curl http://localhost:9090/api/v1/alerts | jq '.data.alerts[] | select(.labels.alert=="APIGatewayFastBurn")'
```

### Simulate Slow Requests

```bash
# Generate slow requests
for i in {1..100}; do
  curl -X GET "http://localhost:5000/api/test/slow?delay=2000"
  sleep 1
done

# Query p95 latency
curl -G http://localhost:9090/api/v1/query \
  --data-urlencode 'query=histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))'
```

---

## 📦 Data Persistence

All data is stored in Docker volumes:

```bash
# List volumes
docker volume ls | grep harvestry

# Inspect volume
docker volume inspect harvestry_app_postgres-data

# Backup database
docker compose exec postgres pg_dump -U harvestry_user harvestry_dev > backup.sql

# Restore database
docker compose exec -T postgres psql -U harvestry_user harvestry_dev < backup.sql
```

---

## 🔄 Updating Services

```bash
# Pull latest images
docker compose pull

# Recreate services with new images
docker compose up -d --force-recreate

# Prune old images
docker image prune -a
```

---

## 📚 Additional Resources

- [Prometheus Query Documentation](https://prometheus.io/docs/prometheus/latest/querying/basics/)
- [Grafana Dashboard Guide](https://grafana.com/docs/grafana/latest/dashboards/)
- [Loki LogQL Reference](https://grafana.com/docs/loki/latest/logql/)
- [OpenTelemetry Instrumentation](https://opentelemetry.io/docs/instrumentation/)
- [Unleash Feature Flags](https://docs.getunleash.io/)

---

## 🆘 Support

For issues with the Docker environment:

1. Check service health: `docker compose ps`
2. View logs: `docker compose logs [service-name]`
3. Restart services: `docker compose restart`
4. Full reset: `docker compose down -v && docker compose up -d`

For development questions, reach out to **@harvestry/devops-sre-security** on Slack.

---

**✅ Track A Objective:** Enable local development with full observability stack ready for SLO validation.
