using SmartEstimate.Application.Pricing.Abstractions;
using SmartEstimate.Domain.Pricing;

namespace SmartEstimate.Application.Pricing;

public sealed class PriceResolver(ICatalogPriceRepository prices) : IPriceResolver
{
    public async Task<ResolvedPriceResponse?> GetCurrentPriceAsync(
        PriceTarget target,
        string currency,
        string? regionCode,
        Guid? supplierId,
        string? supplierName,
        DateTimeOffset effectiveDate,
        CancellationToken cancellationToken)
    {
        var scope = new PriceScope(currency, regionCode, supplierId, supplierName);
        var candidates = await prices.GetByTargetIdsAsync(
            target.Type,
            [target.Id],
            scope.Currency,
            scope.SupplierName,
            scope.RegionCode,
            effectiveDate,
            cancellationToken);

        return candidates
            .Where(price => price.TargetId == target.Id && IsApplicable(price, scope, effectiveDate))
            .OrderByDescending(price => Score(price, scope))
            .ThenByDescending(price => price.EffectiveFrom)
            .ThenByDescending(price => price.CreatedAt)
            .Select(MapResolved)
            .FirstOrDefault();
    }

    private static bool IsApplicable(CatalogPrice price, PriceScope requested, DateTimeOffset effectiveDate)
    {
        if (!price.IsCurrentAt(effectiveDate) || price.Currency != requested.Currency)
        {
            return false;
        }

        var supplierMatches = requested.HasSupplier
            ? SupplierMatches(price, requested) || !price.Scope.HasSupplier
            : !price.Scope.HasSupplier;
        var regionMatches = requested.HasRegion
            ? string.Equals(price.RegionCode, requested.RegionCode, StringComparison.OrdinalIgnoreCase) || price.RegionCode is null
            : price.RegionCode is null;

        return supplierMatches && regionMatches;
    }

    private static int Score(CatalogPrice price, PriceScope requested)
    {
        var score = 0;
        if (requested.HasSupplier && SupplierMatches(price, requested))
        {
            score += 4;
        }

        if (requested.HasRegion && string.Equals(price.RegionCode, requested.RegionCode, StringComparison.OrdinalIgnoreCase))
        {
            score += 2;
        }

        return score;
    }

    private static bool SupplierMatches(CatalogPrice price, PriceScope requested)
    {
        if (requested.SupplierId is { } supplierId && price.SupplierId == supplierId)
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(requested.SupplierName)
            && string.Equals(price.SupplierName, requested.SupplierName, StringComparison.OrdinalIgnoreCase);
    }

    private static ResolvedPriceResponse MapResolved(CatalogPrice price) => new(
        price.Id,
        price.Amount,
        price.Currency,
        price.RegionCode,
        price.SupplierId,
        price.SupplierName,
        price.EffectiveFrom,
        price.SourceType);
}
