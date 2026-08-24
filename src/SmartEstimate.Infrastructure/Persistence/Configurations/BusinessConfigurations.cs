using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartEstimate.Domain.Customers;
using SmartEstimate.Domain.Objects;

namespace SmartEstimate.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(customer => customer.Id);
        builder.Property(customer => customer.Id).ValueGeneratedNever();
        builder.Property(customer => customer.Name).HasMaxLength(256).IsRequired();
        builder.Property(customer => customer.Phone).HasMaxLength(64);
        builder.Property(customer => customer.Email).HasMaxLength(256);
        builder.Property(customer => customer.Note).HasMaxLength(2_000);
        builder.Property(customer => customer.CreatedAt).IsRequired();
        builder.Property(customer => customer.UpdatedAt).IsRequired();
        builder.Property(customer => customer.DeletedAt);
        builder.Property(customer => customer.IsDeleted).HasDefaultValue(false).IsRequired();
        builder.Property(customer => customer.Version).IsConcurrencyToken().IsRequired();

        builder.HasIndex(customer => customer.Name).HasDatabaseName("IX_Customers_Name");
        builder.HasIndex(customer => customer.Phone).HasDatabaseName("IX_Customers_Phone");
        builder.HasQueryFilter(customer => !customer.IsDeleted);
    }
}

public sealed class EstimateObjectConfiguration : IEntityTypeConfiguration<EstimateObject>
{
    public void Configure(EntityTypeBuilder<EstimateObject> builder)
    {
        builder.ToTable("EstimateObjects");

        builder.HasKey(value => value.Id);
        builder.Property(value => value.Id).ValueGeneratedNever();
        builder.Property(value => value.CustomerId).IsRequired();
        builder.Property(value => value.Name).HasMaxLength(256).IsRequired();
        builder.Property(value => value.ObjectType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(value => value.Address).HasMaxLength(512);
        builder.Property(value => value.TotalArea).HasPrecision(18, 2);
        builder.Property(value => value.Description).HasMaxLength(2_000);
        builder.Property(value => value.CreatedAt).IsRequired();
        builder.Property(value => value.UpdatedAt).IsRequired();
        builder.Property(value => value.DeletedAt);
        builder.Property(value => value.IsDeleted).HasDefaultValue(false).IsRequired();
        builder.Property(value => value.Version).IsConcurrencyToken().IsRequired();

        builder.HasIndex(value => value.CustomerId).HasDatabaseName("IX_EstimateObjects_CustomerId");
        builder.HasIndex(value => value.Name).HasDatabaseName("IX_EstimateObjects_Name");
        builder.HasIndex(value => value.Address).HasDatabaseName("IX_EstimateObjects_Address");
        builder.HasIndex(value => value.ObjectType).HasDatabaseName("IX_EstimateObjects_ObjectType");
        builder.HasQueryFilter(value => !value.IsDeleted);

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(value => value.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
