# Canonical JSON Implementation Plan - Audit Hash Chain

**Created:** 2025-09-29  
**Priority:** 🟡 **HIGH** - Compliance & Audit Integrity  
**Estimated Effort:** 4-6 hours  
**Risk:** 🟢 **LOW** - Non-breaking, backward compatible

---

## 🎯 EXECUTIVE SUMMARY

### Problem Statement
Current audit hash implementation uses `JsonUtilities.SerializeDictionary()` which preserves dictionary insertion order. This causes **false positives** in tamper detection when:
- Different services add keys in different orders
- Dictionary iteration order varies across environments
- Refactoring changes property ordering

### Solution
Implement **canonical JSON serialization** that:
- ✅ Sorts keys alphabetically
- ✅ Uses strict number handling
- ✅ Maintains backward compatibility (no payload shape change)
- ✅ Provides deterministic cross-environment hashes

### Benefits
1. **Compliance**: Auditors trust identical events = identical hashes
2. **Reliability**: Eliminates false tamper alarms from serialization quirks
3. **Debuggability**: Reproducible hashes across environments
4. **Safety**: Guard rail against future serialization changes

---

## 📋 CURRENT STATE ANALYSIS

### Current Flow

#### 1. Hash Computation (AuditChainVerificationJob.cs:125-143)
```csharp
private static string ComputeRowHash(string? prevHash, string payload)
{
    var options = new JsonSerializerOptions
    {
        WriteIndented = false,
        PropertyNamingPolicy = null,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    var json = JsonSerializer.Serialize(new
    {
        payload,
        prevHash = prevHash ?? string.Empty
    }, options);

    using var sha256 = SHA256.Create();
    var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));
    return Convert.ToHexString(bytes);
}
```

#### 2. Payload Serialization (JsonUtilities.cs:75-83)
```csharp
internal static string SerializeDictionary(IDictionary<string, object> dictionary)
{
    if (dictionary == null || dictionary.Count == 0)
    {
        return "{}";
    }

    return JsonSerializer.Serialize(dictionary, SerializerOptions);
    // ❌ PROBLEM: Preserves insertion order, not deterministic!
}
```

### Problem Illustration

```csharp
// Service A creates audit event
var payload1 = new Dictionary<string, object>
{
    { "userId", "123" },
    { "action", "login" },
    { "siteId", "456" }
};
// Hash: ABC123...

// Service B creates same event (different order)
var payload2 = new Dictionary<string, object>
{
    { "action", "login" },
    { "siteId", "456" },
    { "userId", "123" }
};
// Hash: DEF456... (DIFFERENT! Even though semantically identical)
```

**Result:** Hash mismatch → false tamper alarm ❌

---

## 🏗️ SOLUTION ARCHITECTURE

### Design Principles
1. **Backward Compatible** - No payload shape change
2. **Canonical** - Sort keys alphabetically
3. **Strict** - Deterministic number handling
4. **Reusable** - Central utility for all serialization
5. **Testable** - Unit tests verify determinism

### Canonical JSON Rules
```
1. Sort all object keys alphabetically (case-sensitive)
2. No whitespace (compact format)
3. Strict number handling (no loss of precision)
4. UTF-8 encoding
5. Escape sequences normalized
6. No trailing commas
7. Property order: alphabetical at all nesting levels
```

### Example
```json
// Non-canonical (current)
{"userId":"123","action":"login","siteId":"456"}

// Canonical (new)
{"action":"login","siteId":"456","userId":"123"}
// Keys sorted: action < siteId < userId
```

---

## 📐 IMPLEMENTATION PLAN

### Phase 1: Create Canonical Serializer (1.5 hours)

#### Task 1.1: Create CanonicalJsonSerializer.cs
**Location:** `src/shared/kernel/Serialization/CanonicalJsonSerializer.cs`

**Features:**
- Alphabetical key sorting
- Strict number handling
- Custom converter for Dictionary
- Recursive sorting for nested objects
- UTF-8 encoding

**API:**
```csharp
public static class CanonicalJsonSerializer
{
    // Serialize object to canonical JSON string
    public static string Serialize(object value);
    
    // Serialize with options
    public static string Serialize(object value, CanonicalJsonOptions options);
    
    // Compute SHA256 hash of canonical JSON
    public static string ComputeHash(object value);
}

public record CanonicalJsonOptions
{
    public bool SortKeys { get; init; } = true;
    public bool WriteIndented { get; init; } = false;
    public JsonNumberHandling NumberHandling { get; init; } = JsonNumberHandling.Strict;
}
```

#### Task 1.2: Create Custom JsonConverter
**Purpose:** Ensure dictionary keys are sorted

