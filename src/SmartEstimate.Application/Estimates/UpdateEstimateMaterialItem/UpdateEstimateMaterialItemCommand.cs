namespace SmartEstimate.Application.Estimates.UpdateEstimateMaterialItem;

/// <summary>
/// Updates the editable values of a material line in an estimate.
/// </summary>
public sealed record UpdateEstimateMaterialItemCommand(
    Guid EstimateId,
    Guid ItemId,
    decimal Quantity,
    decimal UnitPrice,
    string? Notes);
