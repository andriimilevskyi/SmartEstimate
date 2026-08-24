using SmartEstimate.Documents.EstimateDocuments;
using SmartEstimate.Documents.EstimateDocuments.Pdf;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using Xunit;

namespace SmartEstimate.UnitTests.Documents;

public sealed class PdfEstimateDocumentRendererTests
{
    public static TheoryData<EstimateDocumentTemplate> Templates => new()
    {
        EstimateDocumentTemplate.FullEstimate,
        EstimateDocumentTemplate.ShortEstimate,
        EstimateDocumentTemplate.CommercialProposal
    };

    public static TheoryData<EstimateDocumentTemplate, DocumentLocale> TemplatesAndLocales => new()
    {
        { EstimateDocumentTemplate.FullEstimate, DocumentLocale.Uk },
        { EstimateDocumentTemplate.FullEstimate, DocumentLocale.En },
        { EstimateDocumentTemplate.FullEstimate, DocumentLocale.De },
        { EstimateDocumentTemplate.ShortEstimate, DocumentLocale.Uk },
        { EstimateDocumentTemplate.ShortEstimate, DocumentLocale.En },
        { EstimateDocumentTemplate.ShortEstimate, DocumentLocale.De },
        { EstimateDocumentTemplate.CommercialProposal, DocumentLocale.Uk },
        { EstimateDocumentTemplate.CommercialProposal, DocumentLocale.En },
        { EstimateDocumentTemplate.CommercialProposal, DocumentLocale.De }
    };

    [Fact]
    public void GetTemplatesReturnsThreePdfTemplates()
    {
        var renderer = new PdfEstimateDocumentRenderer();

        var templates = renderer.GetTemplates();

        Assert.Equal(DocumentOutputFormat.Pdf, renderer.Format);
        Assert.Collection(
            templates,
            template => Assert.Equal(EstimateDocumentTemplate.FullEstimate, template.Template),
            template => Assert.Equal(EstimateDocumentTemplate.ShortEstimate, template.Template),
            template => Assert.Equal(EstimateDocumentTemplate.CommercialProposal, template.Template));
    }

    [Theory]
    [MemberData(nameof(Templates))]
    public void RenderCreatesPdfPayloadForEveryTemplate(EstimateDocumentTemplate template)
    {
        var renderer = new PdfEstimateDocumentRenderer();

        var document = renderer.Render(CreateRequest(template));

        Assert.Equal("application/pdf", document.ContentType);
        Assert.EndsWith(".pdf", document.FileName, StringComparison.OrdinalIgnoreCase);
        Assert.True(document.Content.Length > 1_000);
        Assert.Equal("%PDF"u8.ToArray(), document.Content[..4]);
    }

    [Theory]
    [MemberData(nameof(TemplatesAndLocales))]
    public void RenderCreatesLocalizedPdfPayloadForEveryTemplateAndLocale(
        EstimateDocumentTemplate template,
        DocumentLocale locale)
    {
        var renderer = new PdfEstimateDocumentRenderer();

        var document = renderer.Render(CreateRequest(template, locale: locale));

        Assert.Equal("application/pdf", document.ContentType);
        Assert.EndsWith($"-{locale.ToCode()}.pdf", document.FileName, StringComparison.OrdinalIgnoreCase);
        Assert.True(document.Content.Length > 1_000);
        Assert.Equal("%PDF"u8.ToArray(), document.Content[..4]);
    }

    [Theory]
    [InlineData(EstimateDocumentTemplate.FullEstimate, DocumentLocale.Uk, "Повний кошторис", "Роботи", "Матеріали", "Загальна сума")]
    [InlineData(EstimateDocumentTemplate.FullEstimate, DocumentLocale.En, "Full Estimate", "Works", "Materials", "Grand total")]
    [InlineData(EstimateDocumentTemplate.FullEstimate, DocumentLocale.De, "Kostenaufstellung", "Leistungen", "Materialien", "Gesamtsumme")]
    [InlineData(EstimateDocumentTemplate.ShortEstimate, DocumentLocale.Uk, "Короткий кошторис", "Роботи", "Матеріали", "Загальна сума")]
    [InlineData(EstimateDocumentTemplate.ShortEstimate, DocumentLocale.En, "Short Estimate", "Works", "Materials", "Grand total")]
    [InlineData(EstimateDocumentTemplate.ShortEstimate, DocumentLocale.De, "Kurz-Kostenvoranschlag", "Leistungen", "Materialien", "Gesamtsumme")]
    [InlineData(EstimateDocumentTemplate.CommercialProposal, DocumentLocale.Uk, "Комерційна пропозиція", "Роботи", "Матеріали", "Загальна сума")]
    [InlineData(EstimateDocumentTemplate.CommercialProposal, DocumentLocale.En, "Commercial Proposal", "Works", "Materials", "Grand total")]
    [InlineData(EstimateDocumentTemplate.CommercialProposal, DocumentLocale.De, "Angebot", "Leistungen", "Materialien", "Gesamtsumme")]
    public void RenderUsesLocalizedSystemLabelsForEveryTemplateAndLocale(
        EstimateDocumentTemplate template,
        DocumentLocale locale,
        string documentName,
        string works,
        string materials,
        string grandTotal)
    {
        var text = ExtractDocumentText(PdfEstimateDocumentRenderer.CreateDocumentForTesting(CreateRequest(template, locale: locale)));

        Assert.Contains(documentName, text);
        Assert.Contains(works, text);
        Assert.Contains(materials, text);
        Assert.Contains(grandTotal, text);
    }

