# Timescale Cloud Setup Guide — Complete Walkthrough

**Purpose:** Set up managed TimescaleDB for telemetry time-series data  
**Time:** 10-15 minutes  
**Cost:** FREE tier (1GB storage, perfect for MVP)

---

## Step 1: Create Timescale Cloud Account

### 1.1 Sign Up

1. Go to **https://console.cloud.timescale.com**
2. Click **"Sign Up"** or **"Get Started"**
3. Sign up options:
   - **Email + Password** (recommended)
   - Or sign in with GitHub/Google

4. Verify your email address (check inbox)

---

## Step 2: Create Your First Service

### 2.1 Start Service Creation

After logging in:

1. You'll see the **"Services"** dashboard
2. Click **"Create service"** button (big green button)

### 2.2 Choose Service Configuration

**Service Name:**
```
harvestry-telemetry-dev
```

**Cloud Provider:**
- Choose: **AWS** (recommended) or **Google Cloud**
- AWS typically has better availability

**Region:**
- Choose the **same region** as your Supabase project
- Example: If Supabase is in `us-west-1`, choose `us-west-2` (closest AWS region)
- Common regions:
  - **US West:** `us-west-2` (Oregon)
  - **US East:** `us-east-1` (Virginia)
  - **EU:** `eu-central-1` (Frankfurt)

**Service Plan:**

This is the important part! Choose:

```
┌─────────────────────────────────────────────┐
│  FREE TRIAL (1 GB)                          │
│  ✓ 1 GB storage                             │
│  ✓ 1 GB RAM                                 │
│  ✓ Shared CPU                               │
│  ✓ TimescaleDB 2.13+                        │
│  ✓ PostgreSQL 15                            │
│  ✓ Daily backups (7-day retention)          │
│  ✓ Perfect for MVP/development              │
│                                             │
│  Cost: $0/month                             │
└─────────────────────────────────────────────┘
```

**Database Configuration:**
- **PostgreSQL Version:** 15 (default)
- **TimescaleDB Version:** 2.13+ (auto-included)

### 2.3 Review & Create

1. Review your selections:
   - Name: `harvestry-telemetry-dev`
   - Region: Same as Supabase
   - Plan: Free (1GB)

2. Click **"Create service"**

3. **Wait 2-3 minutes** for provisioning
   - You'll see a progress indicator
   - The service will show "Initializing..." then "Running"

---

## Step 3: Get Connection Details

### 3.1 Access Service Dashboard

Once the service is running:

1. Click on your service name: **`harvestry-telemetry-dev`**
2. You'll see the service overview page

### 3.2 Find Connection Information

Look for the **"Connection Info"** section (usually at the top)

You'll see several connection strings:

**Option 1: Service URL (Recommended for Development)**
```
postgresql://tsdbadmin:[PASSWORD]@abc123xyz.tsdb.cloud.timescale.com:5432/tsdb?sslmode=require
```

**Option 2: Pooler URL (For Production/High Concurrency)**
```
postgresql://tsdbadmin:[PASSWORD]@abc123xyz.pooler.tsdb.cloud.timescale.com:5432/tsdb?sslmode=require
```

### 3.3 Copy Your Credentials

You need these values:

| Field | Example | Where to Find |
|-------|---------|---------------|
| **Host** | `abc123xyz.tsdb.cloud.timescale.com` | In connection string |
| **Port** | `5432` | Always 5432 for PostgreSQL |
| **Database** | `tsdb` | Default database name |
| **User** | `tsdbadmin` | Default admin user |
| **Password** | `[shown once at creation]` | **SAVE THIS!** |

**⚠️ Important:** The password is shown **only once** during service creation. If you missed it:
- Click **"Reset password"** in the service dashboard
- Save the new password immediately

---

## Step 4: Configure Your .env.local

### 4.1 Open Your Environment File

```bash
# Navigate to your project root directory and edit .env.local
nano .env.local  # or use VS Code: code .env.local
```

### 4.2 Add Timescale Cloud Connection

Find the Timescale section and fill in:

