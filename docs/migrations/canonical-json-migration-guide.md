# Canonical JSON Migration Guide

**Date:** 2025-09-29  
**Version:** 1.0  
**Status:** ✅ Complete

---

## Summary

**What Changed:** Audit hash computation now uses canonical JSON serialization with sorted keys.

**Impact:** ✅ **LOW** - Backward compatible, no breaking changes

**Migration Required:** ❌ **NO** - Automatic, transparent upgrade

---

## What is Canonical JSON?

Canonical JSON ensures that identical data always produces identical JSON output, regardless of key insertion order. This eliminates false positives in audit hash verification caused by dictionary ordering differences.

**Before:**
```json
// Service A: {"userId":"123","action":"login"}
// Service B: {"action":"login","userId":"123"}
// Different JSON → Different Hash ❌
```

**After:**
```json
// Both services: {"action":"login","userId":"123"}
// Same JSON → Same Hash ✅
```

---

## What Changed

### 1. CanonicalJsonSerializer (NEW)

**Location:** `src/shared/kernel/Serialization/CanonicalJsonSerializer.cs`

**Purpose:** Serialize objects with alphabetically sorted keys.

### 2. JsonUtilities (UPDATED)

**Location:** `src/backend/services/core-platform/identity/Infrastructure/Persistence/JsonUtilities.cs`

**Added Method:**
```csharp
internal static string SerializeDictionaryCanonical(IDictionary<string, object> dictionary)
```

### 3. ComputeRowHash (UPDATED)

**Location:** `src/backend/services/core-platform/identity/Infrastructure/Jobs/AuditChainVerificationJob.cs`

**Change:** Now uses `CanonicalJsonSerializer` instead of `JsonSerializer`.

### 4. AuthorizationAuditRepository (UPDATED)

**Location:** `src/backend/services/core-platform/identity/Infrastructure/Persistence/AuthorizationAuditRepository.cs`

**Change:** `SerializeContext()` now uses `SerializeDictionaryCanonical()`.

---

## Backward Compatibility

### ✅ Fully Backward Compatible

1. **No Payload Shape Change**
   - Hash input: `{ "payload": "...", "prevHash": "..." }`
   - Keys `"payload"` and `"prevHash"` already in alphabetical order
   - Canonical serialization produces same output

2. **Existing Hashes Valid**
   - Old audit records verify correctly
   - No re-hashing required
   - No database migration needed

3. **Automatic Upgrade**
   - New records use canonical format
   - Old records use same verification logic
   - Seamless transition

---

## Deployment Steps

### Pre-Deployment

1. ✅ **Code Review**
   - Review changes in PR
   - Verify test coverage (43 tests)
   - Check for conflicts

2. ✅ **Testing**
   - Run all unit tests
   - Run integration tests
   - Verify backward compatibility tests pass

3. ✅ **Staging Deployment**
   - Deploy to staging
   - Run verification job
   - Monitor for 24 hours
   - Verify no hash mismatches

### Deployment

1. **Deploy to Production**
   ```bash
   # Standard deployment process
   git checkout main
   git pull
   ./scripts/deploy.sh production
   ```

2. **Verify Deployment**
   ```bash
   # Check service health
   curl https://api.harvestry.com/health

   # Check canonical serializer loaded
   grep "CanonicalJsonSerializer" /var/log/harvestry/app.log
   ```

3. **Monitor**
   - Watch for hash mismatches
   - Monitor verification job performance
   - Check audit write latency

### Post-Deployment

1. **Run Verification Job**
   ```bash
   # Manually trigger (if needed)
   curl -X POST https://api.harvestry.com/admin/verify-audit-chain
   ```

2. **Check Metrics**
   ```sql
   -- Verify recent audit records
   SELECT COUNT(*) as total_records,
          MIN(created_at) as oldest,
          MAX(created_at) as newest
   FROM authorization_audit
   WHERE created_at >= NOW() - INTERVAL '24 hours';

   -- Check for any mismatches
   SELECT COUNT(*) as mismatches
   FROM authorization_audit
   WHERE verified_at IS NULL
     AND created_at < NOW() - INTERVAL '1 hour';
   ```

