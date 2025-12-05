-- RLS Test Suite Runner
-- Executes all RLS tests in order

\echo '============================================='
\echo 'Starting RLS Test Suite'
\echo '============================================='
\echo ''

-- Create test schema if not exists
CREATE SCHEMA IF NOT EXISTS test;

-- Setup test data
\echo 'Setting up test data...'
\i 00_setup_test_data.sql

\echo ''
\echo '============================================='
\echo 'Running Tenant Isolation Tests'
\echo '============================================='
\i 01_tenant_isolation_tests.sql

\echo ''
\echo '============================================='
\echo 'Running Role-Based Access Tests'
\echo '============================================='
\i 02_role_based_access_tests.sql

\echo ''
\echo '============================================='
\echo 'All RLS Tests Complete'
\echo '============================================='
\echo 'Review output above for test results.'
\echo ''
\echo 'Legend:'
\echo '  ✓ = Test Passed'
\echo '  ✗ = Test Failed'
\echo ''


