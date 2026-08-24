using System.Globalization;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Fonts;

namespace SmartEstimate.Documents.EstimateDocuments.Pdf;

/// <summary>
/// Renders professional estimate PDFs with MigraDoc.
/// </summary>
public sealed class PdfEstimateDocumentRenderer : IEstimateDocumentRenderer
{
    private const string ContentType = "application/pdf";
    private const string FontName = "SmartEstimate Sans";
    private const string AccentColorHex = "0F766E";
    private const string DarkColorHex = "111827";
    private const string MutedColorHex = "6B7280";
    private const string BorderColorHex = "D1D5DB";
    private readonly IDocumentTextProvider textProvider;

    public PdfEstimateDocumentRenderer()
        : this(new DocumentTextProvider())
    {
    }

    public PdfEstimateDocumentRenderer(IDocumentTextProvider textProvider)
    {
        this.textProvider = textProvider;
    }

    public DocumentOutputFormat Format => DocumentOutputFormat.Pdf;

    public IReadOnlyCollection<DocumentTemplateDefinition> GetTemplates(DocumentLocale locale = DocumentLocale.Uk)
    {
        var texts = textProvider.GetTexts(locale);

        return
        [
            CreateTemplateDefinition(EstimateDocumentTemplate.FullEstimate, "full-estimate", texts),
            CreateTemplateDefinition(EstimateDocumentTemplate.ShortEstimate, "short-estimate", texts),
            CreateTemplateDefinition(EstimateDocumentTemplate.CommercialProposal, "commercial-proposal", texts)
        ];
    }

    internal static Document CreateDocumentForTesting(EstimateDocumentRenderRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureFontResolver();

        return new PdfEstimateDocumentRenderer().CreateDocument(request);
    }

    public GeneratedDocument Render(EstimateDocumentRenderRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureFontResolver();

        var texts = textProvider.GetTexts(request.Locale);
        var document = CreateDocument(request, texts);
        var renderer = new PdfDocumentRenderer
        {
            Document = document
        };

        renderer.RenderDocument();
        renderer.PdfDocument.Info.Title = GetTitle(request.Template, request.Estimate, texts);
        renderer.PdfDocument.Info.Author = request.Company.Name;
        renderer.PdfDocument.Info.Subject = request.Estimate.EstimateNumber;

        using var stream = new MemoryStream();
        renderer.PdfDocument.Save(stream, false);

        return new GeneratedDocument(
            stream.ToArray(),
            ContentType,
            CreateFileName(request.Template, request.Estimate, texts));
    }

    private Document CreateDocument(EstimateDocumentRenderRequest request)
    {
        var texts = textProvider.GetTexts(request.Locale);
        return CreateDocument(request, texts);
    }

    private static Document CreateDocument(EstimateDocumentRenderRequest request, DocumentTexts texts)
    {
        var document = new Document
        {
            Info =
            {
                Title = GetTitle(request.Template, request.Estimate, texts),
                Author = request.Company.Name,
                Subject = request.Estimate.EstimateNumber
            }
        };

        DefineStyles(document);
        var section = document.AddSection();
        section.PageSetup.PageFormat = PageFormat.A4;
        section.PageSetup.TopMargin = Unit.FromCentimeter(1.3);
        section.PageSetup.BottomMargin = Unit.FromCentimeter(1.4);
        section.PageSetup.LeftMargin = Unit.FromCentimeter(1.4);
        section.PageSetup.RightMargin = Unit.FromCentimeter(1.4);

        AddFooter(section, request.GeneratedAt, texts);
        AddHeader(section, request, texts);
        AddEstimateMeta(section, request, texts);
        AddTemplateIntro(section, request, texts);
        AddZones(section, request, texts);
        AddTotals(section, request.Estimate, texts);
        AddSignatures(section, texts);

        return document;
    }

