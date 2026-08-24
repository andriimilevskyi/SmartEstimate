namespace SmartEstimate.Documents.EstimateDocuments;

/// <summary>
/// Technical company branding data used while rendering documents.
/// </summary>
public sealed record DocumentCompanyProfile(
    string Name,
    IReadOnlyCollection<string> Contacts,
    string? LogoPath);