```csharp
internal sealed class SortedDictionaryConverter : JsonConverter<Dictionary<string, object>>
{
    public override void Write(Utf8JsonWriter writer, Dictionary<string, object> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        // Sort keys alphabetically
        foreach (var kvp in value.OrderBy(x => x.Key, StringComparer.Ordinal))
        {
            writer.WritePropertyName(kvp.Key);
            JsonSerializer.Serialize(writer, kvp.Value, options);
        }
        
        writer.WriteEndObject();
    }
}
```

#### Task 1.3: Unit Tests
**Location:** `tests/unit/Shared/CanonicalJsonSerializerTests.cs`

**Test Cases:**
```csharp
[Fact]
public void Serialize_SortsKeys_Alphabetically()
{
    var obj = new Dictionary<string, object>
    {
        { "zebra", "z" },
        { "apple", "a" },
        { "mango", "m" }
    };
    
    var result = CanonicalJsonSerializer.Serialize(obj);
    
    Assert.Equal(@"{""apple"":""a"",""mango"":""m"",""zebra"":""z""}", result);
}

[Fact]
public void Serialize_DifferentOrder_ProducesSameOutput()
{
    var obj1 = new Dictionary<string, object> { { "b", 2 }, { "a", 1 } };
    var obj2 = new Dictionary<string, object> { { "a", 1 }, { "b", 2 } };
    
    var result1 = CanonicalJsonSerializer.Serialize(obj1);
    var result2 = CanonicalJsonSerializer.Serialize(obj2);
    
    Assert.Equal(result1, result2);
}

[Fact]
public void Serialize_NestedObjects_SortsAllLevels()
{
    var obj = new Dictionary<string, object>
    {
        { "outer", new Dictionary<string, object>
            {
                { "zebra", "z" },
                { "apple", "a" }
            }
        }
    };
    
    var result = CanonicalJsonSerializer.Serialize(obj);
    
    Assert.Contains(@"""outer"":{""apple"":""a"",""zebra"":""z""}", result);
}

[Fact]
public void ComputeHash_IdenticalContent_ProducesIdenticalHash()
{
    var obj1 = new Dictionary<string, object> { { "b", 2 }, { "a", 1 } };
    var obj2 = new Dictionary<string, object> { { "a", 1 }, { "b", 2 } };
    
    var hash1 = CanonicalJsonSerializer.ComputeHash(obj1);
    var hash2 = CanonicalJsonSerializer.ComputeHash(obj2);
    
    Assert.Equal(hash1, hash2);
}

[Fact]
public void Serialize_Numbers_UsesStrictHandling()
{
    var obj = new Dictionary<string, object>
    {
        { "int", 42 },
        { "double", 3.14159 },
        { "decimal", 99.99m }
    };
    
    var result = CanonicalJsonSerializer.Serialize(obj);
    
    // Verify no precision loss
    Assert.Contains(@"""decimal"":99.99", result);
}
```

---

### Phase 2: Update JsonUtilities (30 minutes)

#### Task 2.1: Add Canonical Option
**Location:** `Infrastructure/Persistence/JsonUtilities.cs`

**Add Method:**
```csharp
/// <summary>
/// Serializes dictionary to canonical JSON (sorted keys, deterministic)
/// </summary>
internal static string SerializeDictionaryCanonical(IDictionary<string, object> dictionary)
{
    if (dictionary == null || dictionary.Count == 0)
    {
        return "{}";
    }

    return CanonicalJsonSerializer.Serialize(dictionary);
}

/// <summary>
/// Existing method (preserved for backward compatibility if needed)
/// </summary>
internal static string SerializeDictionary(IDictionary<string, object> dictionary)
{
    // Keep existing implementation for now
    if (dictionary == null || dictionary.Count == 0)
    {
        return "{}";
    }

    return JsonSerializer.Serialize(dictionary, SerializerOptions);
}
```

#### Task 2.2: Unit Tests
**Location:** `tests/unit/Infrastructure/JsonUtilitiesTests.cs`

```csharp
[Fact]
public void SerializeDictionaryCanonical_DifferentOrder_SameOutput()
{
    var dict1 = new Dictionary<string, object> { { "b", 2 }, { "a", 1 } };
    var dict2 = new Dictionary<string, object> { { "a", 1 }, { "b", 2 } };
    
    var result1 = JsonUtilities.SerializeDictionaryCanonical(dict1);
    var result2 = JsonUtilities.SerializeDictionaryCanonical(dict2);
    
    Assert.Equal(result1, result2);
    Assert.Equal(@"{""a"":1,""b"":2}", result1);
}
```

