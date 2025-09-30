# Hybrid Database Architecture — Supabase + Timescale Cloud + ClickHouse

**Version:** 1.0  
**Date:** 2025-09-29  
**Status:** ✅ Approved Architecture  
**Reason:** Supabase deprecated TimescaleDB in PostgreSQL 17

---

## Executive Summary

**Problem:** Supabase no longer supports TimescaleDB extension, which we need for telemetry time-series data.

**Solution:** Hybrid multi-database architecture:

| Database | Purpose | Data Types | Cost |
|----------|---------|-----------|------|
| **Supabase** | Relational OLTP | Identity, Spatial, Tasks, Inventory, Processing, Compliance, QBO | Free tier OK |
| **Timescale Cloud** | Time-series | Telemetry (sensor_readings), Alerts, Irrigation runs, Task events | **Free <1GB** |
| **ClickHouse** | OLAP Analytics | Materialized aggregates (on triggers from Timescale) | Self-hosted free |

**Benefits:**
- ✅ Best-in-class tools for each workload
- ✅ Free tier covers MVP (<1GB telemetry)
- ✅ No vendor lock-in
- ✅ Meets all acceptance criteria
- ✅ Scales to production with minimal changes

---

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                         Application Layer                        │
│  .NET Services + Next.js Frontend + Background Workers          │
└─────────────────────────────────────────────────────────────────┘
                           │
           ┌───────────────┼───────────────┐
           ▼               ▼               ▼
    ┌──────────┐    ┌─────────────┐    ┌──────────┐
    │ Supabase │    │  Timescale  │    │ClickHouse│
    │PostgreSQL│    │    Cloud    │    │  (OLAP)  │
    │  (OLTP)  │    │(Time-Series)│    │          │
    └──────────┘    └─────────────┘    └──────────┘
         │                  │                  │
         │                  │                  │
    Relational         Telemetry          Aggregates
    - users            - sensor_readings  - hourly_rollups
    - sites            - alerts           - site_summaries
    - tasks            - irrigation_runs  - compliance_reports
    - inventory        - task_events      
    - batches          
    - compliance       
    - qbo_sync         

            Trigger-based sync (optional for PRD)
            Timescale → ClickHouse (async)
```

---

## Data Routing Strategy

### Supabase (Relational OLTP)

**Tables:**
- **Identity:** `users`, `roles`, `user_sites`, `user_roles`, `badges`, `device_sessions`, `sops`, `trainings`
- **Spatial:** `sites`, `rooms`, `zones`, `racks`, `bins`, `equipment_registry`
- **Tasks:** `tasks`, `task_dependencies`, `conversations`, `messages`
- **Lifecycle:** `batches`, `plant_movements`, `genetics`, `mother_plants`
- **Inventory:** `inventory_lots`, `inventory_balances`, `inventory_movements`, `lot_relationships`
- **Processing:** `process_definitions`, `process_runs`, `labor_logs`, `waste_events`
- **Compliance:** `compliance_integrations`, `sync_queue`, `labs`, `lab_results`, `holds`, `destruction_events`
- **Accounting:** `accounting_integrations`, `qbo_*_map`, `accounting_queue`
- **Audit:** `audit_trail`, `audit_verification_log`
- **Queues:** `outbox`, `sync_queue`, `accounting_queue`

**Total:** ~50 tables, mostly low-volume transactional data

---

### Timescale Cloud (Time-Series)

**Hypertables:**
- `sensor_readings` (climate, substrate, nutrients)
- `sensor_readings_1m` (continuous aggregate)
- `sensor_readings_5m` (continuous aggregate)
- `sensor_readings_1h` (continuous aggregate)
- `alerts` (real-time alert instances)
- `irrigation_step_runs` (step-level execution logs)
- `task_events` (task state changes)
- `control_loop_outputs` (closed-loop EC/pH adjustments)

**Compression:**
- Raw data: 7 days (then compress)
- Rollups: 90 days (1m), 730 days (5m), 3650 days (1h)

**Expected Volume (MVP):**
- 10 devices × 10 sensors × 10s interval = 100 points/sec = 8.6M points/day
- ~500MB/day raw → compressed to ~50MB/day
- **Free tier (<1GB) covers ~20 days of raw data + all rollups**

---

### ClickHouse (OLAP - Optional for MVP, Required for PRD Triggers)

**Materialized Views (async from Timescale):**
- `site_daily_summary` (DLI, VPD, irrigation, alerts)
- `compliance_reports` (COA status, destruction logs, audit exports)
- `sustainability_metrics` (WUE, NUE, kWh, CO₂)
- `yield_analytics` (g/W, throughput, labor efficiency)

**Trigger:** When telemetry volume > 10M points/day OR query p95 > 5s on Timescale → Enable ClickHouse sync

**MVP:** Not required; Timescale handles analytics  
**PRD:** Wired on triggers (documented in ADR-001)

---

## Setup Instructions

### Step 1: Set Up Supabase (Relational)

Follow existing guide but **skip TimescaleDB sections**:

```bash
# 1. Create Supabase project at app.supabase.com
# 2. Get credentials
# 3. Run relational migrations only

