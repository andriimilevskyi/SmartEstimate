namespace SmartEstimate.Domain.Knowledge;

/// <summary>Localized human-readable text with the approved Ukrainian fallback chain.</summary>
public sealed class LocalizedText
{
    private LocalizedText()
    {
    }

    public LocalizedText(string uk, string? en, string? de)
    {
        Uk = Normalize(uk, nameof(uk));
        En = NormalizeOptional(en) ?? Uk;
        De = NormalizeOptional(de) ?? En;
    }

    public string Uk { get; private set; } = string.Empty;

    public string En { get; private set; } = string.Empty;

    public string De { get; private set; } = string.Empty;

    private static string Normalize(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Ukrainian localization is required.", parameterName);
        }

        var normalized = value.Trim();
        if (normalized.Length > 256)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Localized text cannot exceed 256 characters.");
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : Normalize(value, nameof(value));
}