---

### Phase 3: Update Audit Hash Computation (1 hour)

#### Task 3.1: Update ComputeRowHash
**Location:** `Infrastructure/Jobs/AuditChainVerificationJob.cs:125-143`

**Before:**
```csharp
private static string ComputeRowHash(string? prevHash, string payload)
{
    var options = new JsonSerializerOptions
    {
        WriteIndented = false,
        PropertyNamingPolicy = null,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    var json = JsonSerializer.Serialize(new
    {
        payload,
        prevHash = prevHash ?? string.Empty
    }, options);

    using var sha256 = SHA256.Create();
    var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));
    return Convert.ToHexString(bytes);
}
```

**After:**
```csharp
private static string ComputeRowHash(string? prevHash, string payload)
{
    // Use canonical serialization for deterministic hashing
    // Keys will be sorted: payload, prevHash (alphabetical)
    var hashInput = new Dictionary<string, object>
    {
        { "payload", payload },
        { "prevHash", prevHash ?? string.Empty }
    };

    // Serialize with canonical ordering
    var json = CanonicalJsonSerializer.Serialize(hashInput);

    using var sha256 = SHA256.Create();
    var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));
    return Convert.ToHexString(bytes);
}
```

**Result:**
- Keys always in order: `{"payload":"...","prevHash":"..."}`
- Deterministic across all environments
- Same payload shape (backward compatible)

#### Task 3.2: Add Comment Documentation
```csharp
/// <summary>
/// Computes SHA256 hash of audit row using canonical JSON serialization.
/// 
/// Canonical serialization ensures:
/// - Alphabetical key ordering
/// - Deterministic output across environments
/// - No false positives from insertion-order differences
/// 
/// Input format: { "payload": "...", "prevHash": "..." }
/// </summary>
private static string ComputeRowHash(string? prevHash, string payload)
{
    // Implementation...
}
```

---

### Phase 4: Update Audit Writers (1 hour)

#### Task 4.1: Find All Audit Write Locations
**Search for:** `authorization_audit`, `audit_logs`, `row_hash`

**Expected locations:**
- PolicyEvaluationService
- Session creation/revocation
- Badge operations
- User CRUD operations

#### Task 4.2: Update Audit Writing
**Pattern to find:**
```csharp
// Old pattern
var payloadJson = JsonUtilities.SerializeDictionary(payloadDict);

// New pattern
var payloadJson = JsonUtilities.SerializeDictionaryCanonical(payloadDict);
```

**Action:** Update all audit-writing code to use canonical serialization

#### Task 4.3: Verify Hash Computation
**Ensure:** All places that compute `row_hash` use the updated `ComputeRowHash` method

---

### Phase 5: Integration Testing (1.5 hours)

#### Task 5.1: Cross-Environment Test
**Location:** `tests/integration/Audit/CanonicalHashTests.cs`

```csharp
[Fact]
public async Task AuditChain_MultipleOrders_ProducesSameHash()
{
    // Arrange: Create audit events with different dictionary orders
    var event1 = CreateAuditEvent(
        new Dictionary<string, object>
        {
            { "userId", "user-123" },
            { "action", "login" },
            { "timestamp", "2025-09-29T10:00:00Z" }
        });
    
    var event2 = CreateAuditEvent(
        new Dictionary<string, object>
        {
            { "timestamp", "2025-09-29T10:00:00Z" },
            { "action", "login" },
            { "userId", "user-123" }
        });
    
    // Act: Write both and compute hashes
    await auditService.WriteAsync(event1);
    await auditService.WriteAsync(event2);
    
    // Assert: Hashes should match
    var hash1 = ComputeRowHash(null, event1.Payload);
    var hash2 = ComputeRowHash(null, event2.Payload);
    
    Assert.Equal(hash1, hash2);
}

[Fact]
public async Task AuditChainVerification_CanonicalHashes_PassesVerification()
{
    // Arrange: Create chain of events
    var events = CreateAuditChain(count: 100);
    
    foreach (var evt in events)
    {
        await auditService.WriteAsync(evt);
    }
    
    // Act: Run verification job
    var job = new AuditChainVerificationJob(...);
    await job.VerifyAsync(CancellationToken.None);
    
    // Assert: No mismatches
    Assert.Equal(0, job.MismatchCount);
}

[Fact]
public async Task AuditChain_NestedObjects_DeterministicHash()
{
    // Arrange: Complex nested payload
    var payload = new Dictionary<string, object>
    {
        { "user", new Dictionary<string, object>
            {
                { "name", "Alice" },
                { "id", "123" }
            }
        },
        { "action", "update" },
        { "changes", new List<string> { "email", "phone" } }
    };
    
    // Act: Serialize multiple times
    var json1 = JsonUtilities.SerializeDictionaryCanonical(payload);
    var json2 = JsonUtilities.SerializeDictionaryCanonical(payload);
    
    // Assert: Identical
    Assert.Equal(json1, json2);
}
```