./scripts/db/migrate-supabase-relational.sh
```

**What gets migrated:**
- Baseline: audit_trail, outbox
- FRP-01: Identity tables
- FRP-02: Spatial tables
- FRP-04: Tasks tables
- FRP-07: Inventory tables
- FRP-08: Processing tables
- FRP-09: Compliance tables
- FRP-10: QBO tables

---

### Step 2: Set Up Timescale Cloud (Time-Series)

#### 2.1 Create Timescale Cloud Account

1. Go to [console.cloud.timescale.com](https://console.cloud.timescale.com)
2. Sign up (free tier available)
3. Click **"Create service"**

#### 2.2 Configure Service

- **Name:** `harvestry-telemetry-dev`
- **Plan:** **Free** (1GB storage, 1GB RAM)
- **Region:** Same as Supabase (e.g., `us-west-2`)
- **PostgreSQL Version:** 15
- **TimescaleDB Version:** 2.13+

#### 2.3 Get Connection String

After provisioning (~2 min):

```
DATABASE_URL_TIMESCALE=postgresql://tsdbadmin:[PASSWORD]@[HOST].tsdb.cloud.timescale.com:5432/tsdb?sslmode=require
```

#### 2.4 Run Timescale Migrations

```bash
# Add to .env.local
echo "DATABASE_URL_TIMESCALE=postgresql://tsdbadmin:[PASSWORD]@[HOST].tsdb.cloud.timescale.com:5432/tsdb?sslmode=require" >> .env.local

# Run time-series migrations
./scripts/db/migrate-timescale-cloud.sh
```

**What gets migrated:**
- Hypertables: `sensor_readings`, `alerts`, `irrigation_step_runs`, `task_events`
- Continuous aggregates: 1m/5m/1h rollups
- Compression policies
- Retention policies

---

### Step 3: Set Up ClickHouse (OLAP - Optional for MVP)

**MVP:** Skip this; enable on PRD triggers (when volume/latency hits thresholds)

**For PRD Setup:**

```bash
# Self-hosted ClickHouse via Docker
docker run -d \
  --name harvestry-clickhouse \
  -p 8123:8123 \
  -p 9000:9000 \
  -v harvestry_clickhouse_data:/var/lib/clickhouse \
  clickhouse/clickhouse-server:latest

# Add to .env.local
echo "CLICKHOUSE_URL=http://localhost:8123" >> .env.local
echo "CLICKHOUSE_DATABASE=harvestry" >> .env.local