```bash
# ============================================================================
# TIMESCALE CLOUD - Time-Series
# ============================================================================

# Direct Connection (for API and migrations)
DATABASE_URL_TIMESCALE=postgresql://tsdbadmin:[YOUR-PASSWORD]@[YOUR-HOST].tsdb.cloud.timescale.com:5432/tsdb?sslmode=require

# Pooler (optional, for high-concurrency - can add later)
DATABASE_URL_TIMESCALE_POOLER=postgresql://tsdbadmin:[YOUR-PASSWORD]@[YOUR-HOST].pooler.tsdb.cloud.timescale.com:5432/tsdb?sslmode=require
```

**Example (with fake credentials):**
```bash
DATABASE_URL_TIMESCALE=postgresql://tsdbadmin:MySecureP@ssw0rd123@abc123xyz.tsdb.cloud.timescale.com:5432/tsdb?sslmode=require
```

**Replace:**
- `[YOUR-PASSWORD]` → Your actual password
- `[YOUR-HOST]` → Your service hostname (e.g., `abc123xyz`)

**Save the file:** `Ctrl+X`, then `Y`, then `Enter` (in nano)

---

## Step 5: Test Connection

### 5.1 Test with psql (Command Line)

```bash
# Load environment variables
source .env.local

# Test connection
psql "$DATABASE_URL_TIMESCALE" -c "SELECT version();"
```

**Expected output:**
```
                                              version                                              
---------------------------------------------------------------------------------------------------
 PostgreSQL 15.4 on x86_64-pc-linux-gnu, compiled by gcc (GCC) 11.3.0, 64-bit
(1 row)
```

### 5.2 Verify TimescaleDB Extension

```bash
psql "$DATABASE_URL_TIMESCALE" -c "SELECT extname, extversion FROM pg_extension WHERE extname = 'timescaledb';"
```

**Expected output:**
```
   extname   | extversion 
-------------+------------
 timescaledb | 2.13.0
(1 row)
```

✅ **If you see the above, connection is working!**

---

## Step 6: Run Timescale Migrations

### 6.1 Run Hybrid Setup Script

```bash
./scripts/db/migrate-hybrid-setup.sh
```

This will:
1. Test Supabase connection
2. Run Supabase relational migrations
3. Test Timescale Cloud connection
4. Run Timescale hypertable migrations
5. Verify both setups

**Expected output:**
```
════════════════════════════════════════════════════
  Harvestry ERP - Hybrid Database Setup            
  Supabase (Relational) + Timescale Cloud (Time-Series)
════════════════════════════════════════════════════

✓ Loaded .env.local

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Part 1: Supabase (Relational Data)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

► Testing Supabase connection...
✓ Supabase connection successful

► Running Supabase relational migrations...
  ► 20250929_CreateAuditHashChain.sql
    ✓
  ► 20250929_CreateOutboxPattern.sql
    ✓

✓ Supabase setup complete

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Part 2: Timescale Cloud (Time-Series Data)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

► Testing Timescale Cloud connection...
✓ Timescale Cloud connection successful

► Verifying TimescaleDB extension...
   extname   | extversion 
-------------+------------
 timescaledb | 2.13.0

► Running Timescale Cloud migrations...
  ► 20250929_CreateSensorReadingsHypertable.sql
    ✓
  ► 20250929_CreateSensorRollups.sql
    ✓
  ► 20250929_CreateAlertsHypertable.sql
    ✓
  ► 20250929_CreateIrrigationHypertables.sql
    ✓
  ► 20250929_CreateTaskEventsHypertable.sql
    ✓

✓ Timescale Cloud setup complete

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Verification
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Supabase (Relational):
  ✓ Baseline tables created (audit_trail, outbox)

Timescale Cloud (Time-Series):
  ✓ Hypertables created (4 total)

════════════════════════════════════════════════════
✓ Hybrid database setup complete!
════════════════════════════════════════════════════

Architecture Summary:
  • Supabase:       Relational data (Identity, Tasks, Inventory)
  • Timescale Cloud: Time-series data (Telemetry, Alerts)
  • ClickHouse:      OLAP analytics (optional, add later)

Next steps:
  1. Start FRP-01 (Identity) - uses Supabase only
  2. Setup Timescale Cloud before FRP-05 (Telemetry)
  3. Review: docs/setup/HYBRID_DATABASE_ARCHITECTURE.md
```

