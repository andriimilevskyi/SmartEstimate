namespace SmartEstimate.Domain.Knowledge;

/// <summary>Aggregate root representing a selectable construction material.</summary>
public sealed class KnowledgeMaterial : KnowledgeRecord
{
    private KnowledgeMaterial()
    {
    }

    private KnowledgeMaterial(Guid id, LocalizedText name, string? description, Guid? categoryId, Guid unitId, IEnumerable<string>? tags, KnowledgeStatus status, DateTimeOffset createdAt, Guid? actorId)
        : base(id, status, createdAt, actorId)
    {
        Name = name;
        Description = NormalizeDescription(description);
        CategoryId = categoryId;
        UnitId = RequiredId(unitId);
        Tags = NormalizeTags(tags);
    }

    public LocalizedText Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid? CategoryId { get; private set; }
    public Guid UnitId { get; private set; }
    public string Tags { get; private set; } = string.Empty;
    public IReadOnlyCollection<string> TagValues => Tags.Length == 0 ? Array.Empty<string>() : Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public static KnowledgeMaterial Create(Guid id, LocalizedText name, string? description, Guid? categoryId, Guid unitId, IEnumerable<string>? tags, KnowledgeStatus status, DateTimeOffset createdAt, Guid? actorId) =>
        new(id, name, description, categoryId, unitId, tags, status, createdAt, actorId);

    public void Update(LocalizedText name, string? description, Guid? categoryId, Guid unitId, IEnumerable<string>? tags, KnowledgeStatus status, DateTimeOffset updatedAt, Guid? actorId)
    {
        Name = name;
        Description = NormalizeDescription(description);
        CategoryId = categoryId;
        UnitId = RequiredId(unitId);
        Tags = NormalizeTags(tags);
        ChangeStatus(status, updatedAt, actorId);
    }

    private static Guid RequiredId(Guid value) => value == Guid.Empty ? throw new ArgumentException("A unit reference is required.") : value;
    private static string? NormalizeDescription(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(value.Trim().Length, 4_000)];
    private static string NormalizeTags(IEnumerable<string>? values) => string.Join(',', (values ?? []).Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Take(20));
}
