namespace SmartEstimate.Documents.EstimateDocuments;

/// <summary>
/// Input required to render an estimate document.
/// </summary>
public sealed record EstimateDocumentRenderRequest(
    EstimateDocumentTemplate Template,
    EstimateDocumentModel Estimate,
    DocumentCompanyProfile Company,
    DateTimeOffset GeneratedAt,
    DocumentLocale Locale = DocumentLocale.Uk);
