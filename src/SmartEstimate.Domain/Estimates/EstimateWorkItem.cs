using SmartEstimate.Domain.Estimates.ValueObjects;

namespace SmartEstimate.Domain.Estimates;

/// <summary>
/// A construction work line owned by an <see cref="Estimate"/> aggregate.
/// </summary>
public sealed class EstimateWorkItem
{
    private EstimateWorkItem()
    {
    }

    internal EstimateWorkItem(
        Guid id,
        Guid estimateId,
        Guid zoneId,
        string name,
        Quantity quantity,
        MeasurementUnit measurementUnit,
        Money unitPrice,
        string? notes,
        DateTimeOffset createdAt,
        string? knowledgeItemId)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Work item identifier is required.", nameof(id));
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
    /// Stable identifier of the construction-work record in the PostgreSQL Knowledge store.
    /// It is optional only for legacy manually-created Sprint 1 lines.
    /// </summary>
    public string? KnowledgeItemId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public Money Total => new(
        decimal.Round(Quantity.Value * UnitPrice.Amount, 2, MidpointRounding.AwayFromZero),
        UnitPrice.Currency);

    internal void Update(Quantity quantity, Money unitPrice, string? notes, DateTimeOffset updatedAt)
    {
        Quantity = quantity ?? throw new ArgumentNullException(nameof(quantity));
        UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
        Notes = NormalizeNotes(notes);
        UpdatedAt = updatedAt;
    }

    internal EstimateWorkItem Duplicate(Guid id, DateTimeOffset createdAt) => new(
        id,
        EstimateId,
        ZoneId,
        Name,
        Quantity,
        MeasurementUnit,
        UnitPrice,
        Notes,
        createdAt,
        KnowledgeItemId);

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Construction work name is required.", nameof(name));
        }

        var normalized = name.Trim();
        if (normalized.Length > 256)
        {
            throw new ArgumentOutOfRangeException(nameof(name), "Construction work name cannot exceed 256 characters.");
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
            throw new ArgumentOutOfRangeException(nameof(notes), "Work item notes cannot exceed 2000 characters.");
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
}
