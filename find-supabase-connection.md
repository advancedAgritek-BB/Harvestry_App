# How to Find Supabase Connection String (Updated UI)

## Method 1: From Project Settings

1. Go to: https://app.supabase.com/project/[YOUR-PROJECT-REF]

2. Click **"Project Settings"** (gear icon in left sidebar, bottom)

3. Click **"Database"** in the left menu

4. Look for section called **"Connection parameters"** or **"Connection info"**

You should see:
- Host
- Database name
- Port
- User
- Password (hidden - you'll need to retrieve this from your secure storage or reset it)

## Method 2: Connection Pooling Section

In the same Database settings page, look for:
- **"Connection pooling"** section
- **"Connection string"** section

## Method 3: Use the Pooler Connection

Use the pooler connection format with your project's credentials:

```bash
# This format (replace placeholders with your actual values from Supabase dashboard):
postgresql://postgres.[YOUR-PROJECT-REF]:[YOUR-PASSWORD]@[POOLER-HOST]:5432/postgres
```

**Where to find [POOLER-HOST]:**
- In Project Settings → Database → **Connection pooling** section (see Method 2)
- The pooler host typically follows this format: `aws-0-us-east-1.pooler.supabase.com` 
- Copy the entire pooler connection string from the dashboard, or extract just the host portion

Try this command after replacing the placeholders:
```bash
psql "postgresql://postgres.[YOUR-PROJECT-REF]:[YOUR-PASSWORD]@[POOLER-HOST]:5432/postgres" -c "SELECT version();"
```

**Note:** Never commit actual credentials to git. Store them in environment variables or use a secrets management tool.

## Method 4: Check Project Status

Your project might be paused. In the Supabase dashboard:
- Top of page shows project status
- If it says "Paused" → Click "Restore" or "Resume"
- Wait 2-3 minutes for it to wake up

## What to Copy

Take a screenshot or copy/paste EXACTLY what you see in:
**Project Settings → Database → Connection info**

We'll use that to figure out the right format!
