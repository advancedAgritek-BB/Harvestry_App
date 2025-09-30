# Supabase Quick Start Checklist

**Estimated Time:** 30 minutes  
**Goal:** Get Supabase configured and ready for FRP-01 Identity work

---

## ✅ Pre-Setup Checklist

Before you begin:

- [ ] Create Supabase account at [supabase.com](https://supabase.com)
- [ ] Install `psql`: `brew install postgresql`
- [ ] Install Supabase CLI: `brew install supabase/tap/supabase`
- [ ] Have this repository cloned locally

---

## 🚀 Quick Setup (5-Minute Version)

### 1. Create Supabase Project

1. Go to [app.supabase.com](https://app.supabase.com)
2. Click **"New Project"**
3. Fill in:
   - Name: `harvestry-erp-dev`
   - Password: Generate strong password (save it!)
   - Region: Closest to you
4. Wait 2-3 minutes for provisioning

### 2. Get Credentials

Go to **Settings → Database** and copy:

- **Connection string** (Direct connection)
- **Connection pooling string** (Transaction mode)

Go to **Settings → API** and copy:

- **Project URL**
- **anon public** key
- **service_role** key (keep secret!)

### 3. Run Automated Setup

```bash
# Navigate to your project root directory
cd ./Harvestry_App

# Run quick setup script
./scripts/setup-supabase.sh

# Follow prompts:
# 1. Script creates .env.local
# 2. Fill in your Supabase credentials
# 3. Press Enter to continue
# 4. Script tests connection
# 5. Script enables TimescaleDB
# 6. Script runs all migrations
```

**That's it!** ✅

---

## 📝 Manual Setup (If Script Fails)

### Step 1: Create .env.local

```bash
# Create environment file
cat > .env.local << 'EOF'
# Database (Direct - for migrations)
DATABASE_URL_DIRECT=postgresql://postgres:[YOUR-PASSWORD]@db.[YOUR-PROJECT-REF].supabase.co:5432/postgres

# Database (Pooler - for API)
DATABASE_URL=postgresql://postgres.[YOUR-PROJECT-REF]:[YOUR-PASSWORD]@aws-0-us-west-1.pooler.supabase.com:6543/postgres

# Supabase API
SUPABASE_URL=https://[YOUR-PROJECT-REF].supabase.co
SUPABASE_ANON_KEY=[YOUR-ANON-KEY]
SUPABASE_SERVICE_ROLE_KEY=[YOUR-SERVICE-ROLE-KEY]

SUPABASE_PROJECT_ID=[YOUR-PROJECT-REF]

NODE_ENV=development
LOG_LEVEL=Information
EOF

# Edit with your actual values
nano .env.local
```

### Step 2: Test Connection

```bash
# Load environment
source .env.local

# Test connection
psql "$DATABASE_URL_DIRECT" -c "SELECT version();"

# Should output: PostgreSQL 15.x ...
```

### Step 3: Enable TimescaleDB

```bash
psql "$DATABASE_URL_DIRECT" << 'SQL'
CREATE EXTENSION IF NOT EXISTS timescaledb CASCADE;
SELECT extname, extversion FROM pg_extension WHERE extname = 'timescaledb';
SQL
```

### Step 4: Run Migrations

```bash
chmod +x scripts/db/migrate-supabase.sh
./scripts/db/migrate-supabase.sh
```

---

## ✅ Verification

After setup completes, verify:

```bash
# Check tables exist
psql "$DATABASE_URL_DIRECT" -c "\dt"

# Should see: audit_trail, outbox, ...
```

```bash
# Check hypertables
psql "$DATABASE_URL_DIRECT" -c "SELECT hypertable_name FROM timescaledb_information.hypertables;"

# Should see: sensor_readings, alerts, irrigation_step_runs, task_events
```

```bash
# Test audit function
psql "$DATABASE_URL_DIRECT" << 'SQL'
SELECT log_audit_event(
    p_user_id := gen_random_uuid(),
    p_site_id := gen_random_uuid(),
    p_event_type := 'TEST',
    p_entity_type := 'test_setup',
    p_entity_id := gen_random_uuid(),
    p_action := 'Verify setup',
    p_context := '{"test": true}'::JSONB
);

SELECT * FROM audit_trail ORDER BY occurred_at DESC LIMIT 1;
SQL
```

If all three commands succeed → **Setup complete!** ✅

---

## 🔐 Security Note

**Never commit .env.local to Git!**

Verify it's ignored:

```bash
grep ".env.local" .gitignore

# If not present, add it:
echo ".env.local" >> .gitignore
```

---

## 📊 Optional: Supabase Dashboard Tour

After setup, explore your Supabase Dashboard:

### **Table Editor**
- View tables: `audit_trail`, `outbox`
- Inspect data directly in browser

### **SQL Editor**
- Run custom queries
- Test functions
- View execution plans

### **Database → Replication**
- Enable Realtime for tables (we'll do this in FRP-05)

### **Authentication → Users**
- Will populate during FRP-01 Identity work

### **Storage**
- Create buckets for COAs, labels (FRP-09, FRP-07)

### **Logs**
- View query logs
- Monitor slow queries

---

## 🐛 Troubleshooting

### Issue: "could not connect to server"

**Check:**
1. Is DATABASE_URL_DIRECT correct?
2. Did you replace `[YOUR-PASSWORD]` with actual password?
3. Did you replace `[YOUR-PROJECT-REF]` with actual project ref?

**Test:**
```bash
# Print (without password showing)
echo $DATABASE_URL_DIRECT | sed 's/:[^@]*@/:***@/'
```

---

### Issue: "extension timescaledb does not exist"

**Solution:** Supabase Free tier may not have TimescaleDB. Check:

```bash
psql "$DATABASE_URL_DIRECT" -c "SELECT * FROM pg_available_extensions WHERE name = 'timescaledb';"
```

If not available, you can:
1. Upgrade to Supabase Pro ($25/mo) with TimescaleDB
2. Or use regular PostgreSQL for development (slower for telemetry)

---

### Issue: Migration fails with "permission denied"

**Solution:** Use direct connection (not pooler) for migrations:

```bash
# Check you're using DATABASE_URL_DIRECT
echo $DATABASE_URL_DIRECT

# Should be: postgresql://postgres:...@db.xxx.supabase.co:5432/postgres
# NOT: postgresql://postgres.xxx:...@pooler.supabase.com:6543/postgres
```

---

### Issue: "relation audit_trail already exists"

**Solution:** Migrations already ran. To reset:

```bash
psql "$DATABASE_URL_DIRECT" << 'SQL'
DROP TABLE IF EXISTS audit_trail CASCADE;
DROP TABLE IF EXISTS outbox CASCADE;
-- Re-run migrations
SQL

./scripts/db/migrate-supabase.sh
```

---

## 📚 Next Steps

✅ **Supabase configured!**

Now proceed to:

1. ✅ **Mark todo complete:** `w0-setup-supabase`
2. 🚧 **Start FRP-01 work:** Identity, Roles, RLS/ABAC
3. 📖 **Read:** `docs/TRACK_B_IMPLEMENTATION_PLAN.md` → FRP-01 section

---

## 📞 Need Help?

- **Supabase Docs:** https://supabase.com/docs/guides/database
- **TimescaleDB Docs:** https://docs.timescale.com/
- **Team Slack:** `#harvestry-eng`
- **Supabase Discord:** https://discord.supabase.com

---

**Last Updated:** 2025-09-29  
**Status:** ✅ Ready to use