---

## Step 7: Verify Hypertables Created

### 7.1 List Hypertables

```bash
psql "$DATABASE_URL_TIMESCALE" -c "SELECT hypertable_schema, hypertable_name, num_dimensions FROM timescaledb_information.hypertables;"
```

**Expected output:**
```
 hypertable_schema |    hypertable_name    | num_dimensions 
-------------------+-----------------------+----------------
 public            | sensor_readings       |              2
 public            | alerts                |              2
 public            | irrigation_step_runs  |              2
 public            | task_events           |              2
(4 rows)
```

### 7.2 Check Continuous Aggregates (Rollups)

```bash
psql "$DATABASE_URL_TIMESCALE" -c "SELECT view_name, materialized_only FROM timescaledb_information.continuous_aggregates;"
```

**Expected output:**
```
      view_name       | materialized_only 
----------------------+-------------------
 sensor_readings_1m   | t
 sensor_readings_5m   | t
 sensor_readings_1h   | t
(3 rows)
```

---

## Step 8: Explore Timescale Cloud Dashboard

### 8.1 Key Dashboard Features

Back in the Timescale Cloud web console, explore:

**Metrics Tab:**
- CPU usage
- Memory usage
- Disk usage (watch this with free tier!)
- Connection count

**Operations Tab:**
- View running queries
- Kill long-running queries
- Connection pooling stats

**Logs Tab:**
- Real-time PostgreSQL logs
- Error messages
- Slow query logs

**Backups Tab:**
- Daily automatic backups (7-day retention on free tier)
- Manual backup creation
- Point-in-time recovery (PITR) settings

---

## Step 9: Configure Connection Pooling (Optional)

For production/high-concurrency, enable pooler:

### 9.1 Use Pooler Connection

In your application code (for API requests):

```csharp
// For migrations and setup
var timescaleConnection = configuration.GetConnectionString("DATABASE_URL_TIMESCALE");

// For API requests (high concurrency)
var timescalePooler = configuration.GetConnectionString("DATABASE_URL_TIMESCALE_POOLER");

services.AddDbContext<TimescaleDbContext>(options =>
    options.UseNpgsql(timescalePooler)  // Use pooler for API
);
```

---

## Step 10: Monitor Storage Usage

### 10.1 Check Current Storage

```bash
psql "$DATABASE_URL_TIMESCALE" -c "
SELECT 
    hypertable_name,
    pg_size_pretty(hypertable_size(format('%I.%I', hypertable_schema, hypertable_name)::regclass)) as size
FROM timescaledb_information.hypertables;
"
```

**Example output:**
```
   hypertable_name    |  size   
----------------------+---------
 sensor_readings      | 128 kB
 alerts               | 56 kB
 irrigation_step_runs | 72 kB
 task_events          | 48 kB
(4 rows)
```

### 10.2 Set Up Storage Alerts

In Timescale Cloud dashboard:
1. Go to **Settings → Alerts**
2. Enable **"Storage usage alert"**
3. Set threshold: **800 MB** (80% of 1GB free tier)
4. Add your email

---

## Troubleshooting

### Issue: "could not connect to server"

**Check:**
1. Is your IP whitelisted?
   - Timescale Cloud allows all IPs by default
   - Check **Settings → Allowed IPs** if you have issues

2. Is SSL enabled in connection string?
   - Must include `?sslmode=require`

3. Is the password correct?
   - Reset password if needed: **Settings → Reset Password**

---

### Issue: "extension timescaledb does not exist"

**This should never happen!** TimescaleDB is pre-installed in Timescale Cloud.

If you see this:
1. Verify you're connecting to Timescale Cloud (not Supabase)
2. Check the host: should be `*.tsdb.cloud.timescale.com`

---

### Issue: "database does not exist"

**Solution:** The default database is `tsdb`, not `postgres`.

