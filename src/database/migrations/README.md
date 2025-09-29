# Database Migrations

**Track A Implementation** - Zero-downtime migrations with expand/contract pattern

## 🎯 Overview

This directory contains all database migrations for the Harvestry ERP system. Migrations follow a **zero-downtime** strategy using the expand/contract pattern, allowing seamless schema changes without service interruption.

---

## 📁 Directory Structure

```
migrations/
├── baseline/           # Initial schema baseline
├── timescale/         # TimescaleDB-specific migrations (hypertables, continuous aggregates)
├── clickhouse/        # ClickHouse migrations (when sidecar is enabled)
└── README.md          # This file
```

---

## 🔄 Migration Strategy: Expand/Contract Pattern

### Phase 1: Expand (Additive Changes)
Add new schema elements **without** removing old ones:
- Add new tables, columns, indexes
- Create new views alongside old ones
- Deploy application code that can use **both** old and new schema
- Feature flags control which schema version is used

### Phase 2: Deploy Application
- Deploy new application code
- Use feature flags to gradually shift traffic to new schema
- Monitor for issues; rollback flag if needed

### Phase 3: Contract (Remove Old Schema)
Once new schema is stable and old code is fully retired:
- Remove old columns, tables, indexes
- Drop deprecated views
- Clean up temporary migration artifacts

**Key Principle:** Always maintain backward compatibility during the expand phase.

---

## 🚀 Running Migrations

### Using EF Core CLI

```bash
# Create a new migration
dotnet ef migrations add MigrationName --project src/shared/data-access --startup-project src/backend/services/gateway/API

# List pending migrations
dotnet ef migrations list --project src/shared/data-access

# Apply migrations (dry-run)
dotnet ef database update --project src/shared/data-access --startup-project src/backend/services/gateway/API --verbose --dry-run

# Apply migrations (production)
dotnet ef database update --project src/shared/data-access --startup-project src/backend/services/gateway/API

# Rollback to specific migration
dotnet ef database update MigrationName --project src/shared/data-access
```

### Using Migration Scripts

```bash
# Apply all pending migrations
./scripts/db/migrate.sh

# Apply migrations with dry-run
./scripts/db/migrate.sh --dry-run

# Rollback last migration
./scripts/db/rollback.sh

# Rollback to specific version
./scripts/db/rollback.sh --to 20250929120000
```

---

## 📋 Migration Naming Convention

Follow this pattern: `YYYYMMDDHHMMSS_DescriptiveActionName.cs`

**Examples:**
- ✅ `20250929120000_CreateSensorReadingsHypertable.cs`
- ✅ `20250929130000_AddSiteIdIndexToTasks.cs`
- ✅ `20250929140000_AddBatchPhaseColumn_Expand.cs`
- ✅ `20250930150000_RemoveLegacyBatchStatusColumn_Contract.cs`
- ❌ `Migration1.cs` (not descriptive)
- ❌ `AddColumn.cs` (missing timestamp)

**Suffix Conventions:**
- `_Expand` - Additive changes (Phase 1)
- `_Contract` - Removal changes (Phase 3)
- `_Data` - Data migration only (no schema changes)
- `_Timescale` - TimescaleDB-specific (hypertables, continuous aggregates)

---

## 🔒 Zero-Downtime Checklist

Before creating a migration, ensure:

- [ ] **No breaking changes** during expand phase
- [ ] **Backward compatibility** maintained
- [ ] **Feature flag** defined for gradual rollout
- [ ] **Rollback plan** documented
- [ ] **Data migration** tested with production-like data volume
- [ ] **Indexes created concurrently** (no table locks)
- [ ] **Large tables** use batched updates
- [ ] **RLS policies** updated alongside schema changes
- [ ] **Application code** deployed before contract phase
- [ ] **Monitoring** in place for migration performance

---

## 🛡️ Safety Guidelines

### ✅ Safe Operations
- Adding nullable columns
- Adding new tables
- Creating indexes **CONCURRENTLY**
- Adding new foreign keys (if not enforced immediately)
- Creating materialized views
- Adding TimescaleDB continuous aggregates

### ⚠️ Potentially Unsafe Operations
- Adding NOT NULL columns (use expand/contract: add nullable → populate → add constraint)
- Dropping columns (use expand/contract: stop using → feature flag → drop)
- Renaming columns (use expand/contract: add new → dual write → drop old)
- Changing column types (requires expand/contract)
- Adding foreign keys with immediate enforcement (use NOT VALID → VALIDATE CONSTRAINT)

### ❌ Unsafe Operations (Require Downtime)
- Dropping tables with active queries
- Changing primary keys
- Removing NOT NULL constraints on indexed columns
- Large-scale data transformations without batching

---

## 🔧 TimescaleDB-Specific Patterns

### Creating Hypertables

```csharp
public partial class CreateSensorReadingsHypertable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create regular table first
        migrationBuilder.CreateTable(
            name: "sensor_readings",
            columns: table => new
            {
                stream_id = table.Column<Guid>(nullable: false),
                ts = table.Column<DateTime>(type: "timestamptz", nullable: false),
                value = table.Column<double>(nullable: false),
                unit = table.Column<string>(maxLength: 50, nullable: false),
                site_id = table.Column<Guid>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_sensor_readings", x => new { x.stream_id, x.ts });
            });

        // Convert to hypertable
        migrationBuilder.Sql(@"
            SELECT create_hypertable(
                'sensor_readings',
                'ts',
                chunk_time_interval => INTERVAL '1 day',
                if_not_exists => TRUE
            );
        ");

        // Add indexes
        migrationBuilder.Sql(@"
            CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_sensor_readings_site_id_ts 
            ON sensor_readings USING BRIN (site_id, ts);
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "sensor_readings");
    }
}
```

