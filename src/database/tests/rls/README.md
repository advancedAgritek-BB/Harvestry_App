# RLS Isolation Test Suite

This directory contains tests to verify Row-Level Security (RLS) policies are correctly enforcing tenant isolation and access control.

## Test Categories

### 1. Tenant Isolation Tests
- Verifies users can only access data from their assigned sites
- Tests cross-tenant access is blocked
- Validates site-scoped tables enforce `site_id` filtering

### 2. Role-Based Access Tests
- Tests different roles have appropriate permissions
- Validates admin vs operator vs viewer access levels
- Tests role escalation prevention

### 3. Service Account Tests
- Verifies service accounts can access data for their purpose
- Tests background job access patterns

## Running Tests

### Prerequisites
- PostgreSQL 15+ with TimescaleDB
- Test database with seed data
- pgTAP extension installed

### Execute Tests
```bash
# Run all RLS tests
psql -d harvestry_test -f run_all_tests.sql

# Run specific test file
psql -d harvestry_test -f 01_tenant_isolation_tests.sql
```

## Test Database Setup
Tests expect a database with:
- At least 2 test organizations
- At least 2 sites per organization
- Test users with different roles assigned to different sites
- Sample data in key tables (batches, tasks, rooms, etc.)







