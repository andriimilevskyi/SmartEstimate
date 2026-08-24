using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartEstimate.Domain.Estimates;
using SmartEstimate.Domain.Estimates.ValueObjects;
using SmartEstimate.Domain.Objects;

namespace SmartEstimate.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping for the Estimate aggregate root.
/// </summary>
public sealed class EstimateConfiguration : IEntityTypeConfiguration<Estimate>
{
    public void Configure(EntityTypeBuilder<Estimate> builder)
    {
        builder.ToTable("Estimates");

        builder.HasKey(estimate => estimate.Id);
        builder.Property(estimate => estimate.Id)
            .ValueGeneratedNever();

        builder.Property(estimate => estimate.Number)
            .HasConversion(
                number => number.Value,
                value => new EstimateNumber(value))
            .HasColumnName("EstimateNumber")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(estimate => estimate.ObjectId)
            .IsRequired();

        builder.Property(estimate => estimate.Currency)
            .HasMaxLength(3)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(estimate => estimate.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(estimate => estimate.Notes)
            .HasMaxLength(2_000);

        builder.Property(estimate => estimate.TotalLaborAmount)
            .HasColumnName("TotalLabor")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(estimate => estimate.TotalMaterialsAmount)
            .HasColumnName("TotalMaterials")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(estimate => estimate.GrandTotalAmount)
            .HasColumnName("GrandTotal")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(estimate => estimate.CreatedAt)
            .IsRequired();

        builder.Property(estimate => estimate.UpdatedAt)
            .IsRequired();

        builder.Property(estimate => estimate.DeletedAt);

        builder.Property(estimate => estimate.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(estimate => estimate.Version)
            .IsConcurrencyToken()
            .IsRequired();

        builder.HasIndex(estimate => estimate.Number)
            .HasDatabaseName("IX_Estimates_EstimateNumber_Active")
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(estimate => estimate.CreatedAt)
            .HasDatabaseName("IX_Estimates_CreatedAt");

        builder.HasIndex(estimate => estimate.ObjectId)
            .HasDatabaseName("IX_Estimates_ObjectId");

        builder.HasIndex(estimate => estimate.Status)
            .HasDatabaseName("IX_Estimates_Status");

        builder.HasQueryFilter(estimate => !estimate.IsDeleted);

        builder.HasOne<EstimateObject>()
            .WithMany()
            .HasForeignKey(estimate => estimate.ObjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(estimate => estimate.Zones)
            .WithOne()
            .HasForeignKey(zone => zone.EstimateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(estimate => estimate.Zones)
            .HasField("_zones")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(estimate => estimate.WorkItems)
            .WithOne()
            .HasForeignKey(item => item.EstimateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(estimate => estimate.WorkItems)
            .HasField("_workItems")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(estimate => estimate.MaterialItems)
            .WithOne()
            .HasForeignKey(item => item.EstimateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(estimate => estimate.MaterialItems)
            .HasField("_materialItems")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
