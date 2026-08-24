using System.Globalization;

namespace SmartEstimate.Documents.EstimateDocuments;

/// <summary>
/// Locale-specific system text used by estimate document renderers.
/// </summary>
public sealed record DocumentTexts(
    DocumentLocale Locale,
    CultureInfo Culture,
    string EstimateTitle,
    string EstimateNumber,
    string CreatedDate,
    string Customer,
    string Contacts,
    string Project,
    string ObjectType,
    string Address,
    string Area,
    string Currency,
    string Document,
    string ObjectDescription,
    string Notes,
    string ProposalIntroduction,
    string NoItems,
    string Works,
    string Materials,
    string RowNumber,
    string ItemName,
    string Unit,
    string Quantity,
    string UnitPrice,
    string Amount,
    string Comment,
    string WorkTotal,
    string MaterialTotal,
    string Total,
    string Totals,
    string GrandWorkTotal,
    string GrandMaterialTotal,
    string GrandTotal,
    string Signatures,
    string ContractorSignature,
    string CustomerSignature,
    string DateSignature,
    string Generated,
    string Page,
    IReadOnlyDictionary<EstimateDocumentTemplate, DocumentTemplateTexts> Templates,
    IReadOnlyDictionary<string, string> ObjectTypes)
{
    public DocumentTemplateTexts GetTemplate(EstimateDocumentTemplate template) =>
        Templates.TryGetValue(template, out var text)
            ? text
            : Templates[EstimateDocumentTemplate.FullEstimate];

    public string GetObjectTypeLabel(string objectType) =>
        ObjectTypes.TryGetValue(objectType, out var label) ? label : objectType;
}

public sealed record DocumentTemplateTexts(
    string Name,
    string Description,
    string TitlePrefix,
    string FileNameSuffix);
