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

        configuration.NewConfig<EstimateWorkItem, EstimateLineItemResponse>()
            .Map(destination => destination.Quantity, source => source.Quantity.Value)
            .Map(destination => destination.MeasurementUnit, source => source.MeasurementUnit.Value)
            .Map(destination => destination.UnitPrice, source => source.UnitPrice.Amount)
            .Map(destination => destination.Total, source => source.Total.Amount)
            .Map(destination => destination.SourcePriceId, source => source.SourcePriceId)
            .Map(destination => destination.PriceCapturedAt, source => source.PriceCapturedAt)
            .Map(destination => destination.IsUnitPriceManuallyOverridden, source => source.IsUnitPriceManuallyOverridden)
            .Map(destination => destination.NameSource, source => source.NameSource.ToString());

        configuration.NewConfig<EstimateMaterialItem, EstimateLineItemResponse>()
            .Map(destination => destination.Quantity, source => source.Quantity.Value)
            .Map(destination => destination.MeasurementUnit, source => source.MeasurementUnit.Value)
            .Map(destination => destination.UnitPrice, source => source.UnitPrice.Amount)
            .Map(destination => destination.Total, source => source.Total.Amount)
            .Map(destination => destination.SourcePriceId, source => source.SourcePriceId)
            .Map(destination => destination.PriceCapturedAt, source => source.PriceCapturedAt)
            .Map(destination => destination.IsUnitPriceManuallyOverridden, source => source.IsUnitPriceManuallyOverridden)
            .Map(destination => destination.NameSource, source => source.NameSource.ToString());
    }
}
