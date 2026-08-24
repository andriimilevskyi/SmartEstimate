using SmartEstimate.Domain.Estimates.ValueObjects;

namespace SmartEstimate.Domain.Estimates;

/// <summary>
/// Aggregate root for a commercial construction estimate.
/// </summary>
public sealed class Estimate
{
    private readonly List<EstimateZone> _zones = [];
    private readonly List<EstimateWorkItem> _workItems = [];
    private readonly List<EstimateMaterialItem> _materialItems = [];

    private Estimate()
    {
    }

    private Estimate(
        EstimateNumber number,
        Guid objectId,
        string currency,
        string? notes,
        DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid();
        Number = number ?? throw new ArgumentNullException(nameof(number));
        ObjectId = objectId == Guid.Empty ? throw new ArgumentException("Object reference is required.", nameof(objectId)) : objectId;
        Currency = NormalizeCurrency(currency);
        Status = EstimateStatus.Draft;
        Notes = NormalizeNotes(notes);
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        Version = 1;
    }

    public Guid Id { get; private set; }

    public EstimateNumber Number { get; private set; } = null!;

    public Guid ObjectId { get; private set; }

    public string Currency { get; private set; } = string.Empty;

    public EstimateStatus Status { get; private set; }

    public string? Notes { get; private set; }

    public decimal TotalLaborAmount { get; private set; }

    public decimal TotalMaterialsAmount { get; private set; }

