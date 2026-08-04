using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartEstimate.Domain.Estimates;
using SmartEstimate.Domain.Estimates.ValueObjects;

namespace SmartEstimate.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping for material lines owned by an Estimate aggregate.
/// </summary>
public sealed class EstimateMaterialItemConfiguration : IEntityTypeConfiguration<EstimateMaterialItem>
{
    public void Configure(EntityTypeBuilder<EstimateMaterialItem> builder)
    {
        builder.ToTable("EstimateMaterialItems");

        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id)
            .ValueGeneratedNever();

        builder.Property(item => item.EstimateId)
            .IsRequired();

        builder.Property(item => item.ZoneId)
            .IsRequired();

        builder.Property(item => item.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(item => item.Quantity)
            .HasConversion(
                quantity => quantity.Value,
                value => new Quantity(value))
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(item => item.MeasurementUnit)
            .HasConversion(
                measurementUnit => measurementUnit.Value,
                value => new MeasurementUnit(value))
            .HasMaxLength(32)
            .IsRequired();

        builder.OwnsOne(item => item.UnitPrice, money =>
        {
            money.Property(value => value.Amount)
                .HasColumnName("UnitPrice")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(value => value.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsRequired();
        });
        builder.Navigation(item => item.UnitPrice).IsRequired();

        builder.Property(item => item.Notes)
            .HasMaxLength(2_000);

        builder.Property(item => item.KnowledgeItemId)
            .HasMaxLength(128);

        builder.Property(item => item.CreatedAt)
            .IsRequired();

        builder.Property(item => item.UpdatedAt)
            .IsRequired();

        builder.HasIndex(item => item.EstimateId)
            .HasDatabaseName("IX_EstimateMaterialItems_EstimateId");

        builder.HasIndex(item => item.ZoneId)
            .HasDatabaseName("IX_EstimateMaterialItems_ZoneId");

        builder.HasOne<EstimateZone>()
            .WithMany()
            .HasForeignKey(item => item.ZoneId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
