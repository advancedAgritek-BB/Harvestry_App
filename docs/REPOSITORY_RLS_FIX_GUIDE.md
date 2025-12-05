# Repository Layer RLS (Row-Level Security) Fix Guide

## Problem Overview

Many repository methods are missing RLS context setup before executing SQL queries, creating a **CRITICAL security vulnerability** where users could potentially access data from other sites/tenants.

## Current Issues

1. **Missing RLS Context in CUD Operations**: CreateAsync, UpdateAsync, DeleteAsync methods don't call SetRlsContextAsync
2. **Wrong Method Signature**: Some existing calls use incorrect parameters
3. **Missing Row Count Validation**: Update/Delete operations don't verify affected rows
4. **Unsafe Enum Parsing**: Using Enum.Parse instead of TryParse
5. **Unsafe JSON Deserialization**: Using null-forgiving operator after JsonSerializer.Deserialize

## Understanding RLS Context

The `GeneticsDbContext.SetRlsContextAsync` method signature is:
```csharp
public async Task SetRlsContextAsync(
    Guid userId, 
    string role, 
    Guid? siteId, 
    CancellationToken cancellationToken = default)
```

This sets PostgreSQL session variables that RLS policies use to filter data access.

## Solution Approach

### Option 1: Inject IRlsContextAccessor (Recommended)

Update repository constructors to inject `IRlsContextAccessor`:

```csharp
public class BatchCodeRuleRepository : IBatchCodeRuleRepository
{
    private readonly GeneticsDbContext _dbContext;
    private readonly ILogger<BatchCodeRuleRepository> _logger;
    private readonly IRlsContextAccessor _rlsContextAccessor; // ADD THIS

    public BatchCodeRuleRepository(
        GeneticsDbContext dbContext, 
        ILogger<BatchCodeRuleRepository> logger,
        IRlsContextAccessor rlsContextAccessor) // ADD THIS
    {
        _dbContext = dbContext;
        _logger = logger;
        _rlsContextAccessor = rlsContextAccessor; // ADD THIS
    }
}
```

Then use it in methods:

```csharp
public async Task<BatchCodeRule> CreateAsync(BatchCodeRule rule, CancellationToken cancellationToken = default)
{
    const string sql = @"INSERT INTO ...";

    await using var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
    
    // GET CURRENT CONTEXT FROM ACCESSOR
    var context = _rlsContextAccessor.Get();
    if (context == null)
        throw new InvalidOperationException("RLS context is not set");
    
    // SET RLS CONTEXT BEFORE EXECUTING SQL
    await _dbContext.SetRlsContextAsync(
        context.UserId, 
        context.Role, 
        rule.SiteId, 
        cancellationToken);

    await using var command = new NpgsqlCommand(sql, connection);
    // ... rest of method
}
```

### Option 2: Create Simpler Overload (Alternative)

Add an overload to `GeneticsDbContext` that pulls from IRlsContextAccessor internally:

```csharp
// In GeneticsDbContext.cs
private readonly IRlsContextAccessor _rlsContextAccessor;

public GeneticsDbContext(
    DbContextOptions<GeneticsDbContext> options,
    IRlsContextAccessor rlsContextAccessor)
    : base(options)
{
    _rlsContextAccessor = rlsContextAccessor ?? throw new ArgumentNullException(nameof(rlsContextAccessor));
}

public async Task SetRlsContextAsync(
    Guid siteId,
    CancellationToken cancellationToken = default)
{
    var context = _rlsContextAccessor.Get();
    if (context == null)
        throw new InvalidOperationException("RLS context not set");
        
    await SetRlsContextAsync(
        context.UserId,
        context.Role,
        siteId,
        cancellationToken);
}
```

**Important**: Register `IRlsContextAccessor` and `GeneticsDbContext` with compatible lifetimes (typically both scoped) to avoid capture/lifetime issues.

Then repositories can call:
```csharp
await _dbContext.SetRlsContextAsync(rule.SiteId, cancellationToken);
```

## Systematic Fix Checklist

### For Each Repository:

#### 1. Fix CreateAsync
```csharp
public async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
{
    const string sql = @"INSERT INTO ...";

    await using var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
    
    // ADD RLS CONTEXT CALL HERE (choose one option below)

    // Option 1: Full overload with IRlsContextAccessor values
    // var context = _rlsContextAccessor.GetContext();
    // await _dbContext.SetRlsContextAsync(context.UserId, context.Role, entity.SiteId, cancellationToken);

    // Option 2: Simple overload (DbContext fetches context internally)
    await _dbContext.SetRlsContextAsync(entity.SiteId, cancellationToken);
    
    await using var command = new NpgsqlCommand(sql, connection);
    // ... parameters and execution
}
```

**Note**: Use Option 2 for faster implementation. For Option 1, inject `IRlsContextAccessor` in the repository constructor and call `GetContext()` to retrieve user/role information. Ensure `SetRlsContextAsync` is awaited and the `cancellationToken` is passed.

#### 2. Fix UpdateAsync
```csharp
public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
{
    const string sql = @"UPDATE ... WHERE id = @Id AND site_id = @SiteId";

    await using var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
    
    // ADD RLS CONTEXT CALL HERE
    await SetRlsContext(entity.SiteId, cancellationToken);
    
    await using var command = new NpgsqlCommand(sql, connection);
    // ... parameters
    
    // VALIDATE ROW COUNT
    var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
    if (rowsAffected == 0)
    {
        _logger.LogError("Failed to update {EntityType} {EntityId}: no rows affected", 
            typeof(TEntity).Name, entity.Id);
        throw new KeyNotFoundException($"{typeof(TEntity).Name} with ID {entity.Id} not found or not accessible");
    }
    
    _logger.LogDebug("Updated {EntityType} {EntityId}", typeof(TEntity).Name, entity.Id);
    return entity;
}
```