# Run ClickHouse migrations (when ready)
./scripts/db/migrate-clickhouse.sh
```

**Trigger Logic:**
```sql
-- In Timescale: Create trigger to notify ClickHouse sync worker
CREATE OR REPLACE FUNCTION notify_clickhouse_sync()
RETURNS TRIGGER AS $$
BEGIN
    PERFORM pg_notify('clickhouse_sync', json_build_object(
        'table', TG_TABLE_NAME,
        'operation', TG_OP,
        'timestamp', NEW.timestamp
    )::text);
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER sensor_readings_clickhouse_sync
AFTER INSERT ON sensor_readings
FOR EACH ROW EXECUTE FUNCTION notify_clickhouse_sync();
```

**Background Worker:** Listens to `pg_notify`, batches inserts to ClickHouse every 60s

---

## Connection Configuration

### Updated .env.local

```bash
# ============================================================================
# Supabase (Relational OLTP)
# ============================================================================

# Pooler (for API requests)
DATABASE_URL=postgresql://postgres.your_project_ref:[PASSWORD]@aws-0-us-west-1.pooler.supabase.com:6543/postgres

# Direct (for migrations)
DATABASE_URL_DIRECT=postgresql://postgres:[PASSWORD]@db.your_project_ref.supabase.co:5432/postgres

SUPABASE_URL=https://your_project_ref.supabase.co
SUPABASE_ANON_KEY=your_anon_key
SUPABASE_SERVICE_ROLE_KEY=your_service_role_key

# ============================================================================
# Timescale Cloud (Time-Series)
# ============================================================================

DATABASE_URL_TIMESCALE=postgresql://tsdbadmin:[PASSWORD]@[HOST].tsdb.cloud.timescale.com:5432/tsdb?sslmode=require

# For connection pooling (PgBouncer)
DATABASE_URL_TIMESCALE_POOLER=postgresql://tsdbadmin:[PASSWORD]@[HOST].pooler.tsdb.cloud.timescale.com:5432/tsdb?sslmode=require

# ============================================================================
# ClickHouse (OLAP - Optional)
# ============================================================================

CLICKHOUSE_URL=http://localhost:8123
CLICKHOUSE_DATABASE=harvestry
CLICKHOUSE_USER=default
CLICKHOUSE_PASSWORD=

# Enable ClickHouse sync when triggers hit
CLICKHOUSE_SYNC_ENABLED=false  # Set to true when needed
```

---

## Application Configuration

### DbContext Setup (.NET)

```csharp
// Startup.cs or Program.cs

// Supabase (Relational)
services.AddDbContext<SupabaseDbContext>(options =>
    options.UseNpgsql(
        configuration.GetConnectionString("DATABASE_URL"),
        npgsqlOptions => npgsqlOptions
            .EnableRetryOnFailure(maxRetryCount: 3)
            .MigrationsHistoryTable("__EFMigrationsHistory", "public")
    ));

// Timescale Cloud (Time-Series)
services.AddDbContext<TimescaleDbContext>(options =>
    options.UseNpgsql(
        configuration.GetConnectionString("DATABASE_URL_TIMESCALE"),
        npgsqlOptions => npgsqlOptions
            .EnableRetryOnFailure(maxRetryCount: 3)
            .CommandTimeout(30)
    ));

// Repository pattern routes to correct context
services.AddScoped<IUserRepository, UserRepository>(); // → SupabaseDbContext
services.AddScoped<ITelemetryRepository, TelemetryRepository>(); // → TimescaleDbContext
```

### Connection Pooling

**Supabase:**
- Use pooler for API requests: `pooler.supabase.com:6543`
- Use direct for migrations: `db.xxx.supabase.co:5432`

**Timescale:**
- Use pooler for API requests: `pooler.tsdb.cloud.timescale.com:5432`
- Use direct for migrations: `xxx.tsdb.cloud.timescale.com:5432`

---

## Migration Scripts (Updated)

### scripts/db/migrate-supabase-relational.sh

```bash
#!/bin/bash
# Migrate relational tables to Supabase

set -e

echo "Migrating relational tables to Supabase..."

psql "$DATABASE_URL_DIRECT" << 'SQL'
-- Baseline
\i src/database/migrations/baseline/20250929_CreateAuditHashChain.sql
\i src/database/migrations/baseline/20250929_CreateOutboxPattern.sql

-- FRP-01 (Identity) - when ready
-- \i src/database/migrations/frp01/...

