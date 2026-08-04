using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartEstimate.Domain.Estimates;

namespace SmartEstimate.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping for user-editable estimate zones.
/// </summary>
public sealed class EstimateZoneConfiguration : IEntityTypeConfiguration<EstimateZone>
{
    public void Configure(EntityTypeBuilder<EstimateZone> builder)
    {
        builder.ToTable("EstimateZones");

        builder.HasKey(zone => zone.Id);
        builder.Property(zone => zone.Id)
            .ValueGeneratedNever();

        builder.Property(zone => zone.EstimateId)
            .IsRequired();

        builder.Property(zone => zone.Name)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(zone => zone.SortOrder)
            .IsRequired();

        builder.Property(zone => zone.CreatedAt)
            .IsRequired();

        builder.Property(zone => zone.UpdatedAt)
            .IsRequired();

        builder.HasIndex(zone => zone.EstimateId)
            .HasDatabaseName("IX_EstimateZones_EstimateId");

        builder.HasIndex(zone => new { zone.EstimateId, zone.Name })
            .HasDatabaseName("IX_EstimateZones_EstimateId_Name")
            .IsUnique();
    }
}
