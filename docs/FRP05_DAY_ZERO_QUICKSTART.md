# FRP-05 Day Zero Quick Start Guide

**Purpose:** Execute infrastructure validation before starting Slice 1  
**Duration:** 4-6 hours  
**Owner:** DevOps + Telemetry & Controls Squad

---

## 🚀 QUICK START (5 MINUTES)

### Step 1: Set Database Connection

```bash
export DATABASE_URL="postgresql://user:password@host:port/database"
```

### Step 2: Run Master Validation Script

```bash
cd /Users/brandonburnette/Downloads/Harvestry_App
./scripts/frp05-day-zero.sh
```

This single command will:

- ✅ Validate TimescaleDB (extension, hypertables, compression, aggregates)
- ✅ Validate logical replication (WAL level, slots, permissions)
- ✅ Check MQTT configuration
- ✅ Verify environment variables
- ✅ Confirm load test readiness
- ✅ Generate results document

### Step 3: Review Results

```bash
# View summary
cat logs/frp05-day0-summary.txt

# View comprehensive results
open docs/FRP05_DAY_ZERO_RESULTS.md
```

---

## 📋 DETAILED STEPS (IF ISSUES FOUND)

### Phase 1: TimescaleDB Validation (Manual)

```bash
# Run TimescaleDB validation only
./scripts/db/validate-timescaledb.sh "$DATABASE_URL"

# Check results
cat logs/frp05-day0-timescaledb-results.txt
```

**If failures:** Contact database team to enable TimescaleDB or grant permissions

---

### Phase 2: Logical Replication Validation (Manual)

```bash
# Run replication validation only
./scripts/db/validate-logical-replication.sh "$DATABASE_URL"

# Check results
cat logs/frp05-day0-replication-results.txt
```

**If failures:** See fallback strategy in results (polling-based real-time push)

---

### Phase 3: MQTT Configuration (Manual)

#### 3.1: Get MQTT Broker Details

- Contact Sensors Team for broker URL, credentials
- Fill in template: `docs/telemetry/mqtt-configuration-template.md`

#### 3.2: Test Connectivity

```bash
# Install mosquitto clients (if not installed)
# macOS: brew install mosquitto
# Linux: apt-get install mosquitto-clients

# Test publish
mosquitto_pub -h broker.example.com -p 1883 \
  -t "test/topic" -m "hello" \
  -u "username" -P "password"

# Test subscribe (in another terminal)
mosquitto_sub -h broker.example.com -p 1883 \
  -t "test/topic" \
  -u "username" -P "password"
```

#### 3.3: Set Environment Variables

```bash
export TELEMETRY_MQTT_BROKER_URL="mqtt://broker.example.com:1883"
export TELEMETRY_MQTT_USERNAME="username"
export TELEMETRY_MQTT_PASSWORD="password"
```

---

### Phase 4: Environment Setup

```bash
# Generate environment file template
./scripts/setup/setup-frp05-environment.sh dev

# Edit the generated file
vi .env.frp05.dev

# Load environment
source .env.frp05.dev

# Verify variables are set
env | grep TELEMETRY_
```

**Required Variables:**

- `TELEMETRY_DB_CONNECTION`
- `TELEMETRY_MQTT_BROKER_URL`
- `TELEMETRY_MAX_BATCH_SIZE` (default: 5000)
- `TELEMETRY_COPY_BATCH_BYTES` (default: 1048576)

---

### Phase 5: Load Test Baseline

```bash
# Ensure k6 is installed
k6 version

# Run baseline test
k6 run --vus 50 --duration 5m tests/load/telemetry-ingest-load.js

# Review results
# Target: p95 < 1000ms, error rate < 1%
```

---

## ✅ SUCCESS CRITERIA

### Minimum Requirements (GO)

- ✅ TimescaleDB extension enabled
- ✅ Hypertables working
- ✅ Environment variables set
- ⚠️ MQTT configured OR HTTP-only fallback agreed
- ⚠️ Logical replication OR polling fallback designed

### Ideal State (FULL GO)

- ✅ All above requirements met
- ✅ Compression policies working
- ✅ Retention policies working
- ✅ Continuous aggregates working
- ✅ Logical replication available
- ✅ MQTT broker tested and verified
- ✅ Load test baseline passes

---

## 🚦 INTERPRETING RESULTS

### ✅ All Tests Pass

