using SmartEstimate.Application.Business.Abstractions;
using SmartEstimate.Domain.Customers;
using SmartEstimate.Domain.Estimates;
using SmartEstimate.Domain.Objects;

namespace SmartEstimate.Application.Estimates;

public sealed class EstimateResponseFactory(
    IEstimateObjectRepository objects,
    ICustomerRepository customers)
{
    public async Task<EstimateDetailsResponse> CreateDetailsAsync(
        Estimate estimate,
        EstimateDisplayLocale locale,
        CancellationToken cancellationToken)
    {
        var context = await CreateContextAsync(estimate.ObjectId, cancellationToken);
        return new EstimateDetailsResponse(
            estimate.Id,
            estimate.Number.Value,
            estimate.Currency,
            estimate.Status.ToString(),
            estimate.IsDeleted,
            estimate.DeletedAt,
            context,
            estimate.Notes,
            estimate.TotalLabor.Amount,
            estimate.TotalMaterials.Amount,
            estimate.GrandTotal.Amount,
            estimate.CreatedAt,
            estimate.UpdatedAt,
            estimate.Version,
            MapZones(estimate),
            estimate.WorkItems.Select(item => MapLineItem(item, locale)).ToArray(),
            estimate.MaterialItems.Select(item => MapLineItem(item, locale)).ToArray());
    }

    public Task<EstimateDetailsResponse> CreateDetailsAsync(Estimate estimate, CancellationToken cancellationToken) =>
        CreateDetailsAsync(estimate, EstimateDisplayLocale.Uk, cancellationToken);

    public async Task<IReadOnlyCollection<EstimateSummaryResponse>> CreateSummariesAsync(
        IReadOnlyCollection<Estimate> estimates,
        CancellationToken cancellationToken)
    {
        var contexts = await CreateContextsAsync(estimates.Select(estimate => estimate.ObjectId).Distinct().ToArray(), cancellationToken);
        var responses = new List<EstimateSummaryResponse>(estimates.Count);
        foreach (var estimate in estimates)
        {
            var context = contexts.TryGetValue(estimate.ObjectId, out var value)
                ? value
                : throw new InvalidOperationException($"Estimate object '{estimate.ObjectId}' was not found.");
            responses.Add(new EstimateSummaryResponse(
                estimate.Id,
                estimate.Number.Value,
                estimate.Currency,
                estimate.Status.ToString(),
                estimate.IsDeleted,
                estimate.DeletedAt,
                context,
                estimate.TotalLabor.Amount,
                estimate.TotalMaterials.Amount,
                estimate.GrandTotal.Amount,
                estimate.CreatedAt,
                estimate.UpdatedAt,
                estimate.Version));
        }

        return responses;
    }

    private async Task<IReadOnlyDictionary<Guid, EstimateBusinessContextResponse>> CreateContextsAsync(
        IReadOnlyCollection<Guid> objectIds,
        CancellationToken cancellationToken)
    {
        var estimateObjects = await objects.GetByIdsAsync(objectIds, cancellationToken);
        var customersById = (await customers.GetByIdsAsync(
                estimateObjects.Select(estimateObject => estimateObject.CustomerId).Distinct().ToArray(),
                cancellationToken))
            .ToDictionary(customer => customer.Id);

        return estimateObjects.ToDictionary(
            estimateObject => estimateObject.Id,
            estimateObject =>
            {
                var customer = customersById.TryGetValue(estimateObject.CustomerId, out var value)
                    ? value
                    : throw new InvalidOperationException($"Customer '{estimateObject.CustomerId}' was not found.");

                return MapContext(estimateObject, customer);
            });
    }

    private async Task<EstimateBusinessContextResponse> CreateContextAsync(Guid objectId, CancellationToken cancellationToken)
    {
        var estimateObject = await objects.GetByIdAsync(objectId, cancellationToken)
            ?? throw new InvalidOperationException($"Estimate object '{objectId}' was not found.");
        var customer = await customers.GetByIdAsync(estimateObject.CustomerId, cancellationToken)
            ?? throw new InvalidOperationException($"Customer '{estimateObject.CustomerId}' was not found.");

        return MapContext(estimateObject, customer);
    }

    private static EstimateBusinessContextResponse MapContext(EstimateObject estimateObject, Customer customer) => new(
        estimateObject.Id,
        estimateObject.Name,
        estimateObject.ObjectType.ToString(),
        estimateObject.Address,
        estimateObject.TotalArea,
        estimateObject.Description,
        customer.Id,
        customer.Name,
        customer.Phone,
        customer.Email,
        customer.Note);

    private static EstimateLineItemResponse MapLineItem(EstimateWorkItem item, EstimateDisplayLocale locale) => new(
        item.Id,
        item.ZoneId,
        EstimateDisplayNameResolver.Resolve(item.Name, item.NameSnapshot, locale),
        item.Quantity.Value,
        item.MeasurementUnit.Value,
        item.UnitPrice.Amount,
        item.Total.Amount,
        item.Notes,
        item.KnowledgeItemId,
        item.NameSource.ToString(),
        item.SourcePriceId,
        item.PriceCapturedAt,
        item.IsUnitPriceManuallyOverridden);

    private static EstimateLineItemResponse MapLineItem(EstimateMaterialItem item, EstimateDisplayLocale locale) => new(
        item.Id,
        item.ZoneId,
        EstimateDisplayNameResolver.Resolve(item.Name, item.NameSnapshot, locale),
        item.Quantity.Value,
        item.MeasurementUnit.Value,
        item.UnitPrice.Amount,
        item.Total.Amount,
        item.Notes,
        item.KnowledgeItemId,
        item.NameSource.ToString(),
        item.SourcePriceId,
        item.PriceCapturedAt,
        item.IsUnitPriceManuallyOverridden);

    private static EstimateZoneResponse[] MapZones(Estimate estimate) =>
        estimate.Zones
            .OrderBy(zone => zone.SortOrder)
            .Select(zone =>
            {
                var laborTotal = estimate.WorkItems
                    .Where(item => item.ZoneId == zone.Id)
                    .Sum(item => item.Total.Amount);
                var materialsTotal = estimate.MaterialItems
                    .Where(item => item.ZoneId == zone.Id)
                    .Sum(item => item.Total.Amount);

                return new EstimateZoneResponse(
                    zone.Id,
                    zone.Name,
                    zone.SortOrder,
                    laborTotal,
                    materialsTotal,
                    laborTotal + materialsTotal);
            })
            .ToArray();
}
