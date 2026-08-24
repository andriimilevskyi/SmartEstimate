using Microsoft.EntityFrameworkCore;
using SmartEstimate.Domain.Customers;
using SmartEstimate.Domain.Estimates;
using SmartEstimate.Domain.Knowledge;
using SmartEstimate.Domain.Objects;
using SmartEstimate.Domain.Pricing;

namespace SmartEstimate.Infrastructure.Persistence;

/// <summary>
/// The EF Core entry point for SmartEstimate persistence.
/// Domain DbSets and configurations are deliberately introduced with their modules.
/// </summary>
public sealed class SmartEstimateDbContext(DbContextOptions<SmartEstimateDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Estimates managed by the Estimate Core module.
    /// </summary>
    public DbSet<Estimate> Estimates => Set<Estimate>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<EstimateObject> EstimateObjects => Set<EstimateObject>();

    public DbSet<EstimateZone> EstimateZones => Set<EstimateZone>();

    public DbSet<KnowledgeCategory> KnowledgeCategories => Set<KnowledgeCategory>();

    public DbSet<ConstructionWork> ConstructionWorks => Set<ConstructionWork>();

    public DbSet<KnowledgeMaterial> KnowledgeMaterials => Set<KnowledgeMaterial>();

    public DbSet<MeasurementUnit> MeasurementUnits => Set<MeasurementUnit>();

    public DbSet<CatalogPrice> CatalogPrices => Set<CatalogPrice>();

    public DbSet<CatalogPriceHistoryEntry> CatalogPriceHistory => Set<CatalogPriceHistoryEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartEstimateDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
