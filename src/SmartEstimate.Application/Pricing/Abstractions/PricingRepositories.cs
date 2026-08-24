using SmartEstimate.Domain.Pricing;

namespace SmartEstimate.Application.Pricing.Abstractions;

public sealed record PricingCatalogQuery(
    PriceTargetType TargetType,
    int Page,
    int PageSize,
    string? Search = null,
    Guid? CategoryId = null,
    string? Currency = null,
    string? Supplier = null,
    string? RegionCode = null,
    bool MissingOnly = false,
    PricingDisplayLocale Locale = PricingDisplayLocale.Uk);

public sealed record PriceResolutionQuery(
    PriceTarget Target,
    string Currency,
    string? RegionCode,
    Guid? SupplierId,
    string? SupplierName,
    DateTimeOffset EffectiveDate);

public interface ICatalogPriceRepository
{
    Task<CatalogPrice?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CatalogPrice>> GetByTargetIdsAsync(
        PriceTargetType targetType,
        Guid[] targetIds,
        string? currency,
        string? supplier,
        string? regionCode,
        DateTimeOffset effectiveDate,
        CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CatalogPrice>> GetHistoryAsync(PriceTarget target, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CatalogPrice>> GetOpenPricesInScopeAsync(
        PriceTarget target,
        PriceScope scope,
        DateTimeOffset effectiveFrom,
        CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CatalogPriceHistoryEntry>> GetHistoryEventsAsync(PriceTarget target, CancellationToken cancellationToken);
    Task AddAsync(CatalogPrice price, CancellationToken cancellationToken);
    Task AddHistoryAsync(CatalogPriceHistoryEntry entry, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IPriceResolver
{
    Task<ResolvedPriceResponse?> GetCurrentPriceAsync(
        PriceTarget target,
        string currency,
        string? regionCode,
        Guid? supplierId,
        string? supplierName,
        DateTimeOffset effectiveDate,
        CancellationToken cancellationToken);
}
