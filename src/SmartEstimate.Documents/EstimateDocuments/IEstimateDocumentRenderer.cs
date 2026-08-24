namespace SmartEstimate.Documents.EstimateDocuments;

/// <summary>
/// Renders estimate documents for a specific output format.
/// </summary>
public interface IEstimateDocumentRenderer
{
    DocumentOutputFormat Format { get; }

    IReadOnlyCollection<DocumentTemplateDefinition> GetTemplates(DocumentLocale locale = DocumentLocale.Uk);

    GeneratedDocument Render(EstimateDocumentRenderRequest request);
}
