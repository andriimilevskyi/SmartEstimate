namespace SmartEstimate.Application.Estimates.AddEstimateWorkItem;

/// <summary>
/// Adds a construction-work record from the knowledge catalog to an estimate.
/// </summary>
public sealed record AddEstimateWorkItemCommand(
    Guid EstimateId,
    Guid ZoneId,
    string ConstructionWorkId,
    decimal Quantity,
    decimal UnitPrice,
    string? Notes);
