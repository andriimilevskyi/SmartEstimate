namespace SmartEstimate.Documents.EstimateDocuments;

/// <summary>
/// Describes a document template exposed by the API and frontend.
/// </summary>
public sealed record DocumentTemplateDefinition(
    EstimateDocumentTemplate Template,
    DocumentOutputFormat Format,
    string Code,
    string Name,
    string Description);