    [Theory]
    [InlineData(DocumentLocale.En)]
    [InlineData(DocumentLocale.De)]
    public void NonUkrainianDocumentsDoNotContainUkrainianSystemLabels(DocumentLocale locale)
    {
        var text = ExtractDocumentText(PdfEstimateDocumentRenderer.CreateDocumentForTesting(CreateRequest(
            EstimateDocumentTemplate.FullEstimate,
            locale: locale)));

        Assert.Equal(0, CountExactLines(text, "Роботи"));
        Assert.Equal(0, CountExactLines(text, "Матеріали"));
        Assert.Equal(0, CountExactLines(text, "Загальна сума"));
        Assert.Equal(0, CountExactLines(text, "Замовник"));
    }

    [Fact]
    public void MissingLocaleUsesUkrainianByDefault()
    {
        var text = ExtractDocumentText(PdfEstimateDocumentRenderer.CreateDocumentForTesting(CreateRequest(EstimateDocumentTemplate.FullEstimate)));

        Assert.Contains("Повний кошторис", text);
        Assert.Contains("Роботи", text);
        Assert.Contains("Матеріали", text);
    }

    [Theory]
    [InlineData("fr")]
    [InlineData("en-US")]
    public void InvalidLocaleIsRejectedByDocumentLocaleParser(string value)
    {
        var parsed = DocumentLocales.TryParse(value, out var locale);

        Assert.False(parsed);
        Assert.Equal(DocumentLocale.Uk, locale);
    }

    [Theory]
    [InlineData(DocumentLocale.Uk, "Квартира")]
    [InlineData(DocumentLocale.En, "Apartment")]
    [InlineData(DocumentLocale.De, "Wohnung")]
    public void RenderLocalizesObjectType(DocumentLocale locale, string label)
    {
        var text = ExtractDocumentText(PdfEstimateDocumentRenderer.CreateDocumentForTesting(CreateRequest(
            EstimateDocumentTemplate.FullEstimate,
            locale: locale,
            objectType: "Apartment")));

        Assert.Contains(label, text);
    }

    [Theory]
    [InlineData(DocumentLocale.Uk)]
    [InlineData(DocumentLocale.En)]
    [InlineData(DocumentLocale.De)]
    public void RenderKeepsUserEnteredNamesUnchanged(DocumentLocale locale)
    {
        var text = ExtractDocumentText(PdfEstimateDocumentRenderer.CreateDocumentForTesting(CreateRequest(
            EstimateDocumentTemplate.FullEstimate,
            locale: locale)));

        Assert.Contains("ТОВ Замовник", text);
        Assert.Contains("Квартира Антоновича", text);
        Assert.Contains("Кухня", text);
        Assert.Contains("Штукатурка стін", text);
        Assert.Contains("Штукатурна суміш", text);
        Assert.Contains("Капітальний ремонт квартири", text);
    }

    [Theory]
    [InlineData(DocumentLocale.Uk, "Гіпсокартон", "Монтаж гіпсокартону")]
    [InlineData(DocumentLocale.En, "Drywall", "Drywall installation")]
    [InlineData(DocumentLocale.De, "Gipskarton", "Gipskarton montieren")]
    public void RenderUsesEstimateDisplayNamesPreparedForLocale(
        DocumentLocale locale,
        string materialName,
        string workName)
    {
        var text = ExtractDocumentText(PdfEstimateDocumentRenderer.CreateDocumentForTesting(CreateRequest(
            EstimateDocumentTemplate.FullEstimate,
            locale: locale,
            zones:
            [
                CreateZone(
                    "Кухня",
                    workItems: [new EstimateDocumentLineItem(workName, 12m, "m²", 300m, 3_600m, null)],
                    materialItems: [new EstimateDocumentLineItem(materialName, 12m, "m²", 180m, 2_160m, null)])
            ])));

        Assert.Contains(materialName, text);
        Assert.Contains(workName, text);
    }

