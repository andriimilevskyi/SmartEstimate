namespace SmartEstimate.Domain.Customers;

/// <summary>
/// Minimal customer card used by the estimating workflow.
/// </summary>
public sealed class Customer
{
    private Customer()
    {
    }

    private Customer(Guid id, string name, string? phone, string? email, string? note, DateTimeOffset createdAt)
    {
        Id = id == Guid.Empty ? throw new ArgumentException("Customer identifier is required.", nameof(id)) : id;
        Name = NormalizeName(name);
        Phone = NormalizeOptional(phone, 64, nameof(phone));
        Email = NormalizeOptional(email, 256, nameof(email));
        Note = NormalizeOptional(note, 2_000, nameof(note));
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        Version = 1;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Phone { get; private set; }

    public string? Email { get; private set; }

    public string? Note { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    public bool IsDeleted { get; private set; }

    public bool IsArchived => IsDeleted;

    public DateTimeOffset? ArchivedAt => DeletedAt;

    public int Version { get; private set; }

    public static Customer Create(Guid id, string name, string? phone, string? email, string? note, DateTimeOffset createdAt) =>
        new(id, name, phone, email, note, createdAt);

    public void Update(string name, string? phone, string? email, string? note, DateTimeOffset updatedAt)
    {
        EnsureActive();
        Name = NormalizeName(name);
        Phone = NormalizeOptional(phone, 64, nameof(phone));
        Email = NormalizeOptional(email, 256, nameof(email));
        Note = NormalizeOptional(note, 2_000, nameof(note));
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
            throw new InvalidOperationException("A deleted customer cannot be changed.");
        }
    }

    private static string NormalizeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Customer name is required.", nameof(value));
        }

        var normalized = value.Trim();
        if (normalized.Length > 256)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Customer name cannot exceed 256 characters.");
        }

        return normalized;
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
