namespace SmartEstimate.Api.Endpoints.Estimates;

/// <summary>
/// HTTP request for adding a construction work selected from the knowledge catalog.
/// </summary>
public sealed record AddEstimateWorkItemRequest(
    Guid ZoneId,
    string ConstructionWorkId,
    decimal Quantity,
    decimal UnitPrice,
    string? Notes);

/// <summary>
/// HTTP request for adding a material selected from the knowledge catalog.
/// </summary>
public sealed record AddEstimateMaterialItemRequest(
    Guid ZoneId,
    string MaterialId,
    decimal Quantity,
    decimal UnitPrice,
    string? Notes);

/// <summary>
/// HTTP request for changing values that are editable on an estimate line.
/// </summary>
public sealed record UpdateEstimateLineItemRequest(
    decimal Quantity,
    decimal UnitPrice,
    string? Notes);

/// <summary>
/// HTTP request for adding or renaming an estimate zone.
/// </summary>
public sealed record EstimateZoneRequest(string Name);

/// <summary>
/// HTTP request for replacing the estimate zone order.
/// </summary>
public sealed record ReorderEstimateZonesRequest(IReadOnlyCollection<Guid> ZoneIds);