#### Task 5.2: Historical Compatibility Test
**Verify:** New code can verify existing (old) hashes

```csharp
[Fact]
public async Task AuditChain_ExistingRecords_StillVerify()
{
    // Arrange: Query existing audit records from database
    var existingRecords = await GetExistingAuditRecords(limit: 100);
    
    // Act: Verify using current logic
    foreach (var record in existingRecords)
    {
        var computedHash = ComputeRowHash(record.PrevHash, record.Payload);
        
        // Assert: Should still match
        Assert.Equal(
            record.RowHash.ToUpperInvariant(),
            computedHash.ToUpperInvariant());
    }
}
```

---

### Phase 6: Documentation & Migration Guide (30 minutes)

#### Task 6.1: Update Architecture Docs
**Location:** `docs/architecture/audit-hash-chain.md`

**Add Section:**
```markdown
## Canonical JSON Serialization

### Overview
Audit hashes use canonical JSON serialization to ensure deterministic hashing across environments.

### Rules
1. **Key Ordering**: All object keys sorted alphabetically (case-sensitive)
2. **Number Handling**: Strict (no precision loss)
3. **Whitespace**: None (compact format)
4. **Encoding**: UTF-8
5. **Nesting**: Recursive sorting at all levels

### Example
```json
// Input (any order)
{ "userId": "123", "action": "login" }

// Canonical output (sorted)
{"action":"login","userId":"123"}
```

### Benefits
- Eliminates false positives from key order differences
- Reproducible hashes across all environments
- Resistant to refactoring that changes property order
```

#### Task 6.2: Add Migration Notes
**Location:** `docs/migrations/canonical-json-migration.md`

```markdown
# Canonical JSON Migration

## Summary
- **Date**: 2025-09-29
- **Impact**: Low
- **Breaking**: No
- **Backward Compatible**: Yes

## What Changed
Audit hash computation now uses canonical JSON serialization (sorted keys).

## Compatibility
- ✅ **Existing hashes remain valid** (no payload shape change)
- ✅ **New hashes are deterministic** (sorted keys)
- ✅ **Verification job works with both** (normalization during comparison)

## No Action Required
- Existing audit records continue to verify correctly
- New records use canonical format automatically
- No database migration needed
```

---

## 🧪 TESTING STRATEGY

### Unit Tests (10 tests)
1. ✅ Key sorting (single level)
2. ✅ Key sorting (nested objects)
3. ✅ Different insertion order → same output
4. ✅ Number handling (int, double, decimal)
5. ✅ Empty dictionary
6. ✅ Null values
7. ✅ Arrays (order preserved)
8. ✅ Special characters
9. ✅ Unicode handling
10. ✅ Hash computation determinism

### Integration Tests (5 tests)
1. ✅ Audit chain verification with canonical hashes
2. ✅ Cross-service hash compatibility
3. ✅ Historical records still verify
4. ✅ Complex nested payloads
5. ✅ High volume (1000+ records)

### Manual Verification
1. ✅ Run verification job on existing data
2. ✅ Create new audit events and verify
3. ✅ Test in dev, staging, production (same hashes)

---

## 📊 IMPLEMENTATION CHECKLIST

### Phase 1: Canonical Serializer (1.5 hours)
- [ ] Create `CanonicalJsonSerializer.cs`
- [ ] Implement `SortedDictionaryConverter`
- [ ] Add `Serialize()` method
- [ ] Add `ComputeHash()` method
- [ ] Write 10 unit tests
- [ ] All tests pass

### Phase 2: JsonUtilities (30 min)
- [ ] Add `SerializeDictionaryCanonical()` method
- [ ] Write unit tests
- [ ] Document backward compatibility

### Phase 3: Audit Hash (1 hour)
- [ ] Update `ComputeRowHash()` in `AuditChainVerificationJob`
- [ ] Add documentation comments
- [ ] Test hash computation

### Phase 4: Audit Writers (1 hour)
- [ ] Find all audit write locations (grep)
- [ ] Update to use canonical serialization
- [ ] Verify all use updated `ComputeRowHash`

### Phase 5: Integration Testing (1.5 hours)
- [ ] Write cross-environment tests
- [ ] Write historical compatibility tests
- [ ] Write nested object tests
- [ ] All integration tests pass
- [ ] Manual verification in dev

