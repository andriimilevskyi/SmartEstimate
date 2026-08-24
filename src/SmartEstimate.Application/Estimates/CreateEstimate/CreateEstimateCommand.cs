namespace SmartEstimate.Application.Estimates.CreateEstimate;

/// <summary>
/// Creates an Estimate aggregate and its initial line items.
/// </summary>
public sealed record CreateEstimateCommand(
    string EstimateNumber,
    Guid ObjectId,
    string Currency,
    string? Notes,
    IReadOnlyCollection<string> Zones,
    IReadOnlyCollection<CreateEstimateLineItemCommand>? WorkItems,
    IReadOnlyCollection<CreateEstimateLineItemCommand>? MaterialItems);

/// <summary>
/// Input for an initial estimate work or material line.
/// </summary>
public sealed record CreateEstimateLineItemCommand(
    string Name,
    decimal Quantity,
    string MeasurementUnit,
    decimal UnitPrice,
    string? Notes);
