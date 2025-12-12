using Harvestry.AiModels.Domain.Entities;
using Harvestry.AiModels.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.AiModels.Infrastructure.Persistence;

/// <summary>
/// DbContext for graph ML node and edge storage.
/// Uses PostgreSQL with JSONB for flexible properties and array types for feature vectors.
/// </summary>
public sealed class GraphDbContext : DbContext
{
    public GraphDbContext(DbContextOptions<GraphDbContext> options) : base(options)
    {
    }

    public DbSet<GraphNode> Nodes => Set<GraphNode>();
    public DbSet<GraphEdge> Edges => Set<GraphEdge>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureGraphNode(modelBuilder);
        ConfigureGraphEdge(modelBuilder);
    }

    private static void ConfigureGraphNode(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GraphNode>(entity =>
        {
            entity.ToTable("graph_nodes", "ml");

            entity.HasKey(e => e.NodeId);

            entity.Property(e => e.NodeId)
                .HasColumnName("node_id")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.SiteId)
                .HasColumnName("site_id")
                .IsRequired();

            entity.Property(e => e.NodeType)
                .HasColumnName("node_type")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.SourceEntityId)
                .HasColumnName("source_entity_id")
                .IsRequired();

            entity.Property(e => e.Label)
                .HasColumnName("label")
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(e => e.FeatureVector)
                .HasColumnName("feature_vector")
                .HasColumnType("real[]");

            entity.Property(e => e.PropertiesJson)
                .HasColumnName("properties")
                .HasColumnType("jsonb");

            entity.Property(e => e.SourceCreatedAt)
                .HasColumnName("source_created_at")
                .IsRequired();

            entity.Property(e => e.SourceUpdatedAt)
                .HasColumnName("source_updated_at")
                .IsRequired();

            entity.Property(e => e.SnapshotAt)
                .HasColumnName("snapshot_at")
                .IsRequired();

            entity.Property(e => e.Version)
                .HasColumnName("version")
                .IsRequired();

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .IsRequired();

            entity.Property(e => e.AnomalyScore)
                .HasColumnName("anomaly_score");

            entity.Property(e => e.AnomalyExplanation)
                .HasColumnName("anomaly_explanation")
                .HasColumnType("jsonb");

            // Indexes for efficient queries
            entity.HasIndex(e => e.SiteId)
                .HasDatabaseName("ix_graph_nodes_site_id");

            entity.HasIndex(e => new { e.SiteId, e.NodeType })
                .HasDatabaseName("ix_graph_nodes_site_type");

            entity.HasIndex(e => new { e.SiteId, e.NodeType, e.IsActive })
                .HasDatabaseName("ix_graph_nodes_site_type_active");

            entity.HasIndex(e => new { e.NodeType, e.SourceEntityId })
                .HasDatabaseName("ix_graph_nodes_type_source");

            entity.HasIndex(e => new { e.SiteId, e.AnomalyScore })
                .HasDatabaseName("ix_graph_nodes_anomaly")
                .HasFilter("anomaly_score IS NOT NULL");

            entity.HasIndex(e => e.SnapshotAt)
                .HasDatabaseName("ix_graph_nodes_snapshot");
        });
    }

    private static void ConfigureGraphEdge(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GraphEdge>(entity =>
        {
            entity.ToTable("graph_edges", "ml");

            entity.HasKey(e => e.EdgeId);

            entity.Property(e => e.EdgeId)
                .HasColumnName("edge_id")
                .HasMaxLength(300)
                .IsRequired();

            entity.Property(e => e.SiteId)
                .HasColumnName("site_id")
                .IsRequired();

            entity.Property(e => e.EdgeType)
                .HasColumnName("edge_type")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.SourceNodeId)
                .HasColumnName("source_node_id")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.TargetNodeId)
                .HasColumnName("target_node_id")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Weight)
                .HasColumnName("weight")
                .IsRequired();

            entity.Property(e => e.PropertiesJson)
                .HasColumnName("properties")
                .HasColumnType("jsonb");

            entity.Property(e => e.RelationshipCreatedAt)
                .HasColumnName("relationship_created_at")
                .IsRequired();

            entity.Property(e => e.SnapshotAt)
                .HasColumnName("snapshot_at")
                .IsRequired();

            entity.Property(e => e.Version)
                .HasColumnName("version")
                .IsRequired();

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .IsRequired();

            entity.Property(e => e.AnomalyScore)
                .HasColumnName("anomaly_score");

            // Indexes for graph traversal
            entity.HasIndex(e => e.SiteId)
                .HasDatabaseName("ix_graph_edges_site_id");

            entity.HasIndex(e => new { e.SiteId, e.EdgeType })
                .HasDatabaseName("ix_graph_edges_site_type");

            entity.HasIndex(e => new { e.SourceNodeId, e.EdgeType })
                .HasDatabaseName("ix_graph_edges_source_type");

            entity.HasIndex(e => new { e.TargetNodeId, e.EdgeType })
                .HasDatabaseName("ix_graph_edges_target_type");

            entity.HasIndex(e => new { e.SiteId, e.EdgeType, e.IsActive })
                .HasDatabaseName("ix_graph_edges_site_type_active");

            entity.HasIndex(e => e.SnapshotAt)
                .HasDatabaseName("ix_graph_edges_snapshot");

            // Composite index for neighborhood queries
            entity.HasIndex(e => new { e.SourceNodeId, e.TargetNodeId, e.EdgeType })
                .IsUnique()
                .HasDatabaseName("ux_graph_edges_unique");
        });
    }
}
