# Quick Supabase + Timescale Cloud Setup

## Step 1: Create Supabase Project

1. Go to https://app.supabase.com
2. Click "New Project"
3. Fill in:
   - **Name:** harvestry-erp-dev
   - **Database Password:** (Generate strong password - SAVE IT!)
   - **Region:** us-west-1 (or closest to you)
   - **Plan:** Free
4. Wait 2-3 minutes for provisioning

## Step 2: Get Supabase Credentials

Once created, go to **Settings → Database**:

Copy these values:
- **Connection string (Direct):** The URI mode connection
- **Connection pooling (Transaction):** The pooler connection

Go to **Settings → API**:
- **URL:** Your project URL
- **anon public key:** Copy this
- **service_role key:** Copy this (KEEP SECRET!)

## Step 3: Create Timescale Cloud Account

1. Go to https://console.cloud.timescale.com
2. Sign up (free account)
3. Click "Create service"
4. Fill in:
   - **Name:** harvestry-telemetry-dev
   - **Plan:** Free (1GB)
   - **Region:** Same as Supabase (e.g., us-west-2)
5. Wait 2 minutes for provisioning

## Step 4: Get Timescale Credentials

After provisioning, copy:
- **Connection string:** The service URL shown

## Step 5: Create .env.local

```bash
cp env.hybrid.template .env.local
nano .env.local  # or use your favorite editor
```

Fill in the values you copied above.

## Step 6: Run Setup

```bash
./scripts/db/migrate-hybrid-setup.sh
```

That's it! You should see:
- ✓ Supabase connection successful
- ✓ Baseline migrations complete
- ✓ Timescale Cloud connection successful
- ✓ Hypertables created

## Troubleshooting

If you get "command not found", make scripts executable:
```bash
chmod +x scripts/db/*.sh scripts/setup*.sh
```

If connections fail, double-check:
1. Passwords are correct (no extra spaces)
2. You're using DIRECT connection for Supabase (not pooler) in setup
3. Timescale URL includes ?sslmode=require

## Next Steps

Once setup is complete:
1. Review: docs/setup/HYBRID_DATABASE_ARCHITECTURE.md
2. Start FRP-01 (Identity) implementation
3. Test queries to both databases