Make sure your connection string uses:
```
postgresql://tsdbadmin:password@host:5432/tsdb  # ← tsdb, not postgres
```

---

### Issue: "out of storage" (free tier limit)

**Free tier = 1GB total storage**

To manage:

1. **Enable compression earlier:**
```sql
ALTER TABLE sensor_readings SET (
  timescaledb.compress_after = '3 days'  -- Compress after 3 days instead of 7
);
```

2. **Reduce retention:**
```sql
SELECT add_retention_policy('sensor_readings', INTERVAL '30 days');  -- Keep 30 days instead of 90
```

3. **Upgrade to paid plan:**
   - Dev plan: $50/mo for 25GB
   - Pro plan: $299/mo for 100GB

---

## Free Tier Limitations

| Feature | Free Tier | Notes |
|---------|-----------|-------|
| **Storage** | 1 GB | Includes WAL, indexes, compressed data |
| **RAM** | 1 GB | Shared with other processes |
| **CPU** | Shared | Burstable, not guaranteed |
| **Backups** | 7 days | Daily automatic backups |
| **Connections** | 25 max | Use pooler for more |
| **PITR** | ❌ No | Available in Dev+ plan |
| **High Availability** | ❌ No | Single instance |
| **Support** | Community | Email support on paid plans |

**For MVP:** Free tier is perfect (covers ~20 days of raw telemetry + all rollups)

**For Production:** Upgrade to Dev ($50/mo) for 25GB + better performance

---

## Cost Planning

### Expected Storage (MVP - Single Pilot Site)

**Assumptions:**
- 10 devices
- 10 sensors per device
- 10-second interval
- = 100 data points/second
- = 8.64M points/day

**Storage:**
- Raw data: ~500MB/day uncompressed
- Compressed (after 7 days): ~50MB/day (10x compression)
- Rollups (1m/5m/1h): ~10MB/day

**Free tier (1GB) covers:**
- ~20 days of raw data (before compression kicks in)
- + 90 days of compressed data
- + All rollups (minimal space)

**For production (3 sites):**
- Upgrade to **Dev plan ($50/mo)** for 25GB
- This covers ~3 months of data

---

## Security Best Practices

### 1. Rotate Password Regularly

```bash
# In Timescale Cloud Dashboard:
# Settings → Reset Password → Copy new password

# Update .env.local with new password
nano .env.local
```

### 2. Restrict IP Access (Production)

```bash
# In Timescale Cloud Dashboard:
# Settings → Allowed IPs → Add your backend server IPs
```

### 3. Use Read-Only Credentials (Analytics)

```sql
-- Create read-only user for reporting/analytics
CREATE USER reporting_user WITH PASSWORD 'secure_password';
GRANT CONNECT ON DATABASE tsdb TO reporting_user;
GRANT USAGE ON SCHEMA public TO reporting_user;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO reporting_user;
```

---

## Next Steps

✅ **Timescale Cloud configured!**

Now you can:

1. ✅ **Verify both databases are ready:**
   ```bash
   psql "$DATABASE_URL_DIRECT" -c "SELECT 'Supabase connected' as status;"
   psql "$DATABASE_URL_TIMESCALE" -c "SELECT 'Timescale connected' as status;"
   ```

2. 🚧 **Start FRP-01 (Identity)** → Uses Supabase only
   - Users, roles, badges, sessions
   - RLS/ABAC policies
   - Audit hash chain

3. 🚧 **Prepare for FRP-05 (Telemetry)** → Uses Timescale Cloud
   - Ingest adapters (MQTT, HTTP, SDI-12)
   - Realtime push service
   - Alert evaluation

---

## Resources

- **Timescale Cloud Docs:** https://docs.timescale.com/cloud/
- **TimescaleDB Docs:** https://docs.timescale.com/
- **PostgreSQL 15 Docs:** https://www.postgresql.org/docs/15/
- **Pricing Calculator:** https://www.timescale.com/pricing

---

**Last Updated:** 2025-09-29  
**Version:** 1.0  
**Status:** ✅ Ready to use
