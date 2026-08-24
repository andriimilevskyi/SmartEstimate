namespace SmartEstimate.Domain.Pricing;

public sealed class CatalogPriceHistoryEntry
{
    private CatalogPriceHistoryEntry()
    {
    }

    private CatalogPriceHistoryEntry(
        Guid id,
        CatalogPrice price,
        PriceChangeType changeType,
        DateTimeOffset changedAt,
        Guid? changedBy)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("History entry identifier is required.", nameof(id));
        }

        Id = id;
        CatalogPriceId = price.Id;
        TargetType = price.TargetType;
        KnowledgeMaterialId = price.KnowledgeMaterialId;
        ConstructionWorkId = price.ConstructionWorkId;
        Amount = price.Amount;
        Currency = price.Currency;
        RegionCode = price.RegionCode;
        SupplierId = price.SupplierId;
        SupplierName = price.SupplierName;
        EffectiveFrom = price.EffectiveFrom;
        EffectiveUntil = price.EffectiveUntil;
        SourceType = price.SourceType;
        PriceStatus = price.Status;
        Notes = price.Notes;
        ChangeType = changeType;
        ChangedAt = changedAt;
        ChangedBy = changedBy;
    }

    public Guid Id { get; private set; }
    public Guid CatalogPriceId { get; private set; }
    public PriceTargetType TargetType { get; private set; }
    public Guid? KnowledgeMaterialId { get; private set; }
    public Guid? ConstructionWorkId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public string? RegionCode { get; private set; }
    public Guid? SupplierId { get; private set; }
    public string? SupplierName { get; private set; }
    public DateTimeOffset EffectiveFrom { get; private set; }
    public DateTimeOffset? EffectiveUntil { get; private set; }
    public PriceSourceType SourceType { get; private set; }
    public PriceStatus PriceStatus { get; private set; }
    public string? Notes { get; private set; }
    public PriceChangeType ChangeType { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; }
    public Guid? ChangedBy { get; private set; }

    public static CatalogPriceHistoryEntry Capture(
        Guid id,
        CatalogPrice price,
        PriceChangeType changeType,
        DateTimeOffset changedAt,
        Guid? changedBy = null) =>
        new(id, price, changeType, changedAt, changedBy);
}