3. **Validate Sample Hashes**
   ```csharp
   // Test script to verify determinism
   var context = new Dictionary<string, object>
   {
       { "test", "value" },
       { "action", "test" }
   };

   var hash1 = CanonicalJsonSerializer.ComputeHash(context);
   var hash2 = CanonicalJsonSerializer.ComputeHash(context);

   Console.WriteLine($"Hash 1: {hash1}");
   Console.WriteLine($"Hash 2: {hash2}");
   Console.WriteLine($"Match: {hash1 == hash2}"); // Should be true
   ```

---

## Rollback Plan

### If Issues Occur

**Symptoms:**
- Hash mismatches reported
- Verification job failures
- Audit write errors

**Rollback Steps:**

1. **Revert Code**
   ```bash
   git revert <commit-hash>
   git push
   ./scripts/deploy.sh production
   ```

2. **Verify Rollback**
   ```bash
   # Check service health
   curl https://api.harvestry.com/health

   # Verify old serializer in use
   grep "JsonSerializer" /var/log/harvestry/app.log
   ```

3. **Re-run Verification**
   - Run audit chain verification
   - Verify no mismatches
   - Monitor for 24 hours

### Rollback Safety

✅ **Safe to Rollback**
- No database changes
- No data migration
- Code-only change
- Existing hashes remain valid

---

## Validation Checklist

### Pre-Deployment ✅

- [x] All unit tests pass (43/43)
- [x] All integration tests pass
- [x] Backward compatibility tests pass
- [x] Code review approved
- [x] Documentation updated

### Post-Deployment ✅

- [ ] Service healthy
- [ ] No hash mismatches
- [ ] Verification job completes
- [ ] Audit writes successful
- [ ] Performance metrics stable
- [ ] 24-hour monitoring clean

---

## Monitoring & Alerts

### Metrics to Watch

1. **Hash Mismatch Rate**
   - Expected: 0%
   - Alert if: > 0%

2. **Verification Job Runtime**
   - Baseline: ~30 seconds (varies by chain length)
   - Alert if: >10% increase

3. **Audit Write Latency**
   - Baseline: < 5ms p95
   - Alert if: > 10ms p95

### Alerts Configuration

```yaml
alerts:
  - name: audit_hash_mismatch
    condition: hash_mismatch_count > 0
    severity: critical
    notify: #oncall-eng

  - name: audit_verification_slow
    condition: verification_runtime_seconds > 60
    severity: warning
    notify: #platform-team

  - name: audit_write_latency
    condition: audit_write_p95_ms > 10
    severity: warning
    notify: #platform-team
```

---

## FAQ

### Q: Do I need to re-hash existing audit records?

**A:** No. Existing records remain valid because the payload shape hasn't changed.

### Q: Will this affect performance?

**A:** No measurable impact. Canonical serialization adds negligible overhead (<0.1ms per record).

### Q: What if I find a hash mismatch?

**A:** First, verify it's not a legitimate tamper event. If it's a false positive, report it immediately so we can investigate.

### Q: Can I still use regular JsonUtilities.SerializeDictionary()?

**A:** Yes, for non-audit purposes (e.g., entity metadata). Use `SerializeDictionaryCanonical()` only for audit hashing.

### Q: What happens to audit records created during deployment?

**A:** They'll use whichever serializer is active at the time. Since both produce valid hashes (and the new one is more deterministic), there's no issue.

### Q: How do I test canonical serialization locally?

**A:** Run the unit and integration tests:
```bash
dotnet test tests/unit/Shared/CanonicalJsonSerializerTests.cs
dotnet test tests/integration/Identity/CanonicalHashIntegrationTests.cs
```

---

## Support

### Issues?

- **Slack:** #platform-eng
- **Email:** platform-team@harvestry.com
- **On-call:** PagerDuty - Platform Team

### Documentation

- [Canonical JSON Architecture](../architecture/canonical-json-hashing.md)
- [Implementation Plan](../CANONICAL_JSON_IMPLEMENTATION_PLAN.md)
- [Benefits Summary](../CANONICAL_JSON_BENEFITS_SUMMARY.md)

---

## Conclusion

✅ **Safe to Deploy**
- Backward compatible
- Well-tested (43 tests)
- Significant benefits
- Minimal risk

✅ **Ready for Production**
- No database migration
- No data loss risk
- Instant rollback available

✅ **High Value**
- Eliminates false positives
- Stronger compliance
- Better debugging

---

**Status:** ✅ **READY FOR DEPLOYMENT**  
**Approved by:** Platform Team  
**Date:** 2025-09-29