    public decimal GrandTotalAmount { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    public bool IsDeleted { get; private set; }

    public int Version { get; private set; }

    public IReadOnlyCollection<EstimateZone> Zones => _zones.AsReadOnly();

    public IReadOnlyCollection<EstimateWorkItem> WorkItems => _workItems.AsReadOnly();

    public IReadOnlyCollection<EstimateMaterialItem> MaterialItems => _materialItems.AsReadOnly();

    public Money TotalLabor => new(TotalLaborAmount, Currency);

    public Money TotalMaterials => new(TotalMaterialsAmount, Currency);

    public Money GrandTotal => new(GrandTotalAmount, Currency);

    public static Estimate Create(EstimateNumber number, Guid objectId, string currency, string? notes, DateTimeOffset createdAt)
    {
        var estimate = new Estimate(number, objectId, currency, notes, createdAt);
        estimate.AddInitialZone("Основна зона", 0, createdAt);
        return estimate;
    }

    public static Estimate Create(
        EstimateNumber number,
        Guid objectId,
        string currency,
        string? notes,
        IEnumerable<string> zoneNames,
        DateTimeOffset createdAt)
    {
        var estimate = new Estimate(number, objectId, currency, notes, createdAt);
        var normalizedZones = zoneNames
            .Where(zoneName => !string.IsNullOrWhiteSpace(zoneName))
            .Select(zoneName => zoneName.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (normalizedZones.Length == 0)
        {
            throw new ArgumentException("At least one estimate zone is required.", nameof(zoneNames));
        }

        for (var index = 0; index < normalizedZones.Length; index++)
        {
            estimate.AddInitialZone(normalizedZones[index], index, createdAt);
        }

        return estimate;
    }

    public void ChangeStatus(EstimateStatus status, DateTimeOffset updatedAt)
    {
        EnsureActive();
        Status = status;
        Touch(updatedAt);
    }

    public Guid AddZone(string name, int sortOrder, DateTimeOffset createdAt)
    {
        EnsureActive();

        var zone = new EstimateZone(Guid.NewGuid(), Id, name, sortOrder, createdAt);
        if (_zones.Any(existing => string.Equals(existing.Name, zone.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Zone '{zone.Name}' already exists in this estimate.");
        }

        _zones.Add(zone);
        Touch(createdAt);
        return zone.Id;
    }

    public void RenameZone(Guid zoneId, string name, DateTimeOffset updatedAt)
    {
        EnsureActive();
        var zone = GetZone(zoneId);
        var normalizedName = name.Trim();
        if (_zones.Any(existing => existing.Id != zoneId && string.Equals(existing.Name, normalizedName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Zone '{normalizedName}' already exists in this estimate.");
        }

        zone.Rename(name, updatedAt);
        Touch(updatedAt);
    }

    public void ReorderZones(IReadOnlyCollection<Guid> orderedZoneIds, DateTimeOffset updatedAt)
    {
        EnsureActive();
        ArgumentNullException.ThrowIfNull(orderedZoneIds);

        if (orderedZoneIds.Count != _zones.Count || orderedZoneIds.Distinct().Count() != _zones.Count)
        {
            throw new InvalidOperationException("The submitted zone order must include every zone exactly once.");
        }

        foreach (var zoneId in orderedZoneIds)
        {
            GetZone(zoneId);
        }

        var sortOrder = 0;
        foreach (var zoneId in orderedZoneIds)
        {
            GetZone(zoneId).ChangeSortOrder(sortOrder, updatedAt);
            sortOrder++;
        }

        Touch(updatedAt);
    }

    public void RemoveZone(Guid zoneId, DateTimeOffset updatedAt)
    {
        EnsureActive();

        var zone = GetZone(zoneId);
        _workItems.RemoveAll(item => item.ZoneId == zoneId);
        _materialItems.RemoveAll(item => item.ZoneId == zoneId);
        _zones.Remove(zone);

        if (_zones.Count == 0)
        {
            AddZone("Основна зона", 0, updatedAt);
        }

        RecalculateTotals(updatedAt);
    }

    public void AddWorkItem(
        string name,
        Quantity quantity,
        MeasurementUnit measurementUnit,
        Money unitPrice,
        string? notes,
        DateTimeOffset createdAt,
        string? knowledgeItemId = null,
        Guid? zoneId = null,
        Guid? sourcePriceId = null,
        DateTimeOffset? priceCapturedAt = null,
        bool isUnitPriceManuallyOverridden = false,
        LocalizedNameSnapshot? nameSnapshot = null,
        EstimateItemNameSource? nameSource = null)
    {
        EnsureActive();
        EnsureCurrency(unitPrice);
        var targetZoneId = EnsureZone(zoneId, createdAt);

        _workItems.Add(new EstimateWorkItem(
            Guid.NewGuid(),
            Id,
            targetZoneId,
            name,
            quantity,
            measurementUnit,
            unitPrice,
            notes,
            createdAt,
            knowledgeItemId,
            nameSnapshot,
            ResolveNameSource(knowledgeItemId, nameSnapshot, nameSource),
            sourcePriceId,
            priceCapturedAt,
            isUnitPriceManuallyOverridden));

        RecalculateTotals(createdAt);
    }

    public void AddMaterialItem(
        string name,
        Quantity quantity,
        MeasurementUnit measurementUnit,
        Money unitPrice,
        string? notes,
        DateTimeOffset createdAt,
        string? knowledgeItemId = null,
        Guid? zoneId = null,
        Guid? sourcePriceId = null,
        DateTimeOffset? priceCapturedAt = null,
        bool isUnitPriceManuallyOverridden = false,
        LocalizedNameSnapshot? nameSnapshot = null,
        EstimateItemNameSource? nameSource = null)
    {
        EnsureActive();
        EnsureCurrency(unitPrice);
        var targetZoneId = EnsureZone(zoneId, createdAt);

        _materialItems.Add(new EstimateMaterialItem(
            Guid.NewGuid(),
            Id,
            targetZoneId,
            name,
            quantity,
            measurementUnit,
            unitPrice,
            notes,
            createdAt,
            knowledgeItemId,
            nameSnapshot,
            ResolveNameSource(knowledgeItemId, nameSnapshot, nameSource),
            sourcePriceId,
            priceCapturedAt,
            isUnitPriceManuallyOverridden));

        RecalculateTotals(createdAt);
    }

    public void DuplicateWorkItem(Guid itemId, DateTimeOffset createdAt)
    {
        EnsureActive();
        var item = _workItems.SingleOrDefault(workItem => workItem.Id == itemId)
            ?? throw new InvalidOperationException($"Work item '{itemId}' was not found in this estimate.");

        GetZone(item.ZoneId);
        _workItems.Add(item.Duplicate(Guid.NewGuid(), createdAt));
        RecalculateTotals(createdAt);
    }

    public void DuplicateMaterialItem(Guid itemId, DateTimeOffset createdAt)
    {
        EnsureActive();
        var item = _materialItems.SingleOrDefault(materialItem => materialItem.Id == itemId)
            ?? throw new InvalidOperationException($"Material item '{itemId}' was not found in this estimate.");

        GetZone(item.ZoneId);
        _materialItems.Add(item.Duplicate(Guid.NewGuid(), createdAt));
        RecalculateTotals(createdAt);
    }

    /// <summary>
    /// Changes the quantity, unit price, or notes of an existing work line.
    /// The catalog snapshot (name, unit, and knowledge reference) is deliberately immutable.
    /// </summary>
    public void UpdateWorkItem(
        Guid itemId,
        Quantity quantity,
        Money unitPrice,
        string? notes,
        DateTimeOffset updatedAt)
    {
        EnsureActive();
        EnsureCurrency(unitPrice);

        var item = _workItems.SingleOrDefault(workItem => workItem.Id == itemId)
            ?? throw new InvalidOperationException($"Work item '{itemId}' was not found in this estimate.");

        item.Update(quantity, unitPrice, notes, updatedAt);
        RecalculateTotals(updatedAt);
    }

    /// <summary>
    /// Changes the quantity, unit price, or notes of an existing material line.
    /// The catalog snapshot (name, unit, and knowledge reference) is deliberately immutable.
    /// </summary>
    public void UpdateMaterialItem(
        Guid itemId,
        Quantity quantity,
        Money unitPrice,
        string? notes,
        DateTimeOffset updatedAt)
    {
        EnsureActive();
        EnsureCurrency(unitPrice);

        var item = _materialItems.SingleOrDefault(materialItem => materialItem.Id == itemId)
            ?? throw new InvalidOperationException($"Material item '{itemId}' was not found in this estimate.");

        item.Update(quantity, unitPrice, notes, updatedAt);
        RecalculateTotals(updatedAt);
    }

    /// <summary>
    /// Removes an existing work line and recalculates the aggregate totals.
    /// </summary>
    public void RemoveWorkItem(Guid itemId, DateTimeOffset updatedAt)
    {
        EnsureActive();

        var item = _workItems.SingleOrDefault(workItem => workItem.Id == itemId)
            ?? throw new InvalidOperationException($"Work item '{itemId}' was not found in this estimate.");

        _workItems.Remove(item);
        RecalculateTotals(updatedAt);
    }

    /// <summary>
    /// Removes an existing material line and recalculates the aggregate totals.
    /// </summary>
    public void RemoveMaterialItem(Guid itemId, DateTimeOffset updatedAt)
    {
        EnsureActive();

        var item = _materialItems.SingleOrDefault(materialItem => materialItem.Id == itemId)
            ?? throw new InvalidOperationException($"Material item '{itemId}' was not found in this estimate.");

        _materialItems.Remove(item);
        RecalculateTotals(updatedAt);
    }

    public void Delete(DateTimeOffset deletedAt)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedAt = deletedAt;
        UpdatedAt = deletedAt;
        IncrementVersion();
    }

    private Guid EnsureZone(Guid? zoneId, DateTimeOffset createdAt)
    {
        if (_zones.Count == 0)
        {
            return AddZone("Основна зона", 0, createdAt);
        }

        if (zoneId is null)
        {
            return _zones.OrderBy(zone => zone.SortOrder).First().Id;
        }

        return GetZone(zoneId.Value).Id;
    }

    private EstimateZone GetZone(Guid zoneId) =>
        _zones.SingleOrDefault(zone => zone.Id == zoneId)
        ?? throw new InvalidOperationException($"Zone '{zoneId}' was not found in this estimate.");

    private void AddInitialZone(string name, int sortOrder, DateTimeOffset createdAt)
    {
        var zone = new EstimateZone(Guid.NewGuid(), Id, name, sortOrder, createdAt);
        if (_zones.Any(existing => string.Equals(existing.Name, zone.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Zone '{zone.Name}' already exists in this estimate.");
        }

        _zones.Add(zone);
    }

    private void RecalculateTotals(DateTimeOffset updatedAt)
    {
        TotalLaborAmount = _workItems.Sum(item => item.Total.Amount);
        TotalMaterialsAmount = _materialItems.Sum(item => item.Total.Amount);
        GrandTotalAmount = TotalLaborAmount + TotalMaterialsAmount;
        Touch(updatedAt);
    }

    private void Touch(DateTimeOffset updatedAt)
    {
        UpdatedAt = updatedAt;
        IncrementVersion();
    }

    private void EnsureActive()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("A deleted estimate cannot be changed.");
        }
    }

    private void EnsureCurrency(Money unitPrice)
    {
        ArgumentNullException.ThrowIfNull(unitPrice);

        if (!string.Equals(unitPrice.Currency, Currency, StringComparison.Ordinal))
        {
            throw new ArgumentException("All estimate items must use the estimate currency.", nameof(unitPrice));
        }
    }

    private void IncrementVersion()
    {
        checked
        {
            Version++;
        }
    }

    private static string NormalizeCurrency(string currency) => new Money(decimal.Zero, currency).Currency;

    private static EstimateItemNameSource ResolveNameSource(
        string? knowledgeItemId,
        LocalizedNameSnapshot? nameSnapshot,
        EstimateItemNameSource? nameSource)
    {
        if (nameSource is { } explicitSource)
        {
            return explicitSource;
        }

        if (nameSnapshot is not null)
        {
            return EstimateItemNameSource.KnowledgeSnapshot;
        }

        return string.IsNullOrWhiteSpace(knowledgeItemId)
            ? EstimateItemNameSource.Custom
            : EstimateItemNameSource.Legacy;
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
            throw new ArgumentOutOfRangeException(nameof(notes), "Estimate notes cannot exceed 2000 characters.");
        }

        return normalized;
    }
}