#### 3. Fix DeleteAsync
```csharp
public async Task DeleteAsync(Guid id, Guid siteId, CancellationToken cancellationToken = default)
{
    const string sql = @"DELETE FROM ... WHERE id = @Id AND site_id = @SiteId";

    await using var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
    
    // ADD RLS CONTEXT CALL HERE
    await SetRlsContext(siteId, cancellationToken);
    
    await using var command = new NpgsqlCommand(sql, connection);
    command.Parameters.AddWithValue("Id", id);
    command.Parameters.AddWithValue("SiteId", siteId);
    
    // VALIDATE ROW COUNT
    var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
    if (rowsAffected == 0)
    {
        throw new KeyNotFoundException($"Entity with ID {id} not found or not accessible");
    }
    
    _logger.LogDebug("Deleted entity {EntityId}", id);
}
```

#### 4. Fix Enum Parsing
Replace:
```csharp
var enumValue = Enum.Parse<TEnum>(reader.GetString(columnIndex));
```

With:
```csharp
var enumString = reader.GetString(columnIndex);
if (!Enum.TryParse<TEnum>(enumString, ignoreCase: true, out var enumValue))
{
    _logger.LogWarning("Invalid enum value '{Value}' for {EnumType}, using default", 
        enumString, typeof(TEnum).Name);
    enumValue = default(TEnum); // or appropriate default
}
```

#### 5. Fix JSON Deserialization
Replace:
```csharp
var data = JsonSerializer.Deserialize<TData>(jsonString)!;
```

With:
```csharp
TData data;
try
{
    data = JsonSerializer.Deserialize<TData>(jsonString) ?? GetDefaultOrEmpty<TData>();
}
catch (JsonException ex)
{
    _logger.LogWarning(ex, "Failed to deserialize JSON for {DataType}, using empty/default",
        typeof(TData).Name);
    data = GetDefaultOrEmpty<TData>();
}
```

Where `GetDefaultOrEmpty<TData>()` is implemented as:
```csharp
using System.Reflection; // Required for BindingFlags
private static TData GetDefaultOrEmpty<TData>()
{
    // Option 1: Use static Empty property if available
    if (typeof(TData).GetProperty("Empty", BindingFlags.Public | BindingFlags.Static) != null)
    {
        return (TData)typeof(TData).GetProperty("Empty")!.GetValue(null)!;
    }

    // Option 2: Use default for value types and nullable reference types
    if (default(TData) != null || typeof(TData).IsValueType)
    {
        return default(TData)!;
    }

    // Option 3: Use factory method pattern
    if (typeof(TData).GetMethod("CreateEmpty", BindingFlags.Public | BindingFlags.Static) != null)
    {
        return (TData)typeof(TData).GetMethod("CreateEmpty")!.Invoke(null, null)!;
    }

    // Option 4: Throw when null/unset is unacceptable
    throw new InvalidOperationException($"Cannot create default value for type {typeof(TData).Name}");
}
```

Choose the behavior appropriate for your domain type: `default(TData)`, `TData.Empty`, `TData.CreateEmpty()`, or throw an exception.

## Affected Repositories

### Critical (Missing RLS in CUD operations):
1. BatchCodeRuleRepository - CreateAsync, UpdateAsync
2. BatchRelationshipRepository - CreateAsync
3. BatchRepository - DeleteAsync, ExistsAsync, BatchCodeExistsAsync
4. BatchStageDefinitionRepository - UpdateAsync, DeleteAsync
5. BatchStageTransitionRepository - UpdateAsync, DeleteAsync, TransitionExistsAsync
6. GeneticsRepository - (all unsafe parsing issues)
7. PhenotypeRepository - (JSON deserialization issues)
8. StrainRepository - (JSON deserialization issues)

### Pattern Count:
- **15+ repositories** missing RLS calls in Create/Update/Delete
- **10+ methods** lacking row count validation
- **8+ enum parse locations** needing TryParse
- **6+ JSON deserialize locations** needing null handling

## Testing After Fixes

1. **Unit Tests**: Mock IRlsContextAccessor and verify SetRlsContextAsync is called
2. **Integration Tests**: Verify cross-tenant data access is blocked
3. **Concurrency Tests**: Verify multiple users can't interfere with each other's operations

## Estimated Effort

- **Option 1 (Inject IRlsContextAccessor)**: 2-3 days
  - Update all repository constructors
  - Update DI registration
  - Fix all CreateAsync/UpdateAsync/DeleteAsync methods
  - Add row count validation
  - Fix enum parsing and JSON deserialization

- **Option 2 (Add DbContext overload)**: 1.5-2 days
  - Add overload to DbContext
  - Fix all CreateAsync/UpdateAsync/DeleteAsync methods  
  - Add row count validation
  - Fix enum parsing and JSON deserialization

## Recommendation

**Use Option 2** (simpler DbContext overload) for faster implementation. The DbContext already has access to the connection and can internally fetch the RLS context when needed, reducing the need to modify all repository constructors and DI registration.

## Example Complete Fix

See `BatchCodeRuleRepository.CreateAsync` (pending implementation example).

