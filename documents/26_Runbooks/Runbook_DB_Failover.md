# Runbook — Database Failover (Cross-Region)
## Pre-checks
Confirm primary health; replication lag; recent deploys; WAL continuity.
## Steps
Promote replica; update connection strings; invalidate caches; monitor SLOs.
## Safe Abort
If promotion stalls, roll back traffic; engage DB on-call.
## Post
Verify audit chain; open postmortem.