    [Theory]
    [InlineData(DocumentLocale.Uk, "05.08.2026", "7 500,00 UAH")]
    [InlineData(DocumentLocale.En, "8/5/2026", "7,500.00 UAH")]
    [InlineData(DocumentLocale.De, "05.08.2026", "7.500,00 UAH")]
    public void RenderFormatsDateAndCurrencyPerLocale(DocumentLocale locale, string date, string amount)
    {
        var text = ExtractDocumentText(PdfEstimateDocumentRenderer.CreateDocumentForTesting(CreateRequest(
            EstimateDocumentTemplate.FullEstimate,
            locale: locale)));

        Assert.Contains(date, text);
        Assert.Contains(amount, text);
    }

    [Theory]
    [MemberData(nameof(Templates))]
    public void RenderOmitsEmptyZonesForEveryTemplate(EstimateDocumentTemplate template)
    {
        var text = ExtractDocumentText(PdfEstimateDocumentRenderer.CreateDocumentForTesting(CreateRequest(
            template,
            [
                CreateZone("Кухня", workItems: [CreateWorkItem()], materialItems: []),
                CreateZone("Балкон", workItems: [], materialItems: [])
            ])));

        Assert.Contains("Кухня", text);
        Assert.DoesNotContain("Балкон", text);
        Assert.DoesNotContain("Позиції відсутні.", text);
    }

    [Theory]
    [MemberData(nameof(Templates))]
    public void RenderOmitsEmptyMaterialSectionForWorkOnlyZoneForEveryTemplate(EstimateDocumentTemplate template)
    {
        var text = ExtractDocumentText(PdfEstimateDocumentRenderer.CreateDocumentForTesting(CreateRequest(
            template,
            [CreateZone("Кухня", workItems: [CreateWorkItem()], materialItems: [])])));

        Assert.Equal(1, CountExactLines(text, "Роботи"));
        Assert.Equal(0, CountExactLines(text, "Матеріали"));
        Assert.Contains("Вартість робіт", text);
        Assert.DoesNotContain("Вартість матеріалів", text);
        Assert.Contains("Всього", text);
        Assert.DoesNotContain("Позиції відсутні.", text);
    }

    [Theory]
    [MemberData(nameof(Templates))]
    public void RenderOmitsEmptyWorkSectionForMaterialOnlyZoneForEveryTemplate(EstimateDocumentTemplate template)
    {
        var text = ExtractDocumentText(PdfEstimateDocumentRenderer.CreateDocumentForTesting(CreateRequest(
            template,
            [CreateZone("Кухня", workItems: [], materialItems: [CreateMaterialItem()])])));

        Assert.Equal(0, CountExactLines(text, "Роботи"));
        Assert.Equal(1, CountExactLines(text, "Матеріали"));
        Assert.DoesNotContain("Вартість робіт", text);
        Assert.Contains("Вартість матеріалів", text);
        Assert.Contains("Всього", text);
        Assert.DoesNotContain("Позиції відсутні.", text);
    }

    [Theory]
    [MemberData(nameof(Templates))]
    public void RenderIncludesBothSectionsForZoneWithWorkAndMaterialsForEveryTemplate(EstimateDocumentTemplate template)
    {
        var text = ExtractDocumentText(PdfEstimateDocumentRenderer.CreateDocumentForTesting(CreateRequest(
            template,
            [CreateZone("Кухня", workItems: [CreateWorkItem()], materialItems: [CreateMaterialItem()])])));

        Assert.Equal(1, CountExactLines(text, "Роботи"));
        Assert.Equal(1, CountExactLines(text, "Матеріали"));
        Assert.Contains("Вартість робіт", text);
        Assert.Contains("Вартість матеріалів", text);
        Assert.Contains("Всього", text);
    }

    [Theory]
    [MemberData(nameof(Templates))]
    public void RenderKeepsNonEmptyZonesAndOmitsEmptyZoneForEveryTemplate(EstimateDocumentTemplate template)
    {
        var text = ExtractDocumentText(PdfEstimateDocumentRenderer.CreateDocumentForTesting(CreateRequest(
            template,
            [
                CreateZone("Кухня", workItems: [CreateWorkItem()], materialItems: []),
                CreateZone("Ванна", workItems: [], materialItems: [CreateMaterialItem()]),
                CreateZone("Балкон", workItems: [], materialItems: [])
            ])));

        Assert.Contains("Кухня", text);
        Assert.Contains("Ванна", text);
        Assert.DoesNotContain("Балкон", text);
    }