    private static void DefineStyles(Document document)
    {
        var normal = document.Styles[StyleNames.Normal]!;
        normal.Font.Name = FontName;
        normal.Font.Size = Unit.FromPoint(9);
        normal.Font.Color = HexColor(DarkColorHex);

        var heading1 = document.Styles[StyleNames.Heading1]!;
        heading1.Font.Name = FontName;
        heading1.Font.Size = Unit.FromPoint(18);
        heading1.Font.Bold = true;
        heading1.Font.Color = HexColor(DarkColorHex);
        heading1.ParagraphFormat.SpaceAfter = Unit.FromPoint(8);

        var heading2 = document.Styles[StyleNames.Heading2]!;
        heading2.Font.Name = FontName;
        heading2.Font.Size = Unit.FromPoint(12);
        heading2.Font.Bold = true;
        heading2.Font.Color = HexColor(DarkColorHex);
        heading2.ParagraphFormat.SpaceBefore = Unit.FromPoint(14);
        heading2.ParagraphFormat.SpaceAfter = Unit.FromPoint(6);
    }

    private static void AddFooter(Section section, DateTimeOffset generatedAt, DocumentTexts texts)
    {
        var paragraph = section.Footers.Primary.AddParagraph();
        paragraph.Format.Alignment = ParagraphAlignment.Center;
        paragraph.Format.Font.Size = Unit.FromPoint(7);
        paragraph.Format.Font.Color = HexColor(MutedColorHex);
        paragraph.AddText($"SmartEstimate · {texts.Generated} {FormatDate(generatedAt, texts)} · {texts.Page} ");
        paragraph.AddPageField();
        paragraph.AddText(" / ");
        paragraph.AddNumPagesField();
    }

    private static void AddHeader(Section section, EstimateDocumentRenderRequest request, DocumentTexts texts)
    {
        var table = section.AddTable();
        table.Borders.Visible = false;
        table.AddColumn(Unit.FromCentimeter(3.2));
        table.AddColumn(Unit.FromCentimeter(9.2));
        table.AddColumn(Unit.FromCentimeter(4.8));

        var row = table.AddRow();
        row.VerticalAlignment = VerticalAlignment.Center;

        AddLogoOrInitials(row.Cells[0], request.Company);

        var company = row.Cells[1].AddParagraph();
        company.Format.Font.Size = Unit.FromPoint(13);
        company.Format.Font.Bold = true;
        company.AddText(request.Company.Name);

        foreach (var contact in request.Company.Contacts.Where(contact => !string.IsNullOrWhiteSpace(contact)))
        {
            var contactParagraph = row.Cells[1].AddParagraph(contact);
            contactParagraph.Format.Font.Size = Unit.FromPoint(8);
            contactParagraph.Format.Font.Color = HexColor(MutedColorHex);
        }

        var label = row.Cells[2].AddParagraph(texts.GetTemplate(request.Template).Name);
        label.Format.Alignment = ParagraphAlignment.Right;
        label.Format.Font.Size = Unit.FromPoint(10);
        label.Format.Font.Bold = true;
        label.Format.Font.Color = HexColor(AccentColorHex);

        var number = row.Cells[2].AddParagraph(request.Estimate.EstimateNumber);
        number.Format.Alignment = ParagraphAlignment.Right;
        number.Format.Font.Size = Unit.FromPoint(15);
        number.Format.Font.Bold = true;
        number.Format.SpaceBefore = Unit.FromPoint(3);

        AddDivider(section);
    }

    private static void AddLogoOrInitials(Cell cell, DocumentCompanyProfile company)
    {
        if (!string.IsNullOrWhiteSpace(company.LogoPath) && File.Exists(company.LogoPath))
        {
            var image = cell.AddImage(company.LogoPath);
            image.LockAspectRatio = true;
            image.Width = Unit.FromCentimeter(2.8);
            return;
        }

        var initials = cell.AddParagraph(GetInitials(company.Name));
        initials.Format.Alignment = ParagraphAlignment.Center;
        initials.Format.Font.Bold = true;
        initials.Format.Font.Size = Unit.FromPoint(18);
        initials.Format.Font.Color = Colors.White;
        cell.Shading.Color = HexColor(AccentColorHex);
        cell.VerticalAlignment = VerticalAlignment.Center;
    }

    private static void AddDivider(Section section)
    {
        var divider = section.AddTable();
        divider.Borders.Visible = false;
        divider.AddColumn(Unit.FromCentimeter(17.2));
        var row = divider.AddRow();
        row.Height = Unit.FromPoint(9);
        row.Cells[0].Borders.Bottom.Width = Unit.FromPoint(1.2);
        row.Cells[0].Borders.Bottom.Color = HexColor(AccentColorHex);
    }

