namespace SmartEstimate.Domain.Knowledge;

/// <summary>Aggregate root representing a unit of measurement.</summary>
public sealed class MeasurementUnit : KnowledgeRecord
{
    private MeasurementUnit()
    {
    }

    private MeasurementUnit(Guid id, string symbol, LocalizedText name, KnowledgeStatus status, DateTimeOffset createdAt, Guid? actorId)
        : base(id, status, createdAt, actorId)
    {
        Symbol = NormalizeSymbol(symbol);
        Name = name;
    }

    public string Symbol { get; private set; } = string.Empty;
    public LocalizedText Name { get; private set; } = null!;

    public static MeasurementUnit Create(Guid id, string symbol, LocalizedText name, KnowledgeStatus status, DateTimeOffset createdAt, Guid? actorId) =>
        new(id, symbol, name, status, createdAt, actorId);

    public void Update(string symbol, LocalizedText name, KnowledgeStatus status, DateTimeOffset updatedAt, Guid? actorId)
    {
        Symbol = NormalizeSymbol(symbol);
        Name = name;
        ChangeStatus(status, updatedAt, actorId);
    }

    private static string NormalizeSymbol(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A unit symbol is required.", nameof(value));
        }
        var normalized = value.Trim();
        if (normalized.Length > 16)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }
        return normalized;
    }
}
