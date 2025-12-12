using Harvestry.Labor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Labor.Infrastructure.Persistence;

/// <summary>
/// DbContext for Labor domain
/// </summary>
public class LaborDbContext : DbContext
{
    public LaborDbContext(DbContextOptions<LaborDbContext> options) : base(options)
    {
    }

    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureTeam(modelBuilder);
        ConfigureTeamMember(modelBuilder);
    }

    private static void ConfigureTeam(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Team>(entity =>
        {
            entity.ToTable("teams");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("team_id");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            // Navigation to members
            entity.HasMany(e => e.Members)
                .WithOne()
                .HasForeignKey(m => m.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ignore the backing field for EF
            entity.Ignore(e => e.ActiveMembers);
            entity.Ignore(e => e.TeamLeads);

            // Indexes
            entity.HasIndex(e => e.SiteId);
            entity.HasIndex(e => new { e.SiteId, e.Name })
                .IsUnique()
                .HasFilter("status = 'Active'");
        });
    }

    private static void ConfigureTeamMember(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TeamMember>(entity =>
        {
            entity.ToTable("team_members");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("team_member_id");
            entity.Property(e => e.TeamId).HasColumnName("team_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.IsTeamLead).HasColumnName("is_team_lead");
            entity.Property(e => e.JoinedAt).HasColumnName("joined_at");
            entity.Property(e => e.RemovedAt).HasColumnName("removed_at");
            entity.Property(e => e.RemovedBy).HasColumnName("removed_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            // Indexes
            entity.HasIndex(e => new { e.TeamId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.TeamId, e.IsTeamLead })
                .HasFilter("is_team_lead = true AND removed_at IS NULL");
        });
    }
}
