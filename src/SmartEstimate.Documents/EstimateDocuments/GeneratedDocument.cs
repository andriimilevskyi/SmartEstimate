namespace SmartEstimate.Documents.EstimateDocuments;

/// <summary>
/// Rendered document payload returned to the API layer.
/// </summary>
public sealed record GeneratedDocument(
    byte[] Content,
    string ContentType,
    string FileName);
