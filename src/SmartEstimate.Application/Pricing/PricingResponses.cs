using SmartEstimate.Domain.Pricing;

namespace SmartEstimate.Application.Pricing;

public sealed record PriceWriteRequest(
    PriceTargetType TargetType,
    Guid TargetId,
    decimal Amount,
    string Currency,
    DateTimeOffset EffectiveFrom,
    PriceSourceType SourceType,
    string? RegionCode,
    Guid? SupplierId,
    string? SupplierName,
    string? Notes);

public sealed record PriceCatalogItemResponse(
    Guid TargetId,
    PriceTargetType TargetType,
    string Name,
    Guid? CategoryId,
    string? CategoryName,
    Guid UnitId,
    string UnitSymbol,
    PriceSummaryResponse? CurrentPrice);

public sealed record PriceSummaryResponse(
    Guid Id,
    decimal Amount,
    string Currency,
    string? RegionCode,
    Guid? SupplierId,
    string? SupplierName,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveUntil,
    PriceSourceType SourceType,
    PriceStatus Status,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ArchivedAt,
    int Version);

public sealed record ResolvedPriceResponse(
    Guid PriceId,
    decimal Amount,
    string Currency,
    string? RegionCode,
    Guid? SupplierId,
    string? SupplierName,
    DateTimeOffset EffectiveFrom,
    PriceSourceType SourceType);

public sealed record PriceHistoryResponse(
    IReadOnlyCollection<PriceSummaryResponse> Prices,
    IReadOnlyCollection<PriceHistoryEntryResponse> Events);

public sealed record PriceHistoryEntryResponse(
    Guid Id,
    Guid CatalogPriceId,
    decimal Amount,
    string Currency,
    string? RegionCode,
    Guid? SupplierId,
    string? SupplierName,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveUntil,
    PriceSourceType SourceType,
    PriceStatus PriceStatus,
    string? Notes,
    PriceChangeType ChangeType,
    DateTimeOffset ChangedAt,
    Guid? ChangedBy);

public sealed record PagedPricingCatalogResponse(
    IReadOnlyCollection<PriceCatalogItemResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
