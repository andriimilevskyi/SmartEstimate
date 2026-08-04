namespace SmartEstimate.Application.Estimates.UpdateEstimateWorkItem;

/// <summary>
/// Updates the editable values of a work line in an estimate.
/// </summary>
public sealed record UpdateEstimateWorkItemCommand(
    Guid EstimateId,
    Guid ItemId,
    decimal Quantity,
    decimal UnitPrice,
    string? Notes);
