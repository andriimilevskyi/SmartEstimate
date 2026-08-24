using FluentValidation;
using SmartEstimate.Application.Knowledge.Abstractions;
using SmartEstimate.Application.Pricing.Abstractions;
using SmartEstimate.Domain.Knowledge;
using SmartEstimate.Domain.Pricing;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Application.Pricing;

public sealed class PricingManagementService(
    ICatalogPriceRepository prices,
    ICategoryRepository categories,
    IConstructionWorkRepository works,
    IMaterialRepository materials,
    IUnitRepository units,
    IValidator<PriceWriteRequest> validator)
{
    public async Task<Result<PagedPricingCatalogResponse>> GetCatalogAsync(PricingCatalogQuery query, CancellationToken cancellationToken)
    {
        var normalizedCurrency = string.IsNullOrWhiteSpace(query.Currency) ? null : new PriceScope(query.Currency, null, null, null).Currency;
        var knowledgeQuery = new KnowledgeListQuery(query.Page, query.PageSize, query.Search, "name", null, query.CategoryId, true);
        var categoryItems = await categories.ListAsync(new KnowledgeListQuery(1, 500, ActiveOnly: false), cancellationToken);
        var unitsItems = await units.ListAsync(new KnowledgeListQuery(1, 500, ActiveOnly: false), cancellationToken);
        var categoriesById = categoryItems.ToDictionary(category => category.Id);
        var unitsById = unitsItems.ToDictionary(unit => unit.Id);

        IReadOnlyCollection<PriceCatalogItemResponse> items;
        int count;

        if (query.TargetType == PriceTargetType.Material)
        {
            var records = await materials.ListAsync(knowledgeQuery, cancellationToken);
            count = await materials.CountAsync(knowledgeQuery, cancellationToken);
            items = await MapMaterialsAsync(records, categoriesById, unitsById, normalizedCurrency, query, cancellationToken);
        }
        else
        {
            var records = await works.ListAsync(knowledgeQuery, cancellationToken);
            count = await works.CountAsync(knowledgeQuery, cancellationToken);
            items = await MapWorksAsync(records, categoriesById, unitsById, normalizedCurrency, query, cancellationToken);
        }

        if (query.MissingOnly)
        {
            items = items.Where(item => item.CurrentPrice is null).ToArray();
            count = items.Count;
        }

        return Result<PagedPricingCatalogResponse>.Success(new(items, query.Page, query.PageSize, count));
    }

    public async Task<Result<PriceSummaryResponse>> CreatePriceAsync(PriceWriteRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PriceSummaryResponse>.Failure(PricingErrors.Validation(validation));
        }

        var targetResult = await EnsureTargetAsync(request.TargetType, request.TargetId, cancellationToken);
        if (targetResult is { IsSuccess: false })
        {
            return Result<PriceSummaryResponse>.Failure(targetResult.Error);
        }

        var now = DateTimeOffset.UtcNow;
        var target = new PriceTarget(request.TargetType, request.TargetId);
        var scope = new PriceScope(request.Currency, request.RegionCode, request.SupplierId, request.SupplierName);
        var openPrices = await prices.GetOpenPricesInScopeAsync(target, scope, request.EffectiveFrom, cancellationToken);
        foreach (var openPrice in openPrices)
        {
            openPrice.Close(request.EffectiveFrom, now);
            await prices.AddHistoryAsync(CatalogPriceHistoryEntry.Capture(Guid.NewGuid(), openPrice, PriceChangeType.Updated, now), cancellationToken);
        }

        var price = CatalogPrice.Create(Guid.NewGuid(), target, request.Amount, scope, request.EffectiveFrom, request.SourceType, request.Notes, now);
        await prices.AddAsync(price, cancellationToken);
        await prices.AddHistoryAsync(CatalogPriceHistoryEntry.Capture(Guid.NewGuid(), price, PriceChangeType.Created, now), cancellationToken);
        await prices.SaveChangesAsync(cancellationToken);

        return Result<PriceSummaryResponse>.Success(Map(price));
    }

    public async Task<Result<PriceSummaryResponse>> UpdatePriceAsync(Guid id, PriceWriteRequest request, CancellationToken cancellationToken)
    {
        var existing = await prices.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return Result<PriceSummaryResponse>.Failure(PricingErrors.PriceNotFound(id));
        }

        var now = DateTimeOffset.UtcNow;
        if (request.EffectiveFrom > existing.EffectiveFrom)
        {
            existing.Close(request.EffectiveFrom, now);
            await prices.AddHistoryAsync(CatalogPriceHistoryEntry.Capture(Guid.NewGuid(), existing, PriceChangeType.Updated, now), cancellationToken);
        }
        else
        {
            existing.Archive(now);
            await prices.AddHistoryAsync(CatalogPriceHistoryEntry.Capture(Guid.NewGuid(), existing, PriceChangeType.Archived, now), cancellationToken);
        }

        return await CreatePriceAsync(request with { TargetType = existing.TargetType, TargetId = existing.TargetId }, cancellationToken);
    }

    public async Task<Result> ArchivePriceAsync(Guid id, CancellationToken cancellationToken)
    {
        var price = await prices.GetByIdAsync(id, cancellationToken);
        if (price is null)
        {
            return Result.Failure(PricingErrors.PriceNotFound(id));
        }

        var now = DateTimeOffset.UtcNow;
        price.Archive(now);
        await prices.AddHistoryAsync(CatalogPriceHistoryEntry.Capture(Guid.NewGuid(), price, PriceChangeType.Archived, now), cancellationToken);
        await prices.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<PriceHistoryResponse>> GetHistoryAsync(PriceTargetType targetType, Guid targetId, CancellationToken cancellationToken)
    {
        var target = new PriceTarget(targetType, targetId);
        var priceItems = await prices.GetHistoryAsync(target, cancellationToken);
        var events = await prices.GetHistoryEventsAsync(target, cancellationToken);
        return Result<PriceHistoryResponse>.Success(new(priceItems.Select(Map).ToArray(), events.Select(MapEvent).ToArray()));
    }

    private async Task<IReadOnlyCollection<PriceCatalogItemResponse>> MapMaterialsAsync(
        IReadOnlyCollection<KnowledgeMaterial> records,
        Dictionary<Guid, KnowledgeCategory> categoriesById,
        Dictionary<Guid, MeasurementUnit> unitsById,
        string? currency,
        PricingCatalogQuery query,
        CancellationToken cancellationToken)
    {
        var currentPrices = await GetResolvedByTargetAsync(PriceTargetType.Material, records.Select(record => record.Id).ToArray(), currency, query, cancellationToken);
        return records
            .Select(material => new PriceCatalogItemResponse(
                material.Id,
                PriceTargetType.Material,
                Resolve(material.Name, query.Locale),
                material.CategoryId,
                material.CategoryId is { } categoryId && categoriesById.TryGetValue(categoryId, out var category)
                    ? Resolve(category.Name, query.Locale)
                    : null,
                material.UnitId,
                unitsById.TryGetValue(material.UnitId, out var unit) ? unit.Symbol : string.Empty,
                currentPrices.GetValueOrDefault(material.Id)))
            .ToArray();
    }

    private async Task<IReadOnlyCollection<PriceCatalogItemResponse>> MapWorksAsync(
        IReadOnlyCollection<ConstructionWork> records,
        Dictionary<Guid, KnowledgeCategory> categoriesById,
        Dictionary<Guid, MeasurementUnit> unitsById,
        string? currency,
        PricingCatalogQuery query,
        CancellationToken cancellationToken)
    {
        var currentPrices = await GetResolvedByTargetAsync(PriceTargetType.ConstructionWork, records.Select(record => record.Id).ToArray(), currency, query, cancellationToken);
        return records
            .Select(work => new PriceCatalogItemResponse(
                work.Id,
                PriceTargetType.ConstructionWork,
                Resolve(work.Name, query.Locale),
                work.CategoryId,
                categoriesById.TryGetValue(work.CategoryId, out var category) ? Resolve(category.Name, query.Locale) : null,
                work.UnitId,
                unitsById.TryGetValue(work.UnitId, out var unit) ? unit.Symbol : string.Empty,
                currentPrices.GetValueOrDefault(work.Id)))
            .ToArray();
    }

    private async Task<Dictionary<Guid, PriceSummaryResponse>> GetResolvedByTargetAsync(
        PriceTargetType targetType,
        Guid[] targetIds,
        string? currency,
        PricingCatalogQuery query,
        CancellationToken cancellationToken)
    {
        if (targetIds.Length == 0)
        {
            return [];
        }

        var priceItems = await prices.GetByTargetIdsAsync(
            targetType,
            targetIds,
            currency,
            query.Supplier,
            query.RegionCode,
            DateTimeOffset.UtcNow,
            cancellationToken);

        return priceItems
            .GroupBy(price => price.TargetId)
            .ToDictionary(
                group => group.Key,
                group => Map(group
                    .OrderByDescending(price => Score(price, query))
                    .ThenByDescending(price => price.EffectiveFrom)
                    .ThenByDescending(price => price.CreatedAt)
                    .First()));
    }

    private async Task<Result> EnsureTargetAsync(PriceTargetType targetType, Guid targetId, CancellationToken cancellationToken)
    {
        if (targetType == PriceTargetType.Material)
        {
            var material = await materials.GetByIdAsync(targetId, cancellationToken);
            return material is null
                ? Result.Failure(PricingErrors.TargetNotFound(targetId))
                : material.Status == KnowledgeStatus.Active
                    ? Result.Success()
                    : Result.Failure(PricingErrors.TargetInactive(targetId));
        }

        var work = await works.GetByIdAsync(targetId, cancellationToken);
        return work is null
            ? Result.Failure(PricingErrors.TargetNotFound(targetId))
            : work.Status == KnowledgeStatus.Active
                ? Result.Success()
                : Result.Failure(PricingErrors.TargetInactive(targetId));
    }

    private static string Resolve(LocalizedText text, PricingDisplayLocale locale) => locale switch
    {
        PricingDisplayLocale.En => text.En,
        PricingDisplayLocale.De => text.De,
        _ => text.Uk
    };

    private static PriceSummaryResponse Map(CatalogPrice price) => new(
        price.Id,
        price.Amount,
        price.Currency,
        price.RegionCode,
        price.SupplierId,
        price.SupplierName,
        price.EffectiveFrom,
        price.EffectiveUntil,
        price.SourceType,
        price.Status,
        price.Notes,
        price.CreatedAt,
        price.UpdatedAt,
        price.ArchivedAt,
        price.Version);

    private static int Score(CatalogPrice price, PricingCatalogQuery query)
    {
        var score = 0;
        if (!string.IsNullOrWhiteSpace(query.Supplier)
            && string.Equals(price.SupplierName, query.Supplier.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            score += 4;
        }

        if (!string.IsNullOrWhiteSpace(query.RegionCode)
            && string.Equals(price.RegionCode, query.RegionCode.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            score += 2;
        }

        return score;
    }

    private static PriceHistoryEntryResponse MapEvent(CatalogPriceHistoryEntry entry) => new(
        entry.Id,
        entry.CatalogPriceId,
        entry.Amount,
        entry.Currency,
        entry.RegionCode,
        entry.SupplierId,
        entry.SupplierName,
        entry.EffectiveFrom,
        entry.EffectiveUntil,
        entry.SourceType,
        entry.PriceStatus,
        entry.Notes,
        entry.ChangeType,
        entry.ChangedAt,
        entry.ChangedBy);
}