    private static void AddEstimateMeta(Section section, EstimateDocumentRenderRequest request, DocumentTexts texts)
    {
        var estimate = request.Estimate;
        var title = section.AddParagraph(GetTitle(request.Template, estimate, texts), StyleNames.Heading1);
        title.Format.SpaceBefore = Unit.FromPoint(12);

        var table = section.AddTable();
        table.Borders.Width = Unit.FromPoint(0.4);
        table.Borders.Color = HexColor(BorderColorHex);
        table.Rows.LeftIndent = Unit.Zero;
        table.AddColumn(Unit.FromCentimeter(4.1));
        table.AddColumn(Unit.FromCentimeter(4.5));
        table.AddColumn(Unit.FromCentimeter(4.1));
        table.AddColumn(Unit.FromCentimeter(4.5));

        AddMetaRows(
            table,
            [
                new(texts.EstimateNumber, estimate.EstimateNumber),
                new(texts.CreatedDate, FormatDate(estimate.CreatedAt, texts)),
                new(texts.Customer, estimate.CustomerName),
                new(texts.Contacts, FormatCustomerContacts(estimate)),
                new(texts.Project, estimate.ObjectName),
                new(texts.ObjectType, texts.GetObjectTypeLabel(estimate.ObjectType)),
                new(texts.Address, estimate.ObjectAddress),
                new(texts.Area, estimate.TotalArea is null ? null : $"{estimate.TotalArea.Value.ToString("N2", texts.Culture)} m²"),
                new(texts.Currency, estimate.Currency),
                new(texts.Document, texts.GetTemplate(request.Template).Name)
            ]);

        if (!string.IsNullOrWhiteSpace(estimate.ObjectDescription))
        {
            var row = table.AddRow();
            row.Cells[0].AddParagraph(texts.ObjectDescription);
            row.Cells[0].Format.Font.Bold = true;
            row.Cells[1].MergeRight = 2;
            row.Cells[1].AddParagraph(estimate.ObjectDescription);
        }

        if (!string.IsNullOrWhiteSpace(estimate.Notes))
        {
            var row = table.AddRow();
            row.Cells[0].AddParagraph(texts.Notes);
            row.Cells[0].Format.Font.Bold = true;
            row.Cells[1].MergeRight = 2;
            row.Cells[1].AddParagraph(estimate.Notes);
        }
    }

    private static void AddTemplateIntro(Section section, EstimateDocumentRenderRequest request, DocumentTexts texts)
    {
        if (request.Template != EstimateDocumentTemplate.CommercialProposal)
        {
            return;
        }

        var paragraph = section.AddParagraph();
        paragraph.Format.SpaceBefore = Unit.FromPoint(12);
        paragraph.Format.SpaceAfter = Unit.FromPoint(6);
        paragraph.Format.Font.Color = HexColor(DarkColorHex);
        paragraph.AddText(texts.ProposalIntroduction);
    }

    private static void AddZones(Section section, EstimateDocumentRenderRequest request, DocumentTexts texts)
    {
        var zones = request.Estimate.Zones
            .Where(HasAnyItems)
            .ToArray();
        if (zones.Length == 0)
        {
            section.AddParagraph(texts.NoItems, StyleNames.Heading2);
            return;
        }

        foreach (var zone in zones)
        {
            var heading = section.AddParagraph(zone.Name, StyleNames.Heading2);
            heading.Format.KeepWithNext = true;

            AddItems(section, texts.Works, zone.WorkItems, request.Template, request.Estimate.Currency, texts);
            AddItems(section, texts.Materials, zone.MaterialItems, request.Template, request.Estimate.Currency, texts);
            AddZoneTotals(section, zone, request.Estimate.Currency, texts);
        }
    }

    private static void AddItems(
        Section section,
        string title,
        IReadOnlyCollection<EstimateDocumentLineItem> items,
        EstimateDocumentTemplate template,
        string currency,
        DocumentTexts texts)
    {
        if (items.Count == 0)
        {
            return;
        }

        var paragraph = section.AddParagraph(title);
        paragraph.Format.Font.Bold = true;
        paragraph.Format.Font.Color = HexColor(AccentColorHex);
        paragraph.Format.SpaceBefore = Unit.FromPoint(5);
        paragraph.Format.SpaceAfter = Unit.FromPoint(3);

        if (template == EstimateDocumentTemplate.FullEstimate)
        {
            AddDetailedItemsTable(section, items, currency, texts);
        }
        else
        {
            AddCompactItemsTable(section, items, currency, texts);
        }
    }

