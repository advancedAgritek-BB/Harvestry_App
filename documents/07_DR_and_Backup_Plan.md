# Disaster Recovery & Backup Plan

- **Topology:** Warm cross-region read replica with WAL archiving and PITR.
- **Objectives:** RPO ≤ 5 minutes; RTO ≤ 30 minutes.
- **Testing:** Quarterly failover drills; weekly backup restore verifications.
- **Runbook:** Promote replica, verify timeline continuity, re-point services, confirm data integrity and SLOs.
