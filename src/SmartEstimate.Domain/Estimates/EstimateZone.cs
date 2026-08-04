namespace SmartEstimate.Domain.Estimates;

/// <summary>
/// A user-editable zone inside an Estimate, such as kitchen, bedroom, bathroom, or office area.
/// </summary>
public sealed class EstimateZone
{
    private EstimateZone()
    {
    }

    internal EstimateZone(Guid id, Guid estimateId, string name, int sortOrder, DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Zone identifier is required.", nameof(id));
        }

        if (estimateId == Guid.Empty)
        {
            throw new ArgumentException("Estimate identifier is required.", nameof(estimateId));
        }

        Id = id;
        EstimateId = estimateId;
        Name = NormalizeName(name);
        SortOrder = NormalizeSortOrder(sortOrder);
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public Guid EstimateId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    internal void Rename(string name, DateTimeOffset updatedAt)
    {
        Name = NormalizeName(name);
        UpdatedAt = updatedAt;
    }

    internal void ChangeSortOrder(int sortOrder, DateTimeOffset updatedAt)
    {
        SortOrder = NormalizeSortOrder(sortOrder);
        UpdatedAt = updatedAt;
    }

    private static int NormalizeSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortOrder), "Zone sort order cannot be negative.");
        }

        return sortOrder;
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Zone name is required.", nameof(name));
        }

        var normalized = name.Trim();
        if (normalized.Length > 128)
        {
            throw new ArgumentOutOfRangeException(nameof(name), "Zone name cannot exceed 128 characters.");
        }

        return normalized;
    }
}