    private static void AddDetailedItemsTable(
        Section section,
        IReadOnlyCollection<EstimateDocumentLineItem> items,
        string currency,
        DocumentTexts texts)
    {
        var table = CreateBaseTable(section);
        table.AddColumn(Unit.FromCentimeter(0.8));
        table.AddColumn(Unit.FromCentimeter(5.4));
        table.AddColumn(Unit.FromCentimeter(1.4));
        table.AddColumn(Unit.FromCentimeter(1.8));
        table.AddColumn(Unit.FromCentimeter(2.5));
        table.AddColumn(Unit.FromCentimeter(2.5));
        table.AddColumn(Unit.FromCentimeter(2.8));

        AddHeaderRow(table, [texts.RowNumber, texts.ItemName, texts.Unit, texts.Quantity, texts.UnitPrice, texts.Amount, texts.Comment]);

        var index = 1;
        foreach (var item in items)
        {
            var row = table.AddRow();
            row.TopPadding = Unit.FromPoint(3);
            row.BottomPadding = Unit.FromPoint(3);
            row.Cells[0].AddParagraph(index.ToString(texts.Culture));
            row.Cells[1].AddParagraph(item.Name);
            row.Cells[2].AddParagraph(item.MeasurementUnit);
            row.Cells[3].AddParagraph(FormatQuantity(item.Quantity, texts));
            row.Cells[4].AddParagraph(FormatMoney(item.UnitPrice, currency, texts));
            row.Cells[5].AddParagraph(FormatMoney(item.Total, currency, texts));
            row.Cells[6].AddParagraph(item.Notes ?? string.Empty);
            AlignNumericCells(row, [3, 4, 5]);
            index++;
        }
    }

    private static void AddCompactItemsTable(
        Section section,
        IReadOnlyCollection<EstimateDocumentLineItem> items,
        string currency,
        DocumentTexts texts)
    {
        var table = CreateBaseTable(section);
        table.AddColumn(Unit.FromCentimeter(0.9));
        table.AddColumn(Unit.FromCentimeter(9.3));
        table.AddColumn(Unit.FromCentimeter(2.2));
        table.AddColumn(Unit.FromCentimeter(2.3));
        table.AddColumn(Unit.FromCentimeter(2.5));

        AddHeaderRow(table, [texts.RowNumber, texts.ItemName, texts.Quantity, texts.Unit, texts.Amount]);

        var index = 1;
        foreach (var item in items)
        {
            var row = table.AddRow();
            row.TopPadding = Unit.FromPoint(3);
            row.BottomPadding = Unit.FromPoint(3);
            row.Cells[0].AddParagraph(index.ToString(texts.Culture));
            row.Cells[1].AddParagraph(item.Name);
            row.Cells[2].AddParagraph(FormatQuantity(item.Quantity, texts));
            row.Cells[3].AddParagraph(item.MeasurementUnit);
            row.Cells[4].AddParagraph(FormatMoney(item.Total, currency, texts));
            AlignNumericCells(row, [2, 4]);
            index++;
        }
    }

    private static void AddZoneTotals(Section section, EstimateDocumentZone zone, string currency, DocumentTexts texts)
    {
        var table = section.AddTable();
        table.Borders.Visible = false;
        table.Rows.LeftIndent = Unit.Zero;
        table.AddColumn(Unit.FromCentimeter(10.3));
        table.AddColumn(Unit.FromCentimeter(3.4));
        table.AddColumn(Unit.FromCentimeter(3.5));

        if (zone.WorkItems.Count > 0)
        {
            AddTotalRow(table, texts.WorkTotal, FormatMoney(zone.TotalLabor, currency, texts), false);
        }

        if (zone.MaterialItems.Count > 0)
        {
            AddTotalRow(table, texts.MaterialTotal, FormatMoney(zone.TotalMaterials, currency, texts), false);
        }

        AddTotalRow(table, texts.Total, FormatMoney(zone.GrandTotal, currency, texts), true);
    }

