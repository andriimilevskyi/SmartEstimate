namespace SmartEstimate.Documents.EstimateDocuments;

/// <summary>
/// In-memory translation catalog for document system text.
/// </summary>
public sealed class DocumentTextProvider : IDocumentTextProvider
{
    private static readonly Dictionary<DocumentLocale, DocumentTexts> Catalog = new()
    {
        [DocumentLocale.Uk] = CreateUk(),
        [DocumentLocale.En] = CreateEn(),
        [DocumentLocale.De] = CreateDe()
    };

    public DocumentTexts GetTexts(DocumentLocale locale) =>
        Catalog.TryGetValue(locale, out var texts) ? texts : Catalog[DocumentLocales.Default];

    private static DocumentTexts CreateUk() => new(
        DocumentLocale.Uk,
        DocumentLocale.Uk.ToCulture(),
        "Кошторис",
        "Номер кошторису",
        "Дата створення",
        "Замовник",
        "Контакти",
        "Об'єкт",
        "Тип об'єкта",
        "Адреса",
        "Площа",
        "Валюта",
        "Документ",
        "Опис об'єкта",
        "Опис",
        "Пропонуємо виконання ремонтно-будівельних робіт за наведеною нижче структурою. Документ підготовлено для попереднього погодження обсягу робіт, матеріалів та бюджету.",
        "У кошторисі поки немає позицій.",
        "Роботи",
        "Матеріали",
        "№",
        "Найменування",
        "Од.",
        "К-сть",
        "Ціна",
        "Сума",
        "Коментар",
        "Вартість робіт",
        "Вартість матеріалів",
        "Всього",
        "Підсумки",
        "Ітого роботи",
        "Ітого матеріали",
        "Загальна сума",
        "Підписи сторін",
        "Виконавець ____________________",
        "Замовник ______________________",
        "Дата __________________________",
        "сформовано",
        "стор.",
        CreateTemplateTexts(
            ("Повний кошторис", "Детальний кошторис з роботами, матеріалами, кількістю, цінами та коментарями.", "Кошторис", "full"),
            ("Короткий кошторис", "Стисла версія для швидкого погодження обсягу та бюджетів по зонах.", "Кошторис", "short"),
            ("Комерційна пропозиція", "Презентаційний документ для замовника з підсумками та місцем для підписів.", "Комерційна пропозиція", "proposal")),
        new Dictionary<string, string>
        {
            ["Apartment"] = "Квартира",
            ["PrivateHouse"] = "Приватний будинок",
            ["CommercialSpace"] = "Комерційне приміщення",
            ["Office"] = "Офіс",
            ["IndustrialSpace"] = "Виробниче приміщення",
            ["Other"] = "Інше"
        });

    private static DocumentTexts CreateEn() => new(
        DocumentLocale.En,
        DocumentLocale.En.ToCulture(),
        "Estimate",
        "Estimate number",
        "Created date",
        "Customer",
        "Contacts",
        "Project",
        "Project type",
        "Address",
        "Area",
        "Currency",
        "Document",
        "Project description",
        "Notes",
        "We propose to complete the renovation and construction works according to the structure below. This document is prepared for preliminary approval of the scope, materials, and budget.",
        "This estimate does not contain any items yet.",
        "Works",
        "Materials",
        "No.",
        "Item",
        "Unit",
        "Qty",
        "Unit price",
        "Amount",
        "Comment",
        "Work total",
        "Material total",
        "Total",
        "Totals",
        "Works subtotal",
        "Materials subtotal",
        "Grand total",
        "Signatures",
        "Contractor ____________________",
        "Customer ______________________",
        "Date __________________________",
        "generated",
        "page",
        CreateTemplateTexts(
            ("Full Estimate", "Detailed estimate with works, materials, quantities, prices, and comments.", "Estimate", "full"),
            ("Short Estimate", "Compact version for quick approval of scope and zone budgets.", "Estimate", "short"),
            ("Commercial Proposal", "Client-facing proposal with totals and signature space.", "Commercial proposal", "proposal")),
        new Dictionary<string, string>
        {
            ["Apartment"] = "Apartment",
            ["PrivateHouse"] = "Private house",
            ["CommercialSpace"] = "Commercial space",
            ["Office"] = "Office",
            ["IndustrialSpace"] = "Industrial space",
            ["Other"] = "Other"
        });

    private static DocumentTexts CreateDe() => new(
        DocumentLocale.De,
        DocumentLocale.De.ToCulture(),
        "Kostenvoranschlag",
        "Nummer",
        "Erstellt am",
        "Kunde",
        "Kontakt",
        "Projekt",
        "Projekttyp",
        "Adresse",
        "Fläche",
        "Währung",
        "Dokument",
        "Projektbeschreibung",
        "Notizen",
        "Wir bieten die Ausführung der Renovierungs- und Bauleistungen gemäß der untenstehenden Struktur an. Dieses Dokument dient der vorläufigen Abstimmung von Leistungsumfang, Materialien und Budget.",
        "Dieser Kostenvoranschlag enthält noch keine Positionen.",
        "Leistungen",
        "Materialien",
        "Nr.",
        "Position",
        "Einheit",
        "Menge",
        "Einzelpreis",
        "Betrag",
        "Bemerkung",
        "Leistungssumme",
        "Materialsumme",
        "Gesamt",
        "Summen",
        "Leistungssumme",
        "Materialsumme",
        "Gesamtsumme",
        "Unterschriften",
        "Auftragnehmer __________________",
        "Kunde _________________________",
        "Datum _________________________",
        "erstellt",
        "Seite",
        CreateTemplateTexts(
            ("Kostenaufstellung", "Detaillierte Kostenaufstellung mit Leistungen, Materialien, Mengen, Preisen und Bemerkungen.", "Kostenaufstellung", "full"),
            ("Kurz-Kostenvoranschlag", "Kompakte Version zur schnellen Abstimmung von Umfang und Budgets je Zone.", "Kostenvoranschlag", "short"),
            ("Angebot", "Kundenorientiertes Angebot mit Summen und Platz für Unterschriften.", "Angebot", "proposal")),
        new Dictionary<string, string>
        {
            ["Apartment"] = "Wohnung",
            ["PrivateHouse"] = "Einfamilienhaus",
            ["CommercialSpace"] = "Gewerbefläche",
            ["Office"] = "Büro",
            ["IndustrialSpace"] = "Industriefläche",
            ["Other"] = "Sonstiges"
        });

    private static Dictionary<EstimateDocumentTemplate, DocumentTemplateTexts> CreateTemplateTexts(
        (string Name, string Description, string TitlePrefix, string FileNameSuffix) full,
        (string Name, string Description, string TitlePrefix, string FileNameSuffix) shortEstimate,
        (string Name, string Description, string TitlePrefix, string FileNameSuffix) commercial) =>
        new Dictionary<EstimateDocumentTemplate, DocumentTemplateTexts>
        {
            [EstimateDocumentTemplate.FullEstimate] = new(full.Name, full.Description, full.TitlePrefix, full.FileNameSuffix),
            [EstimateDocumentTemplate.ShortEstimate] = new(shortEstimate.Name, shortEstimate.Description, shortEstimate.TitlePrefix, shortEstimate.FileNameSuffix),
            [EstimateDocumentTemplate.CommercialProposal] = new(commercial.Name, commercial.Description, commercial.TitlePrefix, commercial.FileNameSuffix)
        };
}
