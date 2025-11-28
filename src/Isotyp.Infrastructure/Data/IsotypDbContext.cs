using Microsoft.EntityFrameworkCore;
using Isotyp.Core.Entities;

namespace Isotyp.Infrastructure.Data;

/// <summary>
/// Database context for the Isotyp platform.
/// </summary>
public class IsotypDbContext : DbContext
{
    public IsotypDbContext(DbContextOptions<IsotypDbContext> options) : base(options)
    {
    }

    public DbSet<DataSource> DataSources => Set<DataSource>();
    public DbSet<SchemaVersion> SchemaVersions => Set<SchemaVersion>();
    public DbSet<SchemaChangeRequest> SchemaChangeRequests => Set<SchemaChangeRequest>();
    public DbSet<SchemaChangeApproval> SchemaChangeApprovals => Set<SchemaChangeApproval>();
    public DbSet<AiSuggestion> AiSuggestions => Set<AiSuggestion>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<DataValidation> DataValidations => Set<DataValidation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // DataSource configuration
        modelBuilder.Entity<DataSource>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.ConnectionStringReference).IsRequired().HasMaxLength(500);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            entity.Property(e => e.UpdatedBy).HasMaxLength(200);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // SchemaVersion configuration
        modelBuilder.Entity<SchemaVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SchemaDefinition).IsRequired();
            entity.Property(e => e.ChangeDescription).HasMaxLength(5000);
            entity.Property(e => e.OrmMappings).IsRequired();
            entity.Property(e => e.MigrationScript).IsRequired();
            entity.Property(e => e.RollbackScript).IsRequired();
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            entity.Property(e => e.UpdatedBy).HasMaxLength(200);
            entity.Property(e => e.AppliedBy).HasMaxLength(200);
            entity.Property(e => e.RolledBackBy).HasMaxLength(200);
            entity.HasIndex(e => new { e.DataSourceId, e.MajorVersion, e.MinorVersion, e.PatchVersion }).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // SchemaChangeRequest configuration
        modelBuilder.Entity<SchemaChangeRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ChangeDetails).IsRequired();
            entity.Property(e => e.Description).IsRequired().HasMaxLength(5000);
            entity.Property(e => e.Justification).HasMaxLength(5000);
            entity.Property(e => e.ImpactAnalysis).HasMaxLength(10000);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            entity.Property(e => e.UpdatedBy).HasMaxLength(200);
            entity.HasMany(e => e.Approvals).WithOne().HasForeignKey(a => a.SchemaChangeRequestId);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // SchemaChangeApproval configuration
        modelBuilder.Entity<SchemaChangeApproval>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ApproverUserId).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ApproverName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Comments).HasMaxLength(5000);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            entity.Property(e => e.UpdatedBy).HasMaxLength(200);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // AiSuggestion configuration
        modelBuilder.Entity<AiSuggestion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SuggestionDetails).IsRequired();
            entity.Property(e => e.Reasoning).HasMaxLength(5000);
            entity.Property(e => e.TriggeringPatterns).HasMaxLength(5000);
            entity.Property(e => e.ReviewedBy).HasMaxLength(200);
            entity.Property(e => e.ReviewComments).HasMaxLength(5000);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            entity.Property(e => e.UpdatedBy).HasMaxLength(200);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // AuditLog configuration - immutable, no soft delete
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(200);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(200);
            entity.Property(e => e.UserName).HasMaxLength(200);
            entity.Property(e => e.ClientIdentifier).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.EntityType);
            entity.HasIndex(e => e.EntityId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CorrelationId);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Agent configuration
        modelBuilder.Entity<Agent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.AgentKey).IsRequired().HasMaxLength(500);
            entity.Property(e => e.AgentVersion).IsRequired().HasMaxLength(50);
            entity.Property(e => e.HostName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.AuthorizedDataSourceIds).HasMaxLength(5000);
            entity.Property(e => e.Configuration).HasMaxLength(10000);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            entity.Property(e => e.UpdatedBy).HasMaxLength(200);
            entity.HasIndex(e => e.AgentKey).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // DataValidation configuration
        modelBuilder.Entity<DataValidation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Errors).HasMaxLength(50000);
            entity.Property(e => e.Warnings).HasMaxLength(50000);
            entity.Property(e => e.Summary).HasMaxLength(5000);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            entity.Property(e => e.UpdatedBy).HasMaxLength(200);
            entity.HasIndex(e => e.DataSourceId);
            entity.HasIndex(e => e.SchemaVersionId);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }
}
