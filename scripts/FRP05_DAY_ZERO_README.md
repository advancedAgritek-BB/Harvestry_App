# FRP-05 Day Zero Validation Scripts

This directory contains all scripts needed to validate infrastructure prerequisites for FRP-05 (Telemetry Ingest & Rollups) before beginning implementation.

---

## 🚀 Quick Start

### Run Complete Validation (Recommended)

```bash
# Set database connection
export DATABASE_URL="postgresql://user:password@host:port/database"

# Run all validations
./scripts/frp05-day-zero.sh

# View results
cat logs/frp05-day0-summary.txt
```

**Duration:** 30-60 minutes (automated tests) + manual steps as needed

---

## 📋 Individual Scripts

### 1. Master Validation Script
**File:** `frp05-day-zero.sh`  
**Purpose:** Orchestrates all Day Zero validations  
**Usage:** `./scripts/frp05-day-zero.sh [database_url]`

**What it does:**
- Runs all validation scripts in sequence
- Checks environment configuration
- Generates comprehensive results
- Provides GO/NO-GO recommendation

---

### 2. TimescaleDB Validation
**File:** `db/validate-timescaledb.sh`  
**Purpose:** Validates TimescaleDB features  
**Usage:** `./scripts/db/validate-timescaledb.sh [database_url]`

**Tests:**
- Extension enablement
- Hypertable creation
- Compression policies
- Retention policies
- Continuous aggregates
- Query performance

**Output:** `logs/frp05-day0-timescaledb-results.txt`

---

### 3. Logical Replication Validation
**File:** `db/validate-logical-replication.sh`  
**Purpose:** Checks replication availability for real-time push  
**Usage:** `./scripts/db/validate-logical-replication.sh [database_url]`

**Tests:**
- WAL level configuration
- Replication slot availability
- Permissions (REPLICATION privilege)
- Slot creation and deletion

**Output:** `logs/frp05-day0-replication-results.txt`

**Note:** If replication unavailable, fallback strategy is documented (polling-based push)

---

### 4. Environment Setup
**File:** `setup/setup-frp05-environment.sh`  
**Purpose:** Generates environment variable template  
**Usage:** `./scripts/setup/setup-frp05-environment.sh [environment]`

**Creates:** `.env.frp05.[environment]` with all required variables

**Variables Generated:**
- `TELEMETRY_DB_CONNECTION`
- `TELEMETRY_MQTT_BROKER_URL`
- `TELEMETRY_MQTT_USERNAME` / `PASSWORD`
- `TELEMETRY_MAX_BATCH_SIZE`
- `TELEMETRY_COPY_BATCH_BYTES`
- `TELEMETRY_ALERT_EVALUATION_INTERVAL`
- `TELEMETRY_ALERT_COOLDOWN_MINUTES`
- `TELEMETRY_SIGNALR_ALLOWED_ORIGINS`
- `TELEMETRY_WAL_SLOT_NAME`

---

### 5. Results Document Generator
**File:** `setup/create-day-zero-results.sh`  
**Purpose:** Compiles validation results into comprehensive document  
**Usage:** `./scripts/setup/create-day-zero-results.sh`

**Creates:** `docs/FRP05_DAY_ZERO_RESULTS.md`

**Includes:**
- Executive summary
- Detailed test results
- Fallback strategies
- GO/NO-GO recommendation
- Stakeholder sign-off checklist

---

## 📊 Expected Outputs

### Logs Directory
```
logs/
├── frp05-day0-summary.txt              # Overall summary
├── frp05-day0-timescaledb-results.txt  # TimescaleDB details
└── frp05-day0-replication-results.txt  # Replication details
```

### Documentation
```
docs/
├── FRP05_DAY_ZERO_RESULTS.md           # Comprehensive results
└── telemetry/
    └── mqtt-configuration.md            # MQTT config (manual)
```

### Environment Files
```
.env.frp05.dev                           # Development environment
.env.frp05.staging                       # Staging environment
```

---

## ✅ Success Criteria

### Minimum (GO)
- ✅ TimescaleDB extension enabled
- ✅ Hypertables working
- ✅ Environment variables set
- ⚠️ MQTT configured OR HTTP-only fallback
- ⚠️ Logical replication OR polling fallback

### Ideal (FULL GO)
- ✅ All minimum requirements
- ✅ Compression/retention policies working
- ✅ Continuous aggregates working
- ✅ Logical replication available
- ✅ MQTT broker configured and tested
- ✅ Load test baseline completed

---

## 🔧 Troubleshooting

### Script Permission Denied
```bash
chmod +x scripts/frp05-day-zero.sh
chmod +x scripts/db/*.sh
chmod +x scripts/setup/*.sh
```

### Database Connection Failed
```bash
# Test connection manually
psql "$DATABASE_URL" -c "SELECT version();"

# Check connection string format:
# postgresql://user:password@host:port/database
```

### TimescaleDB Not Available
- Contact database team to install TimescaleDB
- Or use managed TimescaleDB service (Timescale Cloud)
- Or accept PostgreSQL partitioning fallback

### Logical Replication Unavailable
- Use polling-based fallback (documented in results)
- Accept 2-5 second latency vs <1 second
- No special database permissions required

---

## 📖 Documentation

Full documentation available in `docs/`:

| Document | Purpose |
|----------|---------|
| `FRP05_READINESS_REVIEW.md` | Comprehensive readiness assessment (400+ lines) |
| `FRP05_DAY_ZERO_CHECKLIST.md` | Detailed validation checklist (44 items) |
| `FRP05_DAY_ZERO_QUICKSTART.md` | Quick start execution guide |
| `FRP05_DAY_ZERO_EXECUTION_LOG.md` | Execution tracking and status |
| `telemetry/mqtt-configuration-template.md` | MQTT broker configuration template |

---

## 🎯 Next Steps After Validation

1. **Review Results**
   - Read `docs/FRP05_DAY_ZERO_RESULTS.md`
   - Check logs in `logs/` directory

2. **Complete Manual Steps**
   - Configure MQTT broker (if required)
   - Set environment variables
   - Run load test baseline

3. **Get Stakeholder Sign-Off**
   - DevOps Lead
   - Database Team
   - Sensors Team
   - Telemetry & Controls Squad Lead

4. **Proceed to Day 1**
   - Pre-Slice Setup (2 hours)
   - Begin Slice 1 implementation

---

## 📞 Support

| Issue | Contact | Script to Re-Run |
|-------|---------|------------------|
| Database issues | Database Team | `validate-timescaledb.sh` |
| Replication issues | Database Team | `validate-logical-replication.sh` |
| MQTT issues | Sensors Team | Manual configuration |
| Environment issues | DevOps | `setup-frp05-environment.sh` |

---

## 📝 Notes

- All test objects are automatically cleaned up
- Scripts are idempotent (safe to run multiple times)
- No production data is modified
- Logs are appended, not overwritten
- Environment files are gitignored by default

---

**Ready to start?** Run: `./scripts/frp05-day-zero.sh` 🚀

**Questions?** See: `docs/FRP05_DAY_ZERO_QUICKSTART.md`

