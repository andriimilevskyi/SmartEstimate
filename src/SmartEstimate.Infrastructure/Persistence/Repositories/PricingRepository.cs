using Microsoft.EntityFrameworkCore;
using SmartEstimate.Application.Pricing.Abstractions;
using SmartEstimate.Domain.Pricing;

namespace SmartEstimate.Infrastructure.Persistence.Repositories;

public sealed class PricingRepository(SmartEstimateDbContext dbContext) : ICatalogPriceRepository
{
    public async Task<CatalogPrice?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.CatalogPrices.SingleOrDefaultAsync(price => price.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<CatalogPrice>> GetByTargetIdsAsync(
        PriceTargetType targetType,
        Guid[] targetIds,
        string? currency,
        string? supplier,
        string? regionCode,
        DateTimeOffset effectiveDate,
        CancellationToken cancellationToken)
    {
        if (targetIds.Length == 0)
        {
            return [];
        }

        var normalizedCurrency = string.IsNullOrWhiteSpace(currency) ? null : new PriceScope(currency, null, null, null).Currency;
        var normalizedRegion = string.IsNullOrWhiteSpace(regionCode) ? null : new PriceScope(normalizedCurrency ?? "UAH", regionCode, null, null).RegionCode;
        var normalizedSupplier = string.IsNullOrWhiteSpace(supplier) ? null : supplier.Trim();

        var source = dbContext.CatalogPrices
            .Where(price => price.TargetType == targetType
                && price.Status == PriceStatus.Active
                && price.EffectiveFrom <= effectiveDate
                && (!price.EffectiveUntil.HasValue || price.EffectiveUntil > effectiveDate));

        source = targetType == PriceTargetType.Material
            ? source.Where(price => price.KnowledgeMaterialId.HasValue && targetIds.Contains(price.KnowledgeMaterialId.Value))
            : source.Where(price => price.ConstructionWorkId.HasValue && targetIds.Contains(price.ConstructionWorkId.Value));

        if (normalizedCurrency is not null)
        {
            source = source.Where(price => price.Currency == normalizedCurrency);
        }

        if (normalizedRegion is not null)
        {
            source = source.Where(price => price.RegionCode == normalizedRegion || price.RegionCode == null);
        }
        else
        {
            source = source.Where(price => price.RegionCode == null);
        }

        if (normalizedSupplier is not null)
        {
            source = source.Where(price => price.SupplierName == normalizedSupplier || price.SupplierName == null);
        }
        else
        {
            source = source.Where(price => price.SupplierId == null && price.SupplierName == null);
        }

        return await source
            .OrderByDescending(price => price.EffectiveFrom)
            .ThenByDescending(price => price.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<CatalogPrice>> GetHistoryAsync(PriceTarget target, CancellationToken cancellationToken)
    {
        var source = dbContext.CatalogPrices.Where(price => price.TargetType == target.Type);
        source = target.Type == PriceTargetType.Material
            ? source.Where(price => price.KnowledgeMaterialId == target.Id)
            : source.Where(price => price.ConstructionWorkId == target.Id);

        return await source
            .OrderByDescending(price => price.EffectiveFrom)
            .ThenByDescending(price => price.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<CatalogPrice>> GetOpenPricesInScopeAsync(
        PriceTarget target,
        PriceScope scope,
        DateTimeOffset effectiveFrom,
        CancellationToken cancellationToken)
    {
        var source = dbContext.CatalogPrices.Where(price =>
            price.TargetType == target.Type
            && price.Currency == scope.Currency
            && price.RegionCode == scope.RegionCode
            && price.SupplierId == scope.SupplierId
            && price.SupplierName == scope.SupplierName
            && price.Status == PriceStatus.Active
            && price.EffectiveFrom < effectiveFrom
            && price.EffectiveUntil == null);

        source = target.Type == PriceTargetType.Material
            ? source.Where(price => price.KnowledgeMaterialId == target.Id)
            : source.Where(price => price.ConstructionWorkId == target.Id);

        return await source.ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<CatalogPriceHistoryEntry>> GetHistoryEventsAsync(PriceTarget target, CancellationToken cancellationToken)
    {
        var source = dbContext.CatalogPriceHistory.Where(entry => entry.TargetType == target.Type);
        source = target.Type == PriceTargetType.Material
            ? source.Where(entry => entry.KnowledgeMaterialId == target.Id)
            : source.Where(entry => entry.ConstructionWorkId == target.Id);

        return await source
            .OrderByDescending(entry => entry.ChangedAt)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddAsync(CatalogPrice price, CancellationToken cancellationToken) =>
        dbContext.CatalogPrices.AddAsync(price, cancellationToken).AsTask();

    public Task AddHistoryAsync(CatalogPriceHistoryEntry entry, CancellationToken cancellationToken) =>
        dbContext.CatalogPriceHistory.AddAsync(entry, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
