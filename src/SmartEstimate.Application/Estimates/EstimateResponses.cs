namespace SmartEstimate.Application.Estimates;

/// <summary>
/// The compact representation used in the estimates collection.
/// </summary>
public sealed record EstimateSummaryResponse(
    Guid Id,
    string EstimateNumber,
    string Currency,
    string ObjectType,
    string? ObjectAddress,
    decimal? TotalArea,
    decimal TotalLabor,
    decimal TotalMaterials,
    decimal GrandTotal,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int Version);

/// <summary>
/// Full representation of an Estimate aggregate.
/// </summary>
public sealed record EstimateDetailsResponse(
    Guid Id,
    string EstimateNumber,
    string Currency,
    string ObjectType,
    string? ObjectAddress,
    decimal? TotalArea,
    string? Notes,
    decimal TotalLabor,
    decimal TotalMaterials,
    decimal GrandTotal,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int Version,
    IReadOnlyCollection<EstimateZoneResponse> Zones,
    IReadOnlyCollection<EstimateLineItemResponse> WorkItems,
    IReadOnlyCollection<EstimateLineItemResponse> MaterialItems);

/// <summary>
/// A user-editable section of an estimate with zone-level subtotals.
/// </summary>
public sealed record EstimateZoneResponse(
    Guid Id,
    string Name,
    int SortOrder,
    decimal TotalLabor,
    decimal TotalMaterials,
    decimal GrandTotal);

/// <summary>
/// A single work or material line rendered as part of an estimate response.
/// </summary>
public sealed record EstimateLineItemResponse(
    Guid Id,
    Guid ZoneId,
    string Name,
    decimal Quantity,
    string MeasurementUnit,
    decimal UnitPrice,
    decimal Total,
    string? Notes,
    string? KnowledgeItemId);

/// <summary>
/// Pagination metadata and estimate summaries.
/// </summary>
public sealed record PagedEstimatesResponse(
    IReadOnlyCollection<EstimateSummaryResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