    [Theory]
    [MemberData(nameof(Templates))]
    public void RenderOmitsEmptyOptionalMetadataForEveryTemplate(EstimateDocumentTemplate template)
    {
        var request = CreateRequest(
            template,
            [CreateZone("Кухня", workItems: [CreateWorkItem()], materialItems: [])],
            customerPhone: null,
            customerEmail: null,
            objectAddress: null,
            totalArea: null,
            objectDescription: null,
            notes: null);

        var text = ExtractDocumentText(PdfEstimateDocumentRenderer.CreateDocumentForTesting(request));

        Assert.DoesNotContain("Контакти", text);
        Assert.DoesNotContain("Адреса", text);
        Assert.DoesNotContain("Площа", text);
        Assert.DoesNotContain("Опис об'єкта", text);
        Assert.DoesNotContain("Опис", text);
        Assert.DoesNotContain("—", text);
    }

    [Theory]
    [MemberData(nameof(Templates))]
    public void RenderZoneTotalsMatchVisibleItemTypesForEveryTemplate(EstimateDocumentTemplate template)
    {
        var text = ExtractDocumentText(PdfEstimateDocumentRenderer.CreateDocumentForTesting(CreateRequest(
            template,
            [
                CreateZone("Кухня", totalLabor: 1_200m, totalMaterials: 0m, grandTotal: 1_200m, workItems: [CreateWorkItem(1_200m)], materialItems: []),
                CreateZone("Ванна", totalLabor: 0m, totalMaterials: 700m, grandTotal: 700m, workItems: [], materialItems: [CreateMaterialItem(700m)]),
                CreateZone("Спальня", totalLabor: 500m, totalMaterials: 300m, grandTotal: 800m, workItems: [CreateWorkItem(500m)], materialItems: [CreateMaterialItem(300m)])
            ])));

        Assert.Equal(2, CountExactLines(text, "Вартість робіт"));
        Assert.Equal(2, CountExactLines(text, "Вартість матеріалів"));
        Assert.Equal(3, CountExactLines(text, "Всього"));
        Assert.Contains("1 200,00 UAH", text);
        Assert.Contains("700,00 UAH", text);
        Assert.Contains("800,00 UAH", text);
    }

    [Fact]
    public void CommercialProposalIntroUsesNaturalLineSpacing()
    {
        var document = PdfEstimateDocumentRenderer.CreateDocumentForTesting(CreateRequest(
            EstimateDocumentTemplate.CommercialProposal,
            [CreateZone("Кухня", workItems: [CreateWorkItem()], materialItems: [])]));

        var intro = FindParagraph(
            document,
            "Пропонуємо виконання ремонтно-будівельних робіт за наведеною нижче структурою. " +
            "Документ підготовлено для попереднього погодження обсягу робіт, матеріалів та бюджету.");

        Assert.NotNull(intro);
        Assert.Equal(LineSpacingRule.Single, intro.Format.LineSpacingRule);
        Assert.False(intro.Format.KeepTogether);
        Assert.False(intro.Format.KeepWithNext);
        Assert.False(intro.Format.PageBreakBefore);
        Assert.Equal(Unit.FromPoint(12), intro.Format.SpaceBefore);
        Assert.Equal(Unit.FromPoint(6), intro.Format.SpaceAfter);
    }

    [Theory]
    [InlineData(EstimateDocumentTemplate.FullEstimate)]
    [InlineData(EstimateDocumentTemplate.ShortEstimate)]
    public void NonCommercialTemplatesDoNotRenderCommercialIntro(EstimateDocumentTemplate template)
    {
        var text = ExtractDocumentText(PdfEstimateDocumentRenderer.CreateDocumentForTesting(CreateRequest(template)));

        Assert.DoesNotContain("Пропонуємо виконання ремонтно-будівельних робіт", text);
    }

