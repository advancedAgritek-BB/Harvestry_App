#!/bin/bash

# Validate required environment variable
if [ -z "$SUPABASE_PROJECT_REF" ]; then
    echo "Error: SUPABASE_PROJECT_REF environment variable is not set" >&2
    echo "Please set it in your .env.local file or export it in your shell" >&2
    exit 1
fi

echo "Checking Supabase project status..."
echo ""
echo "1. Go to: https://app.supabase.com/project/${SUPABASE_PROJECT_REF}"
echo ""
echo "2. Check if the project status shows:"
echo "   ✓ Status: Active (green dot)"
echo "   ✓ Database: Healthy"
echo ""
echo "3. If you see 'Paused' or 'Initializing':"
echo "   - Click 'Resume' or wait for initialization to complete"
echo "   - This can take 2-3 minutes"
echo ""
echo "4. Try the connection string from the Supabase dashboard:"
echo "   Settings → Database → Connection string"
echo "   - Look for 'Direct connection' or 'Session mode'"
echo "   - Copy the FULL string (not just the host)"
echo ""
