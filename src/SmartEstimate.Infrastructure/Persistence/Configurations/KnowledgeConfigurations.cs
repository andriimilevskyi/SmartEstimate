using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartEstimate.Domain.Knowledge;

namespace SmartEstimate.Infrastructure.Persistence.Configurations;

public sealed class KnowledgeCategoryConfiguration : IEntityTypeConfiguration<KnowledgeCategory>
{
    public void Configure(EntityTypeBuilder<KnowledgeCategory> builder)
    {
        builder.ToTable("KnowledgeCategories");
        ConfigureRecord(builder);
        builder.Property(value => value.Description).HasMaxLength(4_000);
        builder.Property(value => value.ParentCategoryId);
        builder.OwnsOne(value => value.Name, ConfigureName);
        builder.HasOne<KnowledgeCategory>().WithMany().HasForeignKey(value => value.ParentCategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(value => value.ParentCategoryId);
        builder.HasIndex(value => value.Status);
    }

    internal static void ConfigureRecord<TRecord>(EntityTypeBuilder<TRecord> builder) where TRecord : KnowledgeRecord
    {
        builder.HasKey(value => value.Id);
        builder.Property(value => value.Id).ValueGeneratedNever();
        builder.Property(value => value.Version).IsConcurrencyToken().IsRequired();
        builder.Property(value => value.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(value => value.CreatedAt).IsRequired();
        builder.Property(value => value.UpdatedAt).IsRequired();
        builder.Property(value => value.CreatedBy);
        builder.Property(value => value.UpdatedBy);
    }

    internal static void ConfigureName<TRecord>(OwnedNavigationBuilder<TRecord, LocalizedText> name) where TRecord : class
    {
        name.Property(value => value.Uk).HasColumnName("NameUk").HasMaxLength(256).IsRequired();
        name.Property(value => value.En).HasColumnName("NameEn").HasMaxLength(256).IsRequired();
        name.Property(value => value.De).HasColumnName("NameDe").HasMaxLength(256).IsRequired();
        name.HasIndex(value => value.Uk).IsUnique();
    }
}

public sealed class ConstructionWorkConfiguration : IEntityTypeConfiguration<ConstructionWork>
{
    public void Configure(EntityTypeBuilder<ConstructionWork> builder)
    {
        builder.ToTable("ConstructionWorks");
        KnowledgeCategoryConfiguration.ConfigureRecord(builder);
        builder.OwnsOne(value => value.Name, KnowledgeCategoryConfiguration.ConfigureName);
        builder.Property(value => value.Description).HasMaxLength(4_000);
        builder.Property(value => value.Tags).HasMaxLength(1_500).IsRequired();
        builder.HasOne<KnowledgeCategory>().WithMany().HasForeignKey(value => value.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MeasurementUnit>().WithMany().HasForeignKey(value => value.UnitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(value => value.CategoryId);
        builder.HasIndex(value => value.UnitId);
        builder.HasIndex(value => value.Status);
    }
}

public sealed class KnowledgeMaterialConfiguration : IEntityTypeConfiguration<KnowledgeMaterial>
{
    public void Configure(EntityTypeBuilder<KnowledgeMaterial> builder)
    {
        builder.ToTable("KnowledgeMaterials");
        KnowledgeCategoryConfiguration.ConfigureRecord(builder);
        builder.OwnsOne(value => value.Name, KnowledgeCategoryConfiguration.ConfigureName);
        builder.Property(value => value.Description).HasMaxLength(4_000);
        builder.Property(value => value.Tags).HasMaxLength(1_500).IsRequired();
        builder.HasOne<KnowledgeCategory>().WithMany().HasForeignKey(value => value.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MeasurementUnit>().WithMany().HasForeignKey(value => value.UnitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(value => value.CategoryId);
        builder.HasIndex(value => value.UnitId);
        builder.HasIndex(value => value.Status);
    }
}

public sealed class MeasurementUnitConfiguration : IEntityTypeConfiguration<MeasurementUnit>
{
    public void Configure(EntityTypeBuilder<MeasurementUnit> builder)
    {
        builder.ToTable("MeasurementUnits");
        KnowledgeCategoryConfiguration.ConfigureRecord(builder);
        builder.OwnsOne(value => value.Name, KnowledgeCategoryConfiguration.ConfigureName);
        builder.Property(value => value.Symbol).HasMaxLength(16).IsRequired();
        builder.HasIndex(value => value.Symbol).IsUnique();
        builder.HasIndex(value => value.Status);
    }
}