    private static void AddTotals(Section section, EstimateDocumentModel estimate, DocumentTexts texts)
    {
        var heading = section.AddParagraph(texts.Totals, StyleNames.Heading2);
        heading.Format.SpaceBefore = Unit.FromPoint(16);

        var table = section.AddTable();
        table.Borders.Visible = false;
        table.Rows.LeftIndent = Unit.FromCentimeter(8.5);
        table.AddColumn(Unit.FromCentimeter(4.6));
        table.AddColumn(Unit.FromCentimeter(4.1));

        AddGrandTotalRow(table, texts.GrandWorkTotal, FormatMoney(estimate.TotalLabor, estimate.Currency, texts), false);
        AddGrandTotalRow(table, texts.GrandMaterialTotal, FormatMoney(estimate.TotalMaterials, estimate.Currency, texts), false);
        AddGrandTotalRow(table, texts.GrandTotal, FormatMoney(estimate.GrandTotal, estimate.Currency, texts), true);
    }

    private static void AddSignatures(Section section, DocumentTexts texts)
    {
        var paragraph = section.AddParagraph(texts.Signatures);
        paragraph.Format.Font.Bold = true;
        paragraph.Format.SpaceBefore = Unit.FromPoint(22);
        paragraph.Format.SpaceAfter = Unit.FromPoint(8);

        var table = section.AddTable();
        table.Borders.Visible = false;
        table.AddColumn(Unit.FromCentimeter(8.2));
        table.AddColumn(Unit.FromCentimeter(0.8));
        table.AddColumn(Unit.FromCentimeter(8.2));

        var row = table.AddRow();
        row.Cells[0].AddParagraph(texts.ContractorSignature);
        row.Cells[2].AddParagraph(texts.CustomerSignature);

        var dateRow = table.AddRow();
        dateRow.TopPadding = Unit.FromPoint(12);
        dateRow.Cells[0].AddParagraph(texts.DateSignature);
        dateRow.Cells[2].AddParagraph(texts.DateSignature);
    }

    private static Table CreateBaseTable(Section section)
    {
        var table = section.AddTable();
        table.Borders.Width = Unit.FromPoint(0.35);
        table.Borders.Color = HexColor(BorderColorHex);
        table.Rows.LeftIndent = Unit.Zero;
        return table;
    }

    private static void AddMetaRows(Table table, IReadOnlyCollection<DocumentMetaItem> items)
    {
        var visibleItems = items
            .Where(item => !string.IsNullOrWhiteSpace(item.Value))
            .ToArray();

        for (var index = 0; index < visibleItems.Length; index += 2)
        {
            var first = visibleItems[index];
            var second = index + 1 < visibleItems.Length ? visibleItems[index + 1] : null;
            AddMetaRow(table, first.Label, first.Value!, second?.Label, second?.Value);
        }
    }

    private static void AddMetaRow(Table table, string label1, string value1, string? label2, string? value2)
    {
        var row = table.AddRow();
        row.TopPadding = Unit.FromPoint(3);
        row.BottomPadding = Unit.FromPoint(3);
        row.Cells[0].AddParagraph(label1);
        row.Cells[0].Format.Font.Bold = true;
        row.Cells[1].AddParagraph(value1);

        if (!string.IsNullOrWhiteSpace(label2) && !string.IsNullOrWhiteSpace(value2))
        {
            row.Cells[2].AddParagraph(label2);
            row.Cells[2].Format.Font.Bold = true;
            row.Cells[3].AddParagraph(value2);
        }
    }

    private static void AddHeaderRow(Table table, IReadOnlyList<string> labels)
    {
        var row = table.AddRow();
        row.HeadingFormat = true;
        row.Shading.Color = HexColor(DarkColorHex);
        row.Format.Font.Color = Colors.White;
        row.Format.Font.Bold = true;
        row.TopPadding = Unit.FromPoint(4);
        row.BottomPadding = Unit.FromPoint(4);

        for (var index = 0; index < labels.Count; index++)
        {
            row.Cells[index].AddParagraph(labels[index]);
        }
    }

    private static void AddTotalRow(Table table, string label, string amount, bool strong)
    {
        var row = table.AddRow();
        row.TopPadding = Unit.FromPoint(3);
        row.Cells[1].AddParagraph(label);
        row.Cells[2].AddParagraph(amount);
        row.Cells[1].Format.Alignment = ParagraphAlignment.Right;
        row.Cells[2].Format.Alignment = ParagraphAlignment.Right;

        if (strong)
        {
            row.Cells[1].Format.Font.Bold = true;
            row.Cells[2].Format.Font.Bold = true;
            row.Cells[2].Format.Font.Color = HexColor(AccentColorHex);
        }
    }

