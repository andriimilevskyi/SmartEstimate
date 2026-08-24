using SmartEstimate.Application.Pricing;
using SmartEstimate.Application.Pricing.Abstractions;
using SmartEstimate.Domain.Pricing;
using Xunit;

namespace SmartEstimate.UnitTests.Pricing;

public sealed class PricingTests
{
    [Fact]
    public void PriceScopeNormalizesCurrencyRegionAndSupplier()
    {
        var scope = new PriceScope("uah", " ua-32 ", null, " Supplier A ");

        Assert.Equal("UAH", scope.Currency);
        Assert.Equal("UA-32", scope.RegionCode);
        Assert.Equal("Supplier A", scope.SupplierName);
        Assert.True(scope.HasRegion);
        Assert.True(scope.HasSupplier);
    }

    [Fact]
    public async Task ResolverPrefersSupplierAndRegionSpecificPriceOverDefault()
    {
        var target = new PriceTarget(PriceTargetType.Material, Guid.NewGuid());
        var effectiveFrom = new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero);
        var repository = new InMemoryPriceRepository(
            CatalogPrice.Create(Guid.NewGuid(), target, 100m, new PriceScope("UAH", null, null, null), effectiveFrom, PriceSourceType.Manual, null, effectiveFrom),
            CatalogPrice.Create(Guid.NewGuid(), target, 130m, new PriceScope("UAH", "UA-32", null, null), effectiveFrom, PriceSourceType.Manual, null, effectiveFrom),
            CatalogPrice.Create(Guid.NewGuid(), target, 145m, new PriceScope("UAH", "UA-32", null, "Supplier A"), effectiveFrom, PriceSourceType.Manual, null, effectiveFrom));
        var resolver = new PriceResolver(repository);

        var resolved = await resolver.GetCurrentPriceAsync(
            target,
            "UAH",
            "UA-32",
            null,
            "Supplier A",
            effectiveFrom.AddDays(10),
            CancellationToken.None);

        Assert.NotNull(resolved);
        Assert.Equal(145m, resolved.Amount);
        Assert.Equal("Supplier A", resolved.SupplierName);
        Assert.Equal("UA-32", resolved.RegionCode);
    }

    [Fact]
    public async Task ResolverDoesNotUseSupplierSpecificPriceWithoutSupplierRequest()
    {
        var target = new PriceTarget(PriceTargetType.ConstructionWork, Guid.NewGuid());
        var effectiveFrom = new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero);
        var repository = new InMemoryPriceRepository(
            CatalogPrice.Create(Guid.NewGuid(), target, 180m, new PriceScope("UAH", null, null, null), effectiveFrom, PriceSourceType.Manual, null, effectiveFrom),
            CatalogPrice.Create(Guid.NewGuid(), target, 160m, new PriceScope("UAH", null, null, "Supplier A"), effectiveFrom, PriceSourceType.Manual, null, effectiveFrom));
        var resolver = new PriceResolver(repository);

        var resolved = await resolver.GetCurrentPriceAsync(
            target,
            "UAH",
            null,
            null,
            null,
            effectiveFrom.AddDays(1),
            CancellationToken.None);

        Assert.NotNull(resolved);
        Assert.Equal(180m, resolved.Amount);
        Assert.Null(resolved.SupplierName);
    }

    private sealed class InMemoryPriceRepository(params CatalogPrice[] seed) : ICatalogPriceRepository
    {
        public Task<CatalogPrice?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(seed.SingleOrDefault(price => price.Id == id));

        public Task<IReadOnlyCollection<CatalogPrice>> GetByTargetIdsAsync(
            PriceTargetType targetType,
            Guid[] targetIds,
            string? currency,
            string? supplier,
            string? regionCode,
            DateTimeOffset effectiveDate,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<CatalogPrice>>(seed
                .Where(price => price.TargetType == targetType && targetIds.Contains(price.TargetId))
                .ToArray());

        public Task<IReadOnlyCollection<CatalogPrice>> GetHistoryAsync(PriceTarget target, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<CatalogPrice>>(seed.Where(price => price.TargetId == target.Id).ToArray());

        public Task<IReadOnlyCollection<CatalogPrice>> GetOpenPricesInScopeAsync(
            PriceTarget target,
            PriceScope scope,
            DateTimeOffset effectiveFrom,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<CatalogPrice>>([]);

        public Task<IReadOnlyCollection<CatalogPriceHistoryEntry>> GetHistoryEventsAsync(PriceTarget target, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<CatalogPriceHistoryEntry>>([]);

        public Task AddAsync(CatalogPrice price, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task AddHistoryAsync(CatalogPriceHistoryEntry entry, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
