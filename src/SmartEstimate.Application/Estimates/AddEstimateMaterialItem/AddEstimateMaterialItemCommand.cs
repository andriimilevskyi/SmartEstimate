namespace SmartEstimate.Application.Estimates.AddEstimateMaterialItem;

/// <summary>
/// Adds a material record from the knowledge catalog to an estimate.
/// </summary>
public sealed record AddEstimateMaterialItemCommand(
    Guid EstimateId,
    Guid ZoneId,
    string MaterialId,
    decimal Quantity,
    decimal UnitPrice,
    string? Notes);
