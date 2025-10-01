# How to Get Supabase Connection String

## Step 1: Go to Supabase Dashboard

1. Go to: <https://app.supabase.com>
2. Click on your project: **[YOUR-PROJECT-REF]** (find this in your Supabase dashboard)

## Step 2: Get Connection String

1. Go to **Settings** (gear icon on left sidebar)
2. Click **Database**
3. Scroll down to **Connection string**

## Step 3: Copy the RIGHT Connection String

You'll see several connection strings. You need **TWO**:

### A) Direct Connection (for migrations)

Look for: **"URI" or "Connection string"**

It will look like:
```bash
postgres://postgres.[YOUR-PROJECT-REF]:[YOUR-PASSWORD]@aws-0-us-west-1.pooler.supabase.com:6543/postgres
```

But we need the DIRECT connection, which is:
```bash
postgresql://postgres:[YOUR-PASSWORD]@db.[YOUR-PROJECT-REF].supabase.co:5432/postgres
```

### B) Transaction Pooler (for API)

Look for: **"Transaction" mode**
```bash
postgres://postgres.[YOUR-PROJECT-REF]:[YOUR-PASSWORD]@aws-0-us-west-1.pooler.supabase.com:6543/postgres
```

## Step 4: Get Your Password

**Important:** Supabase shows the password as `[YOUR-PASSWORD]` placeholder.

To get the actual password:

### Option 1: If you saved it during project creation

- Use that password

### Option 2: Reset password

1. In **Settings → Database**
2. Click **"Reset database password"** button
3. Copy the NEW password shown
4. **SAVE IT IMMEDIATELY** (it won't be shown again)

## Step 5: Update .env.local

Replace these lines in your .env.local:

```bash
# BEFORE (wrong):
DATABASE_URL_DIRECT=postgresql://postgres:[PASSWORD]@db.[PROJECT-REF].supabase.co:5432/postgres

# AFTER (correct - replace placeholders with your values):
DATABASE_URL_DIRECT=postgresql://postgres:YOUR_ACTUAL_PASSWORD_HERE@db.[YOUR-PROJECT-REF].supabase.co:5432/postgres
```

```bash
# BEFORE (wrong):
DATABASE_URL=postgresql://postgres.[PROJECT-REF]:[PASSWORD]@aws-0-us-west-1.pooler.supabase.com:6543/postgres

# AFTER (correct - replace placeholders with your values):
DATABASE_URL=postgresql://postgres.[YOUR-PROJECT-REF]:YOUR_ACTUAL_PASSWORD_HERE@aws-0-us-west-1.pooler.supabase.com:6543/postgres
```

## Quick Fix Command

Run this (replace [YOUR-PROJECT-REF] and [YOUR-PASSWORD] with your actual values):

```bash
nano .env.local

# Find these two lines and update:
DATABASE_URL_DIRECT=postgresql://postgres:[YOUR-PASSWORD]@db.[YOUR-PROJECT-REF].supabase.co:5432/postgres
DATABASE_URL=postgresql://postgres.[YOUR-PROJECT-REF]:[YOUR-PASSWORD]@aws-0-us-west-1.pooler.supabase.com:6543/postgres
```

## Test Connection

```bash
source .env.local
psql "$DATABASE_URL_DIRECT" -c "SELECT version();"
```

If you see PostgreSQL version → Success!
