# Quick Fix: Supabase Connection Issue

## The Error We're Getting:
```
FATAL: Tenant or user not found
```

This usually means your Supabase project is **PAUSED** (very common on free tier).

## FIX IT NOW:

### Step 1: Resume Your Project

1. Open: `https://app.supabase.com/project/{SUPABASE_PROJECT_REF}` (replace `{SUPABASE_PROJECT_REF}` with your project identifier)

2. Look at the top of the page for the project status

3. **If you see a "Paused" message or orange/yellow indicator:**
   - Click **"Restore"** or **"Resume"** button
   - Wait 2-3 minutes (Supabase will wake up your database)
   - The page will refresh when ready

4. **Once it shows "Active" (green):**
   - Proceed to Step 2

### Step 2: Get the EXACT Connection String

While in your Supabase dashboard:

1. Click the **gear icon** (⚙️) at bottom left → **"Project Settings"**

2. Click **"Database"** in the left menu

3. You should see something like:

```
Connection string
[Pooler] [Session] [Direct]
```

4. Click the **"Session"** or **"Direct"** tab

5. You'll see a connection string. Click the **copy icon** to copy it

6. It will look like ONE of these formats:
```
postgres://postgres.[project]:[password]@[region].pooler.supabase.com:5432/postgres
```
OR
```
postgresql://postgres:[password]@db.[project].supabase.co:5432/postgres
```

### Step 3: Update .env.local

Whatever you copied, paste it into .env.local:

```bash
nano .env.local

# Replace BOTH lines with what you copied:
DATABASE_URL_DIRECT=<paste here>
DATABASE_URL=<paste here>
```

### Step 4: Test

```bash
source .env.local
psql "$DATABASE_URL_DIRECT" -c "SELECT version();"
```

## Still Not Working?

**Copy and paste the EXACT text you see** in Supabase under:
Project Settings → Database → Connection string

I'll help you format it correctly!
