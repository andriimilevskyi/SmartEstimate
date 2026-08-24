using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartEstimate.Domain.Knowledge;
using SmartEstimate.Domain.Pricing;

namespace SmartEstimate.Infrastructure.Persistence.Configurations;

public sealed class CatalogPriceConfiguration : IEntityTypeConfiguration<CatalogPrice>
{
    public void Configure(EntityTypeBuilder<CatalogPrice> builder)
    {
        builder.ToTable("CatalogPrices", table =>
        {
            table.HasCheckConstraint(
                "CK_CatalogPrices_Target",
                """
                ("TargetType" = 'Material' AND "KnowledgeMaterialId" IS NOT NULL AND "ConstructionWorkId" IS NULL)
                OR ("TargetType" = 'ConstructionWork' AND "ConstructionWorkId" IS NOT NULL AND "KnowledgeMaterialId" IS NULL)
                """);
            table.HasCheckConstraint("CK_CatalogPrices_EffectiveRange", "\"EffectiveUntil\" IS NULL OR \"EffectiveUntil\" > \"EffectiveFrom\"");
        });

        builder.HasKey(price => price.Id);
        builder.Property(price => price.Id).ValueGeneratedNever();
        builder.Property(price => price.TargetType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(price => price.KnowledgeMaterialId);
        builder.Property(price => price.ConstructionWorkId);
        builder.Property(price => price.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(price => price.Currency).HasMaxLength(3).IsUnicode(false).IsRequired();
        builder.Property(price => price.RegionCode).HasMaxLength(64).IsUnicode(false);
        builder.Property(price => price.SupplierId);
        builder.Property(price => price.SupplierName).HasMaxLength(256);
        builder.Property(price => price.EffectiveFrom).IsRequired();
        builder.Property(price => price.EffectiveUntil);
        builder.Property(price => price.SourceType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(price => price.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(price => price.Notes).HasMaxLength(1_000);
        builder.Property(price => price.CreatedAt).IsRequired();
        builder.Property(price => price.UpdatedAt).IsRequired();
        builder.Property(price => price.ArchivedAt);
        builder.Property(price => price.CreatedBy);
        builder.Property(price => price.UpdatedBy);
        builder.Property(price => price.Version).IsConcurrencyToken().IsRequired();

        builder.Ignore(price => price.TargetId);
        builder.Ignore(price => price.Scope);

        builder.HasOne<KnowledgeMaterial>()
            .WithMany()
            .HasForeignKey(price => price.KnowledgeMaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ConstructionWork>()
            .WithMany()
            .HasForeignKey(price => price.ConstructionWorkId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(price => new { price.TargetType, price.KnowledgeMaterialId, price.Currency, price.Status, price.EffectiveFrom })
            .HasDatabaseName("IX_CatalogPrices_Material_Current");
        builder.HasIndex(price => new { price.TargetType, price.ConstructionWorkId, price.Currency, price.Status, price.EffectiveFrom })
            .HasDatabaseName("IX_CatalogPrices_Work_Current");
        builder.HasIndex(price => price.RegionCode).HasDatabaseName("IX_CatalogPrices_RegionCode");
        builder.HasIndex(price => price.SupplierName).HasDatabaseName("IX_CatalogPrices_SupplierName");
    }
}

public sealed class CatalogPriceHistoryEntryConfiguration : IEntityTypeConfiguration<CatalogPriceHistoryEntry>
{
    public void Configure(EntityTypeBuilder<CatalogPriceHistoryEntry> builder)
    {
        builder.ToTable("CatalogPriceHistory");
        builder.HasKey(entry => entry.Id);
        builder.Property(entry => entry.Id).ValueGeneratedNever();
        builder.Property(entry => entry.CatalogPriceId).IsRequired();
        builder.Property(entry => entry.TargetType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(entry => entry.KnowledgeMaterialId);
        builder.Property(entry => entry.ConstructionWorkId);
        builder.Property(entry => entry.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(entry => entry.Currency).HasMaxLength(3).IsUnicode(false).IsRequired();
        builder.Property(entry => entry.RegionCode).HasMaxLength(64).IsUnicode(false);
        builder.Property(entry => entry.SupplierId);
        builder.Property(entry => entry.SupplierName).HasMaxLength(256);
        builder.Property(entry => entry.EffectiveFrom).IsRequired();
        builder.Property(entry => entry.EffectiveUntil);
        builder.Property(entry => entry.SourceType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(entry => entry.PriceStatus).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(entry => entry.Notes).HasMaxLength(1_000);
        builder.Property(entry => entry.ChangeType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(entry => entry.ChangedAt).IsRequired();
        builder.Property(entry => entry.ChangedBy);

        builder.HasOne<CatalogPrice>()
            .WithMany()
            .HasForeignKey(entry => entry.CatalogPriceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(entry => entry.CatalogPriceId).HasDatabaseName("IX_CatalogPriceHistory_CatalogPriceId");
        builder.HasIndex(entry => new { entry.TargetType, entry.KnowledgeMaterialId, entry.ChangedAt })
            .HasDatabaseName("IX_CatalogPriceHistory_Material");
        builder.HasIndex(entry => new { entry.TargetType, entry.ConstructionWorkId, entry.ChangedAt })
            .HasDatabaseName("IX_CatalogPriceHistory_Work");
    }
}
