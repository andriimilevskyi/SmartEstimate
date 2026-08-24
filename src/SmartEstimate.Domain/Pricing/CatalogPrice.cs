namespace SmartEstimate.Domain.Pricing;

public sealed class CatalogPrice
{
    private CatalogPrice()
    {
    }

    private CatalogPrice(
        Guid id,
        PriceTarget target,
        decimal amount,
        PriceScope scope,
        DateTimeOffset effectiveFrom,
        PriceSourceType sourceType,
        string? notes,
        DateTimeOffset createdAt,
        Guid? actorId)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Price identifier is required.", nameof(id));
        }

        Id = id;
        TargetType = target.Type;
        KnowledgeMaterialId = target.Type == PriceTargetType.Material ? target.Id : null;
        ConstructionWorkId = target.Type == PriceTargetType.ConstructionWork ? target.Id : null;
        Amount = NormalizeAmount(amount);
        Currency = scope.Currency;
        RegionCode = scope.RegionCode;
        SupplierId = scope.SupplierId;
        SupplierName = scope.SupplierName;
        EffectiveFrom = effectiveFrom;
        SourceType = sourceType;
        Notes = NormalizeNotes(notes);
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        CreatedBy = actorId;
        UpdatedBy = actorId;
        Version = 1;
    }

    public Guid Id { get; private set; }
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
    public PriceStatus Status { get; private set; } = PriceStatus.Active;
    public string? Notes { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? ArchivedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public int Version { get; private set; }

    public Guid TargetId => TargetType == PriceTargetType.Material
        ? KnowledgeMaterialId!.Value
        : ConstructionWorkId!.Value;

    public PriceScope Scope => new(Currency, RegionCode, SupplierId, SupplierName);

    public bool IsCurrentAt(DateTimeOffset date) =>
        Status == PriceStatus.Active
        && EffectiveFrom <= date
        && (!EffectiveUntil.HasValue || EffectiveUntil.Value > date);

    public static CatalogPrice Create(
        Guid id,
        PriceTarget target,
        decimal amount,
        PriceScope scope,
        DateTimeOffset effectiveFrom,
        PriceSourceType sourceType,
        string? notes,
        DateTimeOffset createdAt,
        Guid? actorId = null) =>
        new(id, target, amount, scope, effectiveFrom, sourceType, notes, createdAt, actorId);

    public void Close(DateTimeOffset effectiveUntil, DateTimeOffset updatedAt, Guid? actorId = null)
    {
        if (effectiveUntil <= EffectiveFrom)
        {
            throw new ArgumentOutOfRangeException(nameof(effectiveUntil), "Price validity end must be after its effective start.");
        }

        EffectiveUntil = effectiveUntil;
        Touch(updatedAt, actorId);
    }

    public void Archive(DateTimeOffset archivedAt, Guid? actorId = null)
    {
        if (Status == PriceStatus.Archived)
        {
            return;
        }

        Status = PriceStatus.Archived;
        ArchivedAt = archivedAt;
        Touch(archivedAt, actorId);
    }

    private void Touch(DateTimeOffset updatedAt, Guid? actorId)
    {
        UpdatedAt = updatedAt;
        UpdatedBy = actorId;
        checked
        {
            Version++;
        }
    }

    private static decimal NormalizeAmount(decimal amount)
    {
        if (amount < decimal.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Price amount cannot be negative.");
        }

        if (decimal.Round(amount, 2, MidpointRounding.AwayFromZero) != amount)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Price amount cannot have more than two decimal places.");
        }

        return amount;
    }

    private static string? NormalizeNotes(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > 1_000)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Price notes cannot exceed 1000 characters.");
        }

        return normalized;
    }
}
