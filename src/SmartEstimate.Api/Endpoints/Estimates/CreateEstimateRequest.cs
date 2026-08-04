using SmartEstimate.Domain.Estimates;

namespace SmartEstimate.Api.Endpoints.Estimates;

/// <summary>
/// HTTP request for creating an estimate with optional initial work and material lines.
/// </summary>
public sealed record CreateEstimateRequest(
    string EstimateNumber,
    string Currency,
    EstimateObjectType ObjectType,
    string? ObjectAddress,
    decimal? TotalArea,
    string? Notes,
    IReadOnlyCollection<string> Zones,
    IReadOnlyCollection<CreateEstimateLineItemRequest>? WorkItems,
    IReadOnlyCollection<CreateEstimateLineItemRequest>? MaterialItems);

/// <summary>
/// HTTP request representation of an initial work or material line.
/// </summary>
public sealed record CreateEstimateLineItemRequest(
    string Name,
    decimal Quantity,
    string MeasurementUnit,
    decimal UnitPrice,
    string? Notes);