    private static EstimateDocumentRenderRequest CreateRequest(
        EstimateDocumentTemplate template,
        IReadOnlyCollection<EstimateDocumentZone>? zones = null,
        string? customerPhone = "+380 44 000 00 00",
        string? customerEmail = "client@example.test",
        string? objectAddress = "Київ, вул. Антоновича, 44",
        decimal? totalArea = 86.5m,
        string? objectDescription = "Об'єкт для капітального ремонту",
        string? notes = "Капітальний ремонт квартири",
        DocumentLocale locale = DocumentLocale.Uk,
        string objectType = "Apartment")
    {
        var estimate = new EstimateDocumentModel(
            Guid.NewGuid(),
            "EST-PDF-001",
            "UAH",
            "ТОВ Замовник",
            customerPhone,
            customerEmail,
            "Квартира Антоновича",
            objectType,
            objectAddress,
            totalArea,
            objectDescription,
            notes,
            5_600m,
            1_900m,
            7_500m,
            new DateTimeOffset(2026, 8, 5, 10, 0, 0, TimeSpan.Zero),
            zones ?? [CreateZone("Кухня", workItems: [CreateWorkItem()], materialItems: [CreateMaterialItem()])]);
        var company = new DocumentCompanyProfile(
            "SmartEstimate",
            ["Київ, Україна", "hello@smartestimate.local"],
            null);

        return new EstimateDocumentRenderRequest(
            template,
            estimate,
            company,
            new DateTimeOffset(2026, 8, 5, 11, 0, 0, TimeSpan.Zero),
            locale);
    }

    private static EstimateDocumentZone CreateZone(
        string name,
        decimal? totalLabor = null,
        decimal? totalMaterials = null,
        decimal? grandTotal = null,
        IReadOnlyCollection<EstimateDocumentLineItem>? workItems = null,
        IReadOnlyCollection<EstimateDocumentLineItem>? materialItems = null)
    {
        var works = workItems ?? [];
        var materials = materialItems ?? [];
        var labor = totalLabor ?? works.Sum(item => item.Total);
        var materialTotal = totalMaterials ?? materials.Sum(item => item.Total);

        return new EstimateDocumentZone(
            Guid.NewGuid(),
            name,
            labor,
            materialTotal,
            grandTotal ?? labor + materialTotal,
            works,
            materials);
    }

    private static EstimateDocumentLineItem CreateWorkItem(decimal total = 5_600m) =>
        new("Штукатурка стін", 20m, "м²", total / 20m, total, "Під маяки");

    private static EstimateDocumentLineItem CreateMaterialItem(decimal total = 1_900m) =>
        new("Штукатурна суміш", 10m, "міш", total / 10m, total, null);

    private static string ExtractDocumentText(Document document)
    {
        var lines = new List<string>();

        foreach (Section section in document.Sections!)
        {
            ExtractElements(section.Elements!, lines);
        }

        return string.Join('\n', lines)
            .Replace('\u00A0', ' ')
            .Replace('\u202F', ' ');
    }

    private static void ExtractElements(DocumentElements elements, ICollection<string> lines)
    {
        foreach (var element in elements)
        {
            switch (element)
            {
                case Paragraph paragraph:
                    var text = ExtractParagraphText(paragraph);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        lines.Add(text);
                    }

                    break;
                case Table table:
                    ExtractTableText(table, lines);
                    break;
            }
        }
    }

    private static void ExtractTableText(Table table, ICollection<string> lines)
    {
        foreach (Row row in table.Rows!)
        {
            foreach (Cell cell in row.Cells!)
            {
                ExtractElements(cell.Elements!, lines);
            }
        }
    }

    private static string ExtractParagraphText(Paragraph paragraph)
    {
        var parts = new List<string>();

        foreach (var element in paragraph.Elements)
        {
            if (element is Text text && !string.IsNullOrWhiteSpace(text.Content))
            {
                parts.Add(text.Content);
            }
        }

        return string.Join(string.Empty, parts);
    }

    private static Paragraph? FindParagraph(Document document, string value)
    {
        foreach (Section section in document.Sections!)
        {
            var paragraph = FindParagraph(section.Elements!, value);
            if (paragraph is not null)
            {
                return paragraph;
            }
        }

        return null;
    }

    private static Paragraph? FindParagraph(DocumentElements elements, string value)
    {
        foreach (var element in elements)
        {
            switch (element)
            {
                case Paragraph paragraph when string.Equals(ExtractParagraphText(paragraph), value, StringComparison.Ordinal):
                    return paragraph;
                case Table table:
                    var tableParagraph = FindParagraph(table, value);
                    if (tableParagraph is not null)
                    {
                        return tableParagraph;
                    }

                    break;
            }
        }

        return null;
    }

    private static Paragraph? FindParagraph(Table table, string value)
    {
        foreach (Row row in table.Rows!)
        {
            foreach (Cell cell in row.Cells!)
            {
                var paragraph = FindParagraph(cell.Elements!, value);
                if (paragraph is not null)
                {
                    return paragraph;
                }
            }
        }

        return null;
    }

    private static int CountExactLines(string text, string value) =>
        text.Split('\n').Count(line => string.Equals(line, value, StringComparison.Ordinal));
}