### Phase 6: Documentation (30 min)
- [ ] Update architecture docs
- [ ] Create migration guide
- [ ] Add inline comments
- [ ] Update README

---

## 🎯 SUCCESS CRITERIA

### Functional
- ✅ Same content → same hash (regardless of key order)
- ✅ Existing audit records still verify correctly
- ✅ New audit records use canonical format
- ✅ Verification job passes on all data

### Performance
- ✅ Hash computation time < 1ms per record
- ✅ No regression in verification job runtime
- ✅ Minimal memory overhead

### Quality
- ✅ 100% unit test coverage on serializer
- ✅ Integration tests pass
- ✅ No linter errors
- ✅ Documentation complete

---

## 🚀 DEPLOYMENT STRATEGY

### Phase 1: Deploy to Dev
1. Deploy canonical serializer
2. Run verification job
3. Verify all existing records still pass
4. Create new audit events
5. Verify new events pass

### Phase 2: Deploy to Staging
1. Deploy to staging
2. Run verification on production snapshot
3. Monitor for 24 hours
4. Verify no false positives

### Phase 3: Deploy to Production
1. Deploy during low-traffic window
2. Monitor verification job
3. Alert on any mismatches
4. Rollback plan: revert code (data unchanged)

### Rollback Safety
- ✅ No database changes required
- ✅ Existing hashes remain valid
- ✅ Can revert code instantly
- ✅ No data migration needed

---

## 📈 MONITORING & VALIDATION

### Metrics to Track
1. **Hash Mismatch Rate**: Should be 0%
2. **Verification Job Runtime**: Should be unchanged
3. **Audit Write Latency**: Should be unchanged
4. **False Positive Rate**: Should drop to 0%

### Alerts
- 🔴 **Critical**: Hash mismatch detected
- 🟡 **Warning**: Verification job takes >10% longer
- 🟢 **Info**: Canonical serializer in use

### Validation Queries
```sql
-- Check recent audit records
SELECT COUNT(*), MIN(created_at), MAX(created_at)
FROM authorization_audit
WHERE created_at >= NOW() - INTERVAL '24 hours';

-- Verify no mismatches
SELECT COUNT(*)
FROM authorization_audit
WHERE verified_at IS NULL
  AND created_at < NOW() - INTERVAL '1 hour';
```

---

## 💰 COST-BENEFIT ANALYSIS

### Costs
- **Development**: 4-6 hours
- **Testing**: 2 hours
- **Documentation**: 30 minutes
- **Deployment**: 1 hour
- **Total**: ~8 hours

### Benefits
- **Compliance**: High confidence in audit integrity
- **Reliability**: Zero false positives from key ordering
- **Debugging**: Reproducible hashes save hours of investigation
- **Future Safety**: Guard rail against serialization changes
- **Peace of Mind**: Auditors trust the system

### ROI
- **Break-even**: After first prevented false alarm (hours saved)
- **Long-term**: Significantly higher audit confidence
- **Risk Reduction**: Eliminates compliance risk from serialization bugs

---

## 🎯 DECISION CHECKPOINT

### Recommendation: ✅ **PROCEED**

**Reasons:**
1. ✅ **Low Risk** - Backward compatible, no breaking changes
2. ✅ **High Value** - Eliminates false positives, improves compliance
3. ✅ **Reasonable Effort** - 6-8 hours total
4. ✅ **Foundation** - Sets up for future audit improvements
5. ✅ **Best Practice** - Industry standard for audit hashing

### Timeline Options

**Option A: Implement Now (Recommended)**
- Complete before FRP-02
- Clean foundation
- 6-8 hours investment

**Option B: Include in FRP-02**
- Bundle with site audit features
- Natural integration point
- Slightly longer FRP-02

**Option C: Defer to Later**
- Implement after FRP-02
- Separate sprint
- Risk: potential false positives in meantime

**My Strong Recommendation:** **Option A** - The benefits are immense and the risk is minimal. This is foundational work that will pay dividends forever.

---

## 📋 NEXT STEPS

### If Approved:
1. Create feature branch: `feature/canonical-json-audit-hashing`
2. Start with Phase 1 (Canonical Serializer)
3. Incremental commits with tests
4. Code review after Phase 3
5. Integration testing in dev
6. Deploy to staging
7. Production deployment

### Questions?
- Implementation details?
- Testing strategy?
- Deployment concerns?
- Timeline preferences?

---

**Status:** ✅ **PLAN READY FOR APPROVAL**  
**Risk:** 🟢 **LOW**  
**Value:** 🟢 **HIGH**  
**Recommendation:** ✅ **PROCEED**

**Your decision?**