-- FRP-02 (Spatial) - when ready
-- \i src/database/migrations/frp02/...

-- ... etc
SQL

echo "✓ Supabase relational migrations complete"
```

### scripts/db/migrate-timescale-cloud.sh

```bash
#!/bin/bash
# Migrate time-series hypertables to Timescale Cloud

set -e

echo "Migrating time-series tables to Timescale Cloud..."

psql "$DATABASE_URL_TIMESCALE" << 'SQL'
-- Enable TimescaleDB (already enabled in Timescale Cloud)

-- Create hypertables
\i src/database/migrations/timescale/20250929_CreateSensorReadingsHypertable.sql
\i src/database/migrations/timescale/20250929_CreateSensorRollups.sql
\i src/database/migrations/timescale/20250929_CreateAlertsHypertable.sql
\i src/database/migrations/timescale/20250929_CreateIrrigationHypertables.sql
\i src/database/migrations/timescale/20250929_CreateTaskEventsHypertable.sql

SQL

echo "✓ Timescale Cloud migrations complete"
```

---

## Data Access Patterns

### Writing Data

```csharp
// Relational data → Supabase
public class UserRepository : IUserRepository
{
    private readonly SupabaseDbContext _context;
    
    public async Task<User> CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
}

// Time-series data → Timescale
public class TelemetryRepository : ITelemetryRepository
{
    private readonly TimescaleDbContext _context;
    
    public async Task WriteSensorReadingAsync(SensorReading reading)
    {
        _context.SensorReadings.Add(reading);
        await _context.SaveChangesAsync(); // Inserts to Timescale hypertable
    }
}
```

### Reading Data

```csharp
// Query relational data from Supabase
var user = await _supabaseContext.Users
    .Include(u => u.Sites)
    .FirstOrDefaultAsync(u => u.Id == userId);

// Query time-series data from Timescale
var readings = await _timescaleContext.SensorReadings
    .Where(r => r.SiteId == siteId 
        && r.Timestamp >= startDate 
        && r.Timestamp <= endDate)
    .OrderBy(r => r.Timestamp)
    .ToListAsync();

// Query rollups (continuous aggregates)
var hourlyData = await _timescaleContext.SensorReadings1h
    .Where(r => r.SiteId == siteId && r.Bucket >= startDate)
    .ToListAsync();
```

### Joining Across Databases

```csharp
// Avoid direct joins; fetch separately and join in app
var sites = await _supabaseContext.Sites
    .Where(s => s.OrganizationId == orgId)
    .ToListAsync();

var siteIds = sites.Select(s => s.Id).ToArray();

var readings = await _timescaleContext.SensorReadings
    .Where(r => siteIds.Contains(r.SiteId))
    .ToListAsync();

// Join in memory
var result = from site in sites
             join reading in readings on site.Id equals reading.SiteId
             group reading by site into g
             select new {
                 Site = g.Key,
                 LatestReading = g.OrderByDescending(r => r.Timestamp).First()
             };
