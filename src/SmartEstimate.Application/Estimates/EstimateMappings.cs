using Mapster;
using SmartEstimate.Domain.Estimates;

namespace SmartEstimate.Application.Estimates;

/// <summary>
/// Central Mapster mappings for Estimate read models.
/// </summary>
internal static class EstimateMappings
{
    public static void Register(TypeAdapterConfig configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        configuration.NewConfig<Estimate, EstimateSummaryResponse>()
            .Map(destination => destination.EstimateNumber, source => source.Number.Value)
            .Map(destination => destination.ObjectType, source => source.ObjectType.ToString())
            .Map(destination => destination.TotalLabor, source => source.TotalLabor.Amount)
            .Map(destination => destination.TotalMaterials, source => source.TotalMaterials.Amount)
            .Map(destination => destination.GrandTotal, source => source.GrandTotal.Amount);

        configuration.NewConfig<Estimate, EstimateDetailsResponse>()
            .Map(destination => destination.EstimateNumber, source => source.Number.Value)
            .Map(destination => destination.ObjectType, source => source.ObjectType.ToString())
            .Map(destination => destination.TotalLabor, source => source.TotalLabor.Amount)
            .Map(destination => destination.TotalMaterials, source => source.TotalMaterials.Amount)
            .Map(destination => destination.GrandTotal, source => source.GrandTotal.Amount)
            .Map(destination => destination.Zones, source => MapZones(source));

        configuration.NewConfig<EstimateWorkItem, EstimateLineItemResponse>()
            .Map(destination => destination.Quantity, source => source.Quantity.Value)
            .Map(destination => destination.MeasurementUnit, source => source.MeasurementUnit.Value)
            .Map(destination => destination.UnitPrice, source => source.UnitPrice.Amount)
            .Map(destination => destination.Total, source => source.Total.Amount);

        configuration.NewConfig<EstimateMaterialItem, EstimateLineItemResponse>()
            .Map(destination => destination.Quantity, source => source.Quantity.Value)
            .Map(destination => destination.MeasurementUnit, source => source.MeasurementUnit.Value)
            .Map(destination => destination.UnitPrice, source => source.UnitPrice.Amount)
            .Map(destination => destination.Total, source => source.Total.Amount);
    }

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