### Creating Continuous Aggregates

```csharp
public partial class CreateSensorRollups1m : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE MATERIALIZED VIEW IF NOT EXISTS sensor_rollups_1m
            WITH (timescaledb.continuous) AS
            SELECT 
                site_id,
                stream_id,
                time_bucket('1 minute', ts) AS bucket,
                AVG(value) AS avg_value,
                MIN(value) AS min_value,
                MAX(value) AS max_value,
                COUNT(*) AS sample_count
            FROM sensor_readings
            GROUP BY site_id, stream_id, bucket
            WITH NO DATA;
        ");

        // Add refresh policy
        migrationBuilder.Sql(@"
            SELECT add_continuous_aggregate_policy(
                'sensor_rollups_1m',
                start_offset => INTERVAL '2 hours',
                end_offset => INTERVAL '1 minute',
                schedule_interval => INTERVAL '1 minute'
            );
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS sensor_rollups_1m CASCADE;");
    }
}
```

---

## 🔐 Row-Level Security (RLS)

Always apply RLS policies to new tables:

```csharp
public partial class AddRLSToSensorReadings : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Enable RLS
        migrationBuilder.Sql("ALTER TABLE sensor_readings ENABLE ROW LEVEL SECURITY;");

        // Create policy for site-scoped access
        migrationBuilder.Sql(@"
            CREATE POLICY sensor_readings_site_isolation ON sensor_readings
            FOR ALL
            USING (
                site_id IN (
                    SELECT site_id FROM user_sites
                    WHERE user_id = current_setting('app.current_user_id')::uuid
                )
            );
        ");

        // Allow service accounts to bypass RLS
        migrationBuilder.Sql(@"
            CREATE POLICY sensor_readings_service_account ON sensor_readings
            FOR ALL
            USING (current_setting('app.user_role', TRUE) = 'service_account');
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP POLICY IF EXISTS sensor_readings_site_isolation ON sensor_readings;");
        migrationBuilder.Sql("DROP POLICY IF EXISTS sensor_readings_service_account ON sensor_readings;");
        migrationBuilder.Sql("ALTER TABLE sensor_readings DISABLE ROW LEVEL SECURITY;");
    }
}
```

---

## 📊 Data Migrations

For large data transformations:

```csharp
public partial class MigrateTaskStatusData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Use batching for large updates
        migrationBuilder.Sql(@"
            DO $$
            DECLARE
                batch_size INT := 10000;
                rows_updated INT;
            BEGIN
                LOOP
                    UPDATE tasks
                    SET new_status = 
                        CASE old_status
                            WHEN 'open' THEN 'pending'
                            WHEN 'in_progress' THEN 'active'
                            WHEN 'done' THEN 'completed'
                            ELSE 'unknown'
                        END
                    WHERE new_status IS NULL
                    LIMIT batch_size;

                    GET DIAGNOSTICS rows_updated = ROW_COUNT;
                    EXIT WHEN rows_updated = 0;
                    
                    -- Allow other transactions to proceed
                    COMMIT;
                END LOOP;
            END $$;
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Rollback data migration if needed
        migrationBuilder.Sql(@"
            UPDATE tasks
            SET old_status = 
                CASE new_status
                    WHEN 'pending' THEN 'open'
                    WHEN 'active' THEN 'in_progress'
                    WHEN 'completed' THEN 'done'
                    ELSE NULL
                END
            WHERE old_status IS NULL;
        ");
    }
}
```

---

## 🧪 Testing Migrations

### Unit Tests

Test migrations in isolation:

```csharp
[Fact]
public async Task CreateSensorReadingsHypertable_CreatesTableAndHypertable()
{
    // Arrange
    var migration = new CreateSensorReadingsHypertable();
    
    // Act
    migration.Up(migrationBuilder);
    
    // Assert
    var tableExists = await _context.Database.ExecuteSqlRawAsync(
        "SELECT 1 FROM information_schema.tables WHERE table_name = 'sensor_readings'"
    );
    Assert.True(tableExists > 0);
    
    var isHypertable = await _context.Database.ExecuteSqlRawAsync(
        "SELECT 1 FROM timescaledb_information.hypertables WHERE hypertable_name = 'sensor_readings'"
    );
    Assert.True(isHypertable > 0);
}
```

### Integration Tests

Test complete migration path:

```bash
# Run migrations on test database
./scripts/db/migrate.sh --env test

# Verify schema
./scripts/db/verify-schema.sh --env test

# Rollback and re-apply
./scripts/db/rollback.sh --env test
./scripts/db/migrate.sh --env test
```

---

## 📚 References

- [Entity Framework Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [TimescaleDB Hypertables](https://docs.timescale.com/use-timescale/latest/hypertables/)
- [TimescaleDB Continuous Aggregates](https://docs.timescale.com/use-timescale/latest/continuous-aggregates/)
- [PostgreSQL Row-Level Security](https://www.postgresql.org/docs/current/ddl-rowsecurity.html)
- [Zero-Downtime Migrations](https://fly.io/phoenix-files/zero-downtime-database-migrations/)

---

**✅ Track A Objective:** Enable safe, zero-downtime schema evolution with TimescaleDB support and RLS enforcement.
