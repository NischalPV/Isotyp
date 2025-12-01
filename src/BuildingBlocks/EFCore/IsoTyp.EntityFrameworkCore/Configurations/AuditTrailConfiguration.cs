using IsoTyp.EntityFrameworkCore.Entities;

namespace IsoTyp.EntityFrameworkCore.Configurations;

/// <summary>
/// EF Core configuration for <see cref="AuditTrail"/> entity.
/// </summary>
public class AuditTrailConfiguration : IEntityTypeConfiguration<AuditTrail>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AuditTrail> builder)
    {
        builder.ToTable("AuditTrails");

        builder.HasKey(audit => audit.Id);

        builder.Property(audit => audit.EntityName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(audit => audit.EntityId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(audit => audit.Operation)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(audit => audit.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(audit => audit.PerformedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(audit => audit.PerformedAt)
            .IsRequired();

        builder.Property(audit => audit.IpAddress)
            .HasMaxLength(64);

        builder.Property(audit => audit.UserAgent)
            .HasMaxLength(1024);

        builder.Property(audit => audit.OldValues)
            .HasMaxLength(4096);

        builder.Property(audit => audit.NewValues)
            .HasMaxLength(4096);

        builder.Property(audit => audit.ChangedProperties)
            .HasMaxLength(2048);

        builder.Property(audit => audit.ErrorMessage)
            .HasMaxLength(2048);

        builder.Property(audit => audit.StackTrace)
            .HasMaxLength(4096);

        builder.Property(audit => audit.CorrelationId)
            .HasMaxLength(128);

        builder.Property(audit => audit.Metadata)
            .HasMaxLength(4096);

        builder.Property(audit => audit.TransactionId)
            .HasMaxLength(128);

        builder.Property(audit => audit.DurationMs);
    }
}
