namespace SmartEstimate.Documents.EstimateDocuments;

/// <summary>
/// Provides localized system text for generated documents.
/// </summary>
public interface IDocumentTextProvider
{
    DocumentTexts GetTexts(DocumentLocale locale);
}
