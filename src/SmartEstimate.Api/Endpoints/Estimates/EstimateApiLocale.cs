using SmartEstimate.Domain.Estimates;
using SmartEstimate.Documents.EstimateDocuments;

namespace SmartEstimate.Api.Endpoints.Estimates;

internal static class EstimateApiLocale
{
    public static EstimateDisplayLocale Resolve(HttpContext httpContext)
    {
        var header = httpContext.Request.Headers.AcceptLanguage.FirstOrDefault();
        return Parse(header);
    }

    public static EstimateDisplayLocale FromDocumentLocale(DocumentLocale locale) => locale switch
    {
        DocumentLocale.En => EstimateDisplayLocale.En,
        DocumentLocale.De => EstimateDisplayLocale.De,
        _ => EstimateDisplayLocale.Uk
    };

    private static EstimateDisplayLocale Parse(string? value)
    {
        var language = value?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault()?
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault()?
            .Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault()?
            .ToLowerInvariant();

        return language switch
        {
            "en" => EstimateDisplayLocale.En,
            "de" => EstimateDisplayLocale.De,
            _ => EstimateDisplayLocale.Uk
        };
    }
}
