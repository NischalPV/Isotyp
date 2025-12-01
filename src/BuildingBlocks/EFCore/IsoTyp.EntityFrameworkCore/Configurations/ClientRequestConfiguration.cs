using IsoTyp.EntityFrameworkCore.Entities;

namespace IsoTyp.EntityFrameworkCore.Configurations;

/// <summary>
/// EF Core configuration for <see cref="ClientRequest"/> entity.
/// </summary>
public class ClientRequestConfiguration : IEntityTypeConfiguration<ClientRequest>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ClientRequest> builder)
    {
        builder.ToTable("ClientRequests");

        builder.HasKey(request => request.Id);

        builder.Property(request => request.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(request => request.CreatedAt)
            .IsRequired();

        builder.Property(request => request.Payload)
            .HasMaxLength(2048);

        builder.Property(request => request.Owner)
            .HasMaxLength(256);

        builder.HasIndex(request => request.Name)
            .IsUnique()
            .HasDatabaseName("IX_ClientRequest_Name");
    }
}
