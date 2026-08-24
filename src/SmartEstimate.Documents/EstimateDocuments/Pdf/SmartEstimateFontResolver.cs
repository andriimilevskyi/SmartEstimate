using PdfSharp.Fonts;

namespace SmartEstimate.Documents.EstimateDocuments.Pdf;

/// <summary>
/// Resolves fonts for PDFsharp Core in Linux, macOS, and Docker environments.
/// </summary>
internal sealed class SmartEstimateFontResolver : IFontResolver
{
    private const string RegularFace = "SmartEstimateSans#Regular";
    private const string BoldFace = "SmartEstimateSans#Bold";
    private readonly Dictionary<string, string> fontFiles;

    public SmartEstimateFontResolver()
    {
        var regular = FindFirstExisting(
            "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
            "/usr/share/fonts/truetype/liberation2/LiberationSans-Regular.ttf",
            "/System/Library/Fonts/Supplemental/Arial.ttf",
            "/System/Library/Fonts/Supplemental/Arial Unicode.ttf",
            "/Library/Fonts/Arial.ttf");
        var bold = FindFirstExisting(
            "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf",
            "/usr/share/fonts/truetype/liberation2/LiberationSans-Bold.ttf",
            "/System/Library/Fonts/Supplemental/Arial Bold.ttf",
            "/Library/Fonts/Arial Bold.ttf");

        if (regular is null)
        {
            throw new InvalidOperationException(
                "No compatible PDF font was found. Install DejaVu Sans or configure a bundled font.");
        }

        fontFiles = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [RegularFace] = regular,
            [BoldFace] = bold ?? regular
        };
    }

    public byte[]? GetFont(string faceName) =>
        fontFiles.TryGetValue(faceName, out var path) ? File.ReadAllBytes(path) : null;

    public FontResolverInfo? ResolveTypeface(string familyName, bool bold, bool italic)
    {
        _ = familyName;
        return new FontResolverInfo(
            bold ? BoldFace : RegularFace,
            bold && fontFiles[BoldFace] == fontFiles[RegularFace],
            italic);
    }

    private static string? FindFirstExisting(params string[] candidates) =>
        candidates.FirstOrDefault(File.Exists);
}
