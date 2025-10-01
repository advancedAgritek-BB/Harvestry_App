#!/bin/bash
# Update .env.local with correct connection string

# Backup current file if it exists
if [ -f .env.local ]; then
    if ! cp .env.local .env.local.backup; then
        echo "Error: Failed to backup .env.local file" >&2
        exit 1
    fi
    echo "✓ Backup saved to .env.local.backup"
else
    echo "No .env.local file to backup"
fi

# Note: This script has been updated to remove hardcoded credentials
# You must manually edit .env.local with your actual credentials from:
# - Supabase Dashboard (Project Settings → Database)
# - Timescale Cloud Dashboard
# See env.hybrid.template for the required format

echo "⚠️  This script no longer contains hardcoded credentials."
echo "📝 Please manually update .env.local with your credentials from:"
echo "   1. Supabase Dashboard → Project Settings → Database"
echo "   2. Timescale Cloud Dashboard"
echo "   See env.hybrid.template for the required format"
