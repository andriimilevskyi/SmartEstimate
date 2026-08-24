namespace SmartEstimate.Documents.EstimateDocuments;

/// <summary>
/// Read-only estimate data prepared for document rendering.
/// </summary>
public sealed record EstimateDocumentModel(
    Guid Id,
    string EstimateNumber,
    string Currency,
    string CustomerName,
    string? CustomerPhone,
    string? CustomerEmail,
    string ObjectName,
    string ObjectType,
    string? ObjectAddress,
    decimal? TotalArea,
    string? ObjectDescription,
    string? Notes,
    decimal TotalLabor,
    decimal TotalMaterials,
    decimal GrandTotal,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<EstimateDocumentZone> Zones);

/// <summary>
/// Zone-level estimate section used by document renderers.
/// </summary>
public sealed record EstimateDocumentZone(
    Guid Id,
    string Name,
    decimal TotalLabor,
    decimal TotalMaterials,
    decimal GrandTotal,
    IReadOnlyCollection<EstimateDocumentLineItem> WorkItems,
    IReadOnlyCollection<EstimateDocumentLineItem> MaterialItems);

/// <summary>
/// A single document line item.
/// </summary>
public sealed record EstimateDocumentLineItem(
    string Name,
    decimal Quantity,
    string MeasurementUnit,
    decimal UnitPrice,
    decimal Total,
    string? Notes);