```

---

## Cost Analysis (MVP)

| Database | Plan | Storage | Compute | Cost |
|----------|------|---------|---------|------|
| **Supabase** | Free | 500MB | 1GB RAM | **$0** |
| **Timescale Cloud** | Free | 1GB | 1GB RAM | **$0** |
| **ClickHouse** | Self-hosted | 10GB (Docker volume) | 2 CPU / 4GB RAM | **$0** |
| **Total MVP** | | | | **$0/month** |

**Production Scaling:**

| Database | Plan | Storage | Compute | Cost |
|----------|------|---------|---------|------|
| **Supabase** | Pro | 8GB | 2GB RAM | **$25/mo** |
| **Timescale Cloud** | Dev | 25GB | 2GB RAM | **$50/mo** |
| **ClickHouse** | Self-hosted (K8s) | 100GB | 4 CPU / 8GB RAM | **~$100/mo** |
| **Total PRD** | | | | **~$175/month** |

**Still cheaper than single TimescaleDB managed instance at production scale!**

---

## Monitoring & Observability

### Supabase Dashboard
- Query performance
- Connection pool stats
- RLS policy hits

### Timescale Cloud Console
- Hypertable sizes
- Compression ratios
- Continuous aggregate freshness
- Query performance

### ClickHouse Monitoring (when enabled)
- Query latency
- Memory usage
- Disk usage
- Replication lag

### Unified Observability (Grafana)
```yaml
# Prometheus metrics from all databases
- Supabase: pg_stat_database, pg_stat_statements
- Timescale: timescaledb_information.* metrics
- ClickHouse: system.metrics, system.events
```

---

## Backup & Disaster Recovery

### Supabase
- **Free:** No automated backups
- **Pro:** Daily backups + PITR
- **Recommendation:** Upgrade to Pro before pilot ($25/mo)

### Timescale Cloud
- **Free:** Daily backups (7-day retention)
- **Dev:** Daily backups + PITR (14-day retention)
- **Excellent DR story out of the box!**

### ClickHouse
- Self-hosted: Configure backups to S3
```bash
# Daily backup cron
0 2 * * * docker exec harvestry-clickhouse \
  clickhouse-backup create && \
  clickhouse-backup upload latest
```

---

## Acceptance Criteria Met ✅

| Criterion | How It's Met |
|-----------|-------------|
| **Telemetry p95 < 1.0s** | Timescale hypertables + compression |
| **Rollup freshness < 60s** | Timescale continuous aggregates |
| **Realtime push p95 < 1.5s** | Supabase Realtime + Timescale LISTEN/NOTIFY |
| **RLS/ABAC everywhere** | Supabase native RLS on relational data |
| **Audit hash chain** | Supabase (audit_trail table) |
| **Site-scoped security** | Supabase RLS policies |
| **Compliance exports** | Supabase (relational) + Timescale (telemetry history) |
| **QBO recon ≤0.5%** | Supabase (accounting_queue) |

**All Track B acceptance criteria remain achievable with hybrid architecture!**

---

## Migration from Track A

**If you already ran Track A migrations with TimescaleDB:**

1. **Export relational data** from old setup
2. **Import to Supabase** (relational only)
3. **Keep time-series data** in existing TimescaleDB OR migrate to Timescale Cloud
4. **Update connection strings** in .env.local
5. **Deploy updated services** with dual DbContext

**Minimal disruption; data migration is straightforward.**

---

## FAQ

### Q: Why not just use Supabase for everything?

**A:** Supabase (PostgreSQL) is excellent for relational data but not optimized for high-volume time-series. Timescale provides:
- Automatic compression (10x space savings)
- Continuous aggregates (automatic rollups)
- Time-based partitioning
- Query optimization for time-series

### Q: Can I use a single database for MVP?

**A:** Yes, for initial development you can use Supabase only and add Timescale later. Performance will degrade with telemetry volume, but it's workable for initial testing with <10k data points.

### Q: What about ClickHouse complexity?

**A:** ClickHouse is optional for MVP. Only enable when:
- Telemetry volume > 10M points/day
- Query p95 > 5s on Timescale
- Analytics queries cause performance issues

For MVP with single pilot site, Timescale is sufficient.

### Q: How do I query across both databases?

**A:** Use the application layer to join:
1. Fetch relational data from Supabase
2. Fetch time-series data from Timescale
3. Join in-memory (LINQ, JavaScript, etc.)

Avoid cross-database joins at DB level.

---

## Next Steps

1. ✅ **Create Supabase project** (relational only)
2. ✅ **Create Timescale Cloud service** (time-series)
3. ✅ **Update .env.local** with both connection strings
4. ✅ **Run hybrid migrations** (separate scripts)
5. ✅ **Update DbContext** configuration (dual contexts)
6. 🚧 **Start FRP-01** (Identity) → All relational, uses Supabase only
7. 🚧 **Implement FRP-05** (Telemetry) → Uses Timescale for sensor_readings

---

**Last Updated:** 2025-09-29  
**Version:** 1.0  
**Status:** ✅ Approved Architecture  
**Next Review:** After FRP-05 (Telemetry) implementation