**Decision:** **GO** - Proceed to Slice 1  
**Action:** Schedule Day 1 (Pre-Slice Setup + Slice 1 start)

### ⚠️ Warnings Only

**Decision:** **GO WITH CONDITIONS** - Proceed with fallbacks  
**Action:** Document fallback strategies, then proceed to Slice 1

**Common Warnings:**

- Compression/retention policies require manual jobs → Acceptable
- Logical replication unavailable → Use polling fallback
- MQTT not configured → Use HTTP-only initially

### ❌ Critical Failures

**Decision:** **NO-GO** - Fix failures first  
**Action:** Resolve blockers, re-run validation

**Common Failures:**

- TimescaleDB extension cannot be enabled → Contact database team
- Database connection fails → Check connection string
- No database permissions → Grant required permissions

---

## 📁 FILES CREATED

After running Day Zero, you'll have:

```
logs/
├── frp05-day0-summary.txt              # High-level summary
├── frp05-day0-timescaledb-results.txt  # Detailed TimescaleDB results
└── frp05-day0-replication-results.txt  # Detailed replication results

docs/
├── FRP05_DAY_ZERO_RESULTS.md           # Comprehensive results document
└── telemetry/
    └── mqtt-configuration.md            # MQTT configuration (manual)

.env.frp05.dev                           # Environment variables (manual)
```

---

## 🔧 TROUBLESHOOTING

### "Cannot connect to database"

```bash
# Test connection manually
psql "$DATABASE_URL" -c "SELECT version();"

# If fails, check:
- Connection string format
- Host/port accessibility
- Username/password correct
- Database exists
```

### "TimescaleDB extension not available"

```bash
# Check if extension is installed on server
psql "$DATABASE_URL" -c "SELECT * FROM pg_available_extensions WHERE name = 'timescaledb';"

# If not available:
- Contact database team to install TimescaleDB
- Or use managed TimescaleDB service
- Or accept fallback: PostgreSQL partitioning
```

### "Logical replication not available"

```bash
# Check WAL level
psql "$DATABASE_URL" -c "SHOW wal_level;"

# If not 'logical':
- Managed database may restrict this
- Use polling-based fallback (acceptable)
- Or request WAL level change from database team
```

### "MQTT broker not responding"

```bash
# Check network connectivity
ping broker.example.com

# Check port accessibility
telnet broker.example.com 1883

# If fails:
- Verify broker URL and port
- Check firewall rules
- Test from staging environment (not local)
```

---

## 📞 WHO TO CONTACT

| Issue | Contact | Action |
|-------|---------|--------|
| Database permissions | Database Team | Grant CREATE EXTENSION, replication privileges |
| TimescaleDB not available | Database Team | Install TimescaleDB extension on server |
| MQTT broker configuration | Sensors Team | Provide broker URL, credentials, topics |
| Environment provisioning | DevOps | Set up staging environment, secrets manager |
| Load test environment | DevOps | Provision resources for 10k msg/s testing |

---

## 🎯 EXPECTED TIMELINE

| Phase | Duration | Can Parallelize? |
|-------|----------|------------------|
| TimescaleDB validation | 30 min | No (sequential) |
| Logical replication check | 15 min | After TimescaleDB |
| MQTT configuration | 1-2 hours | Yes (manual, parallel) |
| Environment setup | 30 min | Yes (parallel) |
| Load test baseline | 30-60 min | Yes (parallel) |
| Documentation review | 30 min | After all validations |
| **Total** | **3-4 hours** | With parallelization |

**Note:** Initial estimate of 4-6 hours includes buffer for troubleshooting

---

## ✨ TIPS

1. **Run master script first** - It will identify all issues at once
2. **Parallelize manual steps** - MQTT and environment setup can happen simultaneously
3. **Document everything** - Capture broker URLs, credentials, decisions
4. **Get sign-off early** - Don't wait until perfect; GO WITH CONDITIONS is acceptable
5. **Keep stakeholders informed** - Share results as you go

---

## 📋 NEXT STEPS AFTER DAY ZERO

1. **Review** `docs/FRP05_DAY_ZERO_RESULTS.md`
2. **Get stakeholder sign-off** (DevOps, Database, Sensors, Squad Lead)
3. **Schedule Day 1** (Pre-Slice Setup + Slice 1)
4. **Communicate** fallback strategies (if any) to team
5. **Proceed** with confidence to implementation!

---

**Ready to start?** Run: `./scripts/frp05-day-zero.sh` 🚀
