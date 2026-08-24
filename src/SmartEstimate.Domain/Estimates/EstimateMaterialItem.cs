using SmartEstimate.Domain.Estimates.ValueObjects;

namespace SmartEstimate.Domain.Estimates;

/// <summary>
/// A material line owned by an <see cref="Estimate"/> aggregate.
/// </summary>
public sealed class EstimateMaterialItem
{
    private EstimateMaterialItem()
    {
    }

    internal EstimateMaterialItem(
        Guid id,
        Guid estimateId,
        Guid zoneId,
        string name,
        Quantity quantity,
        MeasurementUnit measurementUnit,
        Money unitPrice,
        string? notes,
        DateTimeOffset createdAt,
        string? knowledgeItemId,
        LocalizedNameSnapshot? nameSnapshot,
        EstimateItemNameSource nameSource,
        Guid? sourcePriceId,
        DateTimeOffset? priceCapturedAt,
        bool isUnitPriceManuallyOverridden)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Material item identifier is required.", nameof(id));
        }

        if (estimateId == Guid.Empty)
        {
            throw new ArgumentException("Estimate identifier is required.", nameof(estimateId));
        }

        Id = id;
        EstimateId = estimateId;
        ZoneId = zoneId == Guid.Empty ? throw new ArgumentException("Zone identifier is required.", nameof(zoneId)) : zoneId;
        Name = NormalizeName(name);
        Quantity = quantity ?? throw new ArgumentNullException(nameof(quantity));
        MeasurementUnit = measurementUnit ?? throw new ArgumentNullException(nameof(measurementUnit));
        UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
        Notes = NormalizeNotes(notes);
        KnowledgeItemId = NormalizeKnowledgeItemId(knowledgeItemId);
        NameSnapshot = nameSnapshot;
        NameSource = NormalizeNameSource(nameSource, nameSnapshot);
        SourcePriceId = sourcePriceId == Guid.Empty ? null : sourcePriceId;
        PriceCapturedAt = priceCapturedAt;
        IsUnitPriceManuallyOverridden = isUnitPriceManuallyOverridden;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public Guid EstimateId { get; private set; }

    public Guid ZoneId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public Quantity Quantity { get; private set; } = null!;

    public MeasurementUnit MeasurementUnit { get; private set; } = null!;

    public Money UnitPrice { get; private set; } = null!;

    public string? Notes { get; private set; }

    /// <summary>
    /// Stable identifier of the material record in the PostgreSQL Knowledge store.
    /// It is optional only for legacy manually-created Sprint 1 lines.
    /// </summary>
    public string? KnowledgeItemId { get; private set; }

    public LocalizedNameSnapshot? NameSnapshot { get; private set; }

    public EstimateItemNameSource NameSource { get; private set; } = EstimateItemNameSource.Legacy;

    /// <summary>
    /// Optional Pricing catalog price used when the line was created. The amount is still stored as a snapshot.
    /// </summary>
    public Guid? SourcePriceId { get; private set; }

    public DateTimeOffset? PriceCapturedAt { get; private set; }

    public bool IsUnitPriceManuallyOverridden { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public Money Total => new(
        decimal.Round(Quantity.Value * UnitPrice.Amount, 2, MidpointRounding.AwayFromZero),
        UnitPrice.Currency);

    internal void Update(Quantity quantity, Money unitPrice, string? notes, DateTimeOffset updatedAt)
    {
        if (UnitPrice.Amount != unitPrice.Amount || UnitPrice.Currency != unitPrice.Currency)
        {
            IsUnitPriceManuallyOverridden = true;
        }

        Quantity = quantity ?? throw new ArgumentNullException(nameof(quantity));
        UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
        Notes = NormalizeNotes(notes);
        UpdatedAt = updatedAt;
    }

    internal EstimateMaterialItem Duplicate(Guid id, DateTimeOffset createdAt) => new(
        id,
        EstimateId,
        ZoneId,
        Name,
        new Quantity(Quantity.Value),
        new MeasurementUnit(MeasurementUnit.Value),
        new Money(UnitPrice.Amount, UnitPrice.Currency),
        Notes,
        createdAt,
        KnowledgeItemId,
        DuplicateSnapshot(NameSnapshot),
        NameSource,
        SourcePriceId,
        PriceCapturedAt,
        IsUnitPriceManuallyOverridden);

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Material name is required.", nameof(name));
        }

        var normalized = name.Trim();
        if (normalized.Length > 256)
        {
            throw new ArgumentOutOfRangeException(nameof(name), "Material name cannot exceed 256 characters.");
        }

        return normalized;
    }

    private static string? NormalizeNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            return null;
        }

        var normalized = notes.Trim();
        if (normalized.Length > 2_000)
        {
            throw new ArgumentOutOfRangeException(nameof(notes), "Material item notes cannot exceed 2000 characters.");
        }

        return normalized;
    }

    private static string? NormalizeKnowledgeItemId(string? knowledgeItemId)
    {
        if (string.IsNullOrWhiteSpace(knowledgeItemId))
        {
            return null;
        }

        var normalized = knowledgeItemId.Trim();
        if (normalized.Length > 128)
        {
            throw new ArgumentOutOfRangeException(
                nameof(knowledgeItemId),
                "Knowledge item identifier cannot exceed 128 characters.");
        }

        return normalized;
    }

    private static EstimateItemNameSource NormalizeNameSource(
        EstimateItemNameSource nameSource,
        LocalizedNameSnapshot? nameSnapshot)
    {
        if (nameSource == EstimateItemNameSource.KnowledgeSnapshot && nameSnapshot is null)
        {
            throw new ArgumentException("A localized name snapshot is required for catalog-derived material items.", nameof(nameSource));
        }

        return nameSource;
    }

    private static LocalizedNameSnapshot? DuplicateSnapshot(LocalizedNameSnapshot? snapshot) =>
        snapshot is null ? null : new LocalizedNameSnapshot(snapshot.Uk, snapshot.En, snapshot.De);
}
