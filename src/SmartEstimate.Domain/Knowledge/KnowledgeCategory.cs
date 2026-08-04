namespace SmartEstimate.Domain.Knowledge;

/// <summary>Hierarchical category for construction works and materials.</summary>
public sealed class KnowledgeCategory : KnowledgeRecord
{
    private KnowledgeCategory()
    {
    }

    private KnowledgeCategory(Guid id, LocalizedText name, string? description, Guid? parentCategoryId, KnowledgeStatus status, DateTimeOffset createdAt, Guid? actorId)
        : base(id, status, createdAt, actorId)
    {
        Name = name;
        Description = NormalizeDescription(description);
        ParentCategoryId = parentCategoryId;
    }

    public LocalizedText Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public Guid? ParentCategoryId { get; private set; }

    public static KnowledgeCategory Create(Guid id, LocalizedText name, string? description, Guid? parentCategoryId, KnowledgeStatus status, DateTimeOffset createdAt, Guid? actorId) =>
        new(id, name, description, parentCategoryId, status, createdAt, actorId);

    public void Update(LocalizedText name, string? description, Guid? parentCategoryId, KnowledgeStatus status, DateTimeOffset updatedAt, Guid? actorId)
    {
        if (parentCategoryId == Id)
        {
            throw new ArgumentException("A category cannot be its own parent.", nameof(parentCategoryId));
        }

        Name = name;
        Description = NormalizeDescription(description);
        ParentCategoryId = parentCategoryId;
        ChangeStatus(status, updatedAt, actorId);
    }

    private static string? NormalizeDescription(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        var normalized = value.Trim();
        if (normalized.Length > 4_000)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }
        return normalized;
    }
}