    private static void AddGrandTotalRow(Table table, string label, string amount, bool strong)
    {
        var row = table.AddRow();
        row.TopPadding = Unit.FromPoint(4);
        row.BottomPadding = Unit.FromPoint(4);
        row.Cells[0].AddParagraph(label);
        row.Cells[1].AddParagraph(amount);
        row.Cells[0].Format.Alignment = ParagraphAlignment.Right;
        row.Cells[1].Format.Alignment = ParagraphAlignment.Right;
        row.Cells[0].Borders.Bottom.Color = HexColor(BorderColorHex);
        row.Cells[1].Borders.Bottom.Color = HexColor(BorderColorHex);
        row.Cells[0].Borders.Bottom.Width = Unit.FromPoint(0.4);
        row.Cells[1].Borders.Bottom.Width = Unit.FromPoint(0.4);

        if (strong)
        {
            row.Shading.Color = HexColor("ECFDF5");
            row.Cells[0].Format.Font.Bold = true;
            row.Cells[1].Format.Font.Bold = true;
            row.Cells[1].Format.Font.Color = HexColor(AccentColorHex);
        }
    }

    private static void AlignNumericCells(Row row, IReadOnlyCollection<int> cellIndexes)
    {
        foreach (var index in cellIndexes)
        {
            row.Cells[index].Format.Alignment = ParagraphAlignment.Right;
        }
    }

    private static DocumentTemplateDefinition CreateTemplateDefinition(
        EstimateDocumentTemplate template,
        string code,
        DocumentTexts texts)
    {
        var templateTexts = texts.GetTemplate(template);
        return new DocumentTemplateDefinition(
            template,
            DocumentOutputFormat.Pdf,
            code,
            templateTexts.Name,
            templateTexts.Description);
    }

    private static string GetTitle(
        EstimateDocumentTemplate template,
        EstimateDocumentModel estimate,
        DocumentTexts texts) =>
        $"{texts.GetTemplate(template).TitlePrefix} · {estimate.EstimateNumber}";

    private static string CreateFileName(
        EstimateDocumentTemplate template,
        EstimateDocumentModel estimate,
        DocumentTexts texts)
    {
        var safeNumber = string.Join(
            "-",
            estimate.EstimateNumber.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));

        return $"smartestimate-{safeNumber}-{texts.GetTemplate(template).FileNameSuffix}-{texts.Locale.ToCode()}.pdf";
    }

    private static string GetInitials(string companyName)
    {
        var parts = companyName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            return "SE";
        }

        return string.Concat(parts.Take(2).Select(part => char.ToUpperInvariant(part[0])));
    }

    private static string FormatMoney(decimal amount, string currency, DocumentTexts texts) =>
        string.Create(texts.Culture, $"{amount:N2} {currency}");

    private static bool HasAnyItems(EstimateDocumentZone zone) =>
        zone.WorkItems.Count > 0 || zone.MaterialItems.Count > 0;

    private static string? FormatCustomerContacts(EstimateDocumentModel estimate)
    {
        var contacts = new[] { estimate.CustomerPhone, estimate.CustomerEmail }
            .Where(contact => !string.IsNullOrWhiteSpace(contact))
            .ToArray();

        return contacts.Length == 0 ? null : string.Join(" · ", contacts);
    }

    private static string FormatQuantity(decimal quantity, DocumentTexts texts) =>
        quantity.ToString("0.###", texts.Culture);

    private static string FormatDate(DateTimeOffset date, DocumentTexts texts) =>
        date.ToString("d", texts.Culture);

    private static void EnsureFontResolver()
    {
        if (GlobalFontSettings.FontResolver is null)
        {
            GlobalFontSettings.FontResolver = new SmartEstimateFontResolver();
        }
    }

    private static Color HexColor(string hex)
    {
        var normalized = hex.TrimStart('#');
        if (normalized.Length != 6)
        {
            throw new ArgumentException("HEX color must contain exactly six characters.", nameof(hex));
        }

        return Color.FromRgb(
            byte.Parse(normalized[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture),
            byte.Parse(normalized.AsSpan(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
            byte.Parse(normalized.AsSpan(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
    }

    private sealed record DocumentMetaItem(string Label, string? Value);
}
