namespace SmartEstimate.Domain.Objects;

/// <summary>
/// A construction object that belongs to one customer and can contain multiple estimates.
/// </summary>
public sealed class EstimateObject
{
    private EstimateObject()
    {
    }

    private EstimateObject(
        Guid id,
        Guid customerId,
        string name,
        EstimateObjectType objectType,
        string? address,
        decimal? totalArea,
        string? description,
        DateTimeOffset createdAt)
    {
        Id = id == Guid.Empty ? throw new ArgumentException("Object identifier is required.", nameof(id)) : id;
        CustomerId = customerId == Guid.Empty ? throw new ArgumentException("Customer reference is required.", nameof(customerId)) : customerId;
        Name = NormalizeName(name);
        ObjectType = objectType;
        Address = NormalizeOptional(address, 512, nameof(address));
        TotalArea = NormalizeTotalArea(totalArea);
        Description = NormalizeOptional(description, 2_000, nameof(description));
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        Version = 1;
    }

    public Guid Id { get; private set; }

    public Guid CustomerId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public EstimateObjectType ObjectType { get; private set; }

    public string? Address { get; private set; }

    public decimal? TotalArea { get; private set; }

    public string? Description { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    public bool IsDeleted { get; private set; }

    public bool IsArchived => IsDeleted;

    public DateTimeOffset? ArchivedAt => DeletedAt;

    public int Version { get; private set; }

    public static EstimateObject Create(
        Guid id,
        Guid customerId,
        string name,
        EstimateObjectType objectType,
        string? address,
        decimal? totalArea,
        string? description,
        DateTimeOffset createdAt) =>
        new(id, customerId, name, objectType, address, totalArea, description, createdAt);

    public void Update(
        string name,
        EstimateObjectType objectType,
        string? address,
        decimal? totalArea,
        string? description,
        DateTimeOffset updatedAt)
    {
        EnsureActive();
        Name = NormalizeName(name);
        ObjectType = objectType;
        Address = NormalizeOptional(address, 512, nameof(address));
        TotalArea = NormalizeTotalArea(totalArea);
        Description = NormalizeOptional(description, 2_000, nameof(description));
        Touch(updatedAt);
    }

    public void Archive(DateTimeOffset archivedAt)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedAt = archivedAt;
        Touch(archivedAt);
    }

    public void Restore(DateTimeOffset restoredAt)
    {
        if (!IsDeleted)
        {
            return;
        }

        IsDeleted = false;
        DeletedAt = null;
        Touch(restoredAt);
    }

    public void Delete(DateTimeOffset deletedAt) => Archive(deletedAt);

    private void Touch(DateTimeOffset updatedAt)
    {
        UpdatedAt = updatedAt;
        checked
        {
            Version++;
        }
    }

    private void EnsureActive()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("A deleted object cannot be changed.");
        }
    }

    private static string NormalizeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Object name is required.", nameof(value));
        }

        var normalized = value.Trim();
        if (normalized.Length > 256)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Object name cannot exceed 256 characters.");
        }

        return normalized;
    }

    private static decimal? NormalizeTotalArea(decimal? totalArea)
    {
        if (totalArea is null)
        {
            return null;
        }

        if (totalArea <= decimal.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(totalArea), "Total area must be greater than zero.");
        }

        if (decimal.Round(totalArea.Value, 2, MidpointRounding.AwayFromZero) != totalArea)
        {
            throw new ArgumentOutOfRangeException(nameof(totalArea), "Total area cannot have more than two decimal places.");
        }

        return totalArea;
    }

    private static string? NormalizeOptional(string? value, int maxLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }

        return normalized;
    }
}
