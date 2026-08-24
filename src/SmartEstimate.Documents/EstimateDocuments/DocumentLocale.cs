using System.Globalization;

namespace SmartEstimate.Documents.EstimateDocuments;

/// <summary>
/// Supported locales for system text in generated documents.
/// </summary>
public enum DocumentLocale
{
    Uk = 0,
    En = 1,
    De = 2
}

public static class DocumentLocales
{
    public const string DefaultCode = "uk";

    public static DocumentLocale Default => DocumentLocale.Uk;

    public static bool TryParse(string? value, out DocumentLocale locale)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            locale = Default;
            return true;
        }

        var normalized = value.Trim().ToLowerInvariant();
        locale = normalized switch
        {
            "uk" => DocumentLocale.Uk,
            "en" => DocumentLocale.En,
            "de" => DocumentLocale.De,
            _ => Default
        };

        return normalized is "uk" or "en" or "de";
    }

    public static string ToCode(this DocumentLocale locale) => locale switch
    {
        DocumentLocale.Uk => "uk",
        DocumentLocale.En => "en",
        DocumentLocale.De => "de",
        _ => DefaultCode
    };

    public static CultureInfo ToCulture(this DocumentLocale locale) => CultureInfo.GetCultureInfo(locale switch
    {
        DocumentLocale.Uk => "uk-UA",
        DocumentLocale.En => "en-US",
        DocumentLocale.De => "de-DE",
        _ => "uk-UA"
    });
}
