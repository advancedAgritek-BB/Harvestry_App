using Harvestry.Telemetry.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Harvestry.Telemetry.Infrastructure.Persistence;

/// <summary>
/// Database context for telemetry service.
/// Manages sensor streams, readings, alerts, and ingestion tracking.
/// </summary>
public class TelemetryDbContext : DbContext
{
    private readonly NpgsqlDataSource _dataSource;
    
    public TelemetryDbContext(DbContextOptions<TelemetryDbContext> options, NpgsqlDataSource dataSource)
        : base(options)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
    }
    
    // DbSets
    public DbSet<SensorStream> SensorStreams => Set<SensorStream>();
    public DbSet<SensorReading> SensorReadings => Set<SensorReading>();
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();
    public DbSet<AlertInstance> AlertInstances => Set<AlertInstance>();
    public DbSet<IngestionSession> IngestionSessions => Set<IngestionSession>();
    public DbSet<IngestionError> IngestionErrors => Set<IngestionError>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        ConfigureSensorStream(modelBuilder);
        ConfigureSensorReading(modelBuilder);
        ConfigureAlertRule(modelBuilder);
        ConfigureAlertInstance(modelBuilder);
        ConfigureIngestionSession(modelBuilder);
        ConfigureIngestionError(modelBuilder);
    }
    
    private void ConfigureSensorStream(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SensorStream>(entity =>
        {
            entity.ToTable("sensor_streams");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();
                
            entity.Property(e => e.SiteId)
                .HasColumnName("site_id")
                .IsRequired();
                
            entity.Property(e => e.EquipmentId)
                .HasColumnName("equipment_id")
                .IsRequired();
                
            entity.Property(e => e.EquipmentChannelId)
                .HasColumnName("equipment_channel_id");
                
            entity.Property(e => e.StreamType)
                .HasColumnName("stream_type")
                .HasConversion<string>()
                .IsRequired();
                
            entity.Property(e => e.Unit)
                .HasColumnName("unit")
                .HasConversion<string>()
                .IsRequired();
                
            entity.Property(e => e.DisplayName)
                .HasColumnName("display_name")
                .HasMaxLength(200)
                .IsRequired();
                
            entity.Property(e => e.LocationId)
                .HasColumnName("location_id");
                
            entity.Property(e => e.RoomId)
                .HasColumnName("room_id");
                
            entity.Property(e => e.ZoneId)
                .HasColumnName("zone_id");
                
            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .IsRequired();
                
            entity.Property(e => e.Metadata)
                .HasColumnName("metadata")
                .HasColumnType("jsonb");
                
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();
                
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();
                
            // Indexes
            entity.HasIndex(e => e.SiteId);
            entity.HasIndex(e => e.EquipmentId);
            entity.HasIndex(e => new { e.SiteId, e.IsActive });
        });
    }
    
    private void ConfigureSensorReading(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SensorReading>(entity =>
        {
            entity.ToTable("sensor_readings");
            
            // Composite key (time, stream_id) for TimescaleDB hypertable
            entity.HasKey(e => new { e.Time, e.StreamId });
            
            entity.Property(e => e.Time)
                .HasColumnName("time")
                .IsRequired();
                
            entity.Property(e => e.StreamId)
                .HasColumnName("stream_id")
                .IsRequired();
                
            entity.Property(e => e.Value)
                .HasColumnName("value")
                .IsRequired();
                
            entity.Property(e => e.QualityCode)
                .HasColumnName("quality_code")
                .HasConversion<short>()
                .IsRequired();
                
            entity.Property(e => e.SourceTimestamp)
                .HasColumnName("source_timestamp");
                
            entity.Property(e => e.IngestionTimestamp)
                .HasColumnName("ingestion_timestamp")
                .IsRequired();
                
            entity.Property(e => e.MessageId)
                .HasColumnName("message_id")
                .HasMaxLength(100);
                
            entity.Property(e => e.Metadata)
                .HasColumnName("metadata")
                .HasColumnType("jsonb");
                
            // Indexes (in addition to primary key on time, stream_id)
            entity.HasIndex(e => new { e.StreamId, e.Time });
            entity.HasIndex(e => new { e.StreamId, e.MessageId })
                .IsUnique()
                .HasFilter("message_id IS NOT NULL");
        });
    }
    
    private void ConfigureAlertRule(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AlertRule>(entity =>
        {
            entity.ToTable("alert_rules");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();
                
            entity.Property(e => e.SiteId)
                .HasColumnName("site_id")
                .IsRequired();
                
            entity.Property(e => e.RuleName)
                .HasColumnName("rule_name")
                .HasMaxLength(200)
                .IsRequired();
                
            entity.Property(e => e.RuleType)
                .HasColumnName("rule_type")
                .HasConversion<string>()
                .IsRequired();
                
            entity.Property(e => e.StreamIds)
                .HasColumnName("stream_ids")
                .HasColumnType("uuid[]")
                .IsRequired();
                
            entity.Property(e => e.ThresholdConfig)
                .HasColumnName("threshold_config")
                .HasColumnType("jsonb")
                .IsRequired();
                
            entity.Property(e => e.EvaluationWindowMinutes)
                .HasColumnName("evaluation_window_minutes")
                .IsRequired();
                
            entity.Property(e => e.CooldownMinutes)
                .HasColumnName("cooldown_minutes")
                .IsRequired();
                
            entity.Property(e => e.Severity)
                .HasColumnName("severity")
                .HasConversion<string>()
                .IsRequired();
                
            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .IsRequired();
                
            entity.Property(e => e.NotifyChannels)
                .HasColumnName("notify_channels")
                .HasColumnType("text[]")
                .IsRequired();
                
            entity.Property(e => e.Metadata)
                .HasColumnName("metadata")
                .HasColumnType("jsonb");
                
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();
                
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();
                
            entity.Property(e => e.CreatedBy)
                .HasColumnName("created_by")
                .IsRequired();
                
            entity.Property(e => e.UpdatedBy)
                .HasColumnName("updated_by")
                .IsRequired();
                
            // Indexes
            entity.HasIndex(e => new { e.SiteId, e.IsActive });
        });
    }
    
    private void ConfigureAlertInstance(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AlertInstance>(entity =>
        {
            entity.ToTable("alert_instances");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();
                
            entity.Property(e => e.SiteId)
                .HasColumnName("site_id")
                .IsRequired();
                
            entity.Property(e => e.RuleId)
                .HasColumnName("rule_id")
                .IsRequired();
                
            entity.Property(e => e.StreamId)
                .HasColumnName("stream_id")
                .IsRequired();
                
            entity.Property(e => e.FiredAt)
                .HasColumnName("fired_at")
                .IsRequired();
                
            entity.Property(e => e.ClearedAt)
                .HasColumnName("cleared_at");
                
            entity.Property(e => e.Severity)
                .HasColumnName("severity")
                .HasConversion<string>()
                .IsRequired();
                
            entity.Property(e => e.CurrentValue)
                .HasColumnName("current_value");
                
            entity.Property(e => e.ThresholdValue)
                .HasColumnName("threshold_value");
                
            entity.Property(e => e.Message)
                .HasColumnName("message")
                .IsRequired();
                
            entity.Property(e => e.AcknowledgedAt)
                .HasColumnName("acknowledged_at");
                
            entity.Property(e => e.AcknowledgedBy)
                .HasColumnName("acknowledged_by");
                
            entity.Property(e => e.AcknowledgmentNotes)
                .HasColumnName("acknowledgment_notes");
                
            entity.Property(e => e.Metadata)
                .HasColumnName("metadata")
                .HasColumnType("jsonb");
                
            // Indexes
            entity.HasIndex(e => new { e.RuleId, e.FiredAt });
            entity.HasIndex(e => new { e.SiteId, e.FiredAt })
                .HasFilter("cleared_at IS NULL");
            entity.HasIndex(e => new { e.StreamId, e.FiredAt });
        });
    }
    
    private void ConfigureIngestionSession(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IngestionSession>(entity =>
        {
            entity.ToTable("ingestion_sessions");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();
                
            entity.Property(e => e.SiteId)
                .HasColumnName("site_id")
                .IsRequired();
                
            entity.Property(e => e.EquipmentId)
                .HasColumnName("equipment_id")
                .IsRequired();
                
            entity.Property(e => e.Protocol)
                .HasColumnName("protocol")
                .HasConversion<string>()
                .IsRequired();
                
            entity.Property(e => e.StartedAt)
                .HasColumnName("started_at")
                .IsRequired();
                
            entity.Property(e => e.LastHeartbeatAt)
                .HasColumnName("last_heartbeat_at")
                .IsRequired();
                
            entity.Property(e => e.EndedAt)
                .HasColumnName("ended_at");
                
            entity.Property(e => e.MessageCount)
                .HasColumnName("message_count")
                .IsRequired();
                
            entity.Property(e => e.ErrorCount)
                .HasColumnName("error_count")
                .IsRequired();
                
            entity.Property(e => e.Metadata)
                .HasColumnName("metadata")
                .HasColumnType("jsonb");
                
            // Indexes
            entity.HasIndex(e => new { e.EquipmentId, e.StartedAt });
            entity.HasIndex(e => new { e.SiteId, e.LastHeartbeatAt })
                .HasFilter("ended_at IS NULL");
        });
    }
    
    private void ConfigureIngestionError(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IngestionError>(entity =>
        {
            entity.ToTable("ingestion_errors");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();
                
            entity.Property(e => e.SiteId)
                .HasColumnName("site_id")
                .IsRequired();
                
            entity.Property(e => e.SessionId)
                .HasColumnName("session_id");
                
            entity.Property(e => e.EquipmentId)
                .HasColumnName("equipment_id");
                
            entity.Property(e => e.Protocol)
                .HasColumnName("protocol")
                .HasConversion<string>()
                .IsRequired();
                
            entity.Property(e => e.ErrorType)
                .HasColumnName("error_type")
                .HasConversion<string>()
                .IsRequired();
                
            entity.Property(e => e.ErrorMessage)
                .HasColumnName("error_message")
                .IsRequired();
                
            entity.Property(e => e.RawPayload)
                .HasColumnName("raw_payload")
                .HasColumnType("jsonb");
                
            entity.Property(e => e.OccurredAt)
                .HasColumnName("occurred_at")
                .IsRequired();
                
            // Index
            entity.HasIndex(e => new { e.SiteId, e.OccurredAt });
        });
    }
}

