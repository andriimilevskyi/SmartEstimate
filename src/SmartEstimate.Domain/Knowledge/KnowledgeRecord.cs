namespace SmartEstimate.Domain.Knowledge;

/// <summary>Auditable, versioned base class for a knowledge aggregate.</summary>
public abstract class KnowledgeRecord
{
    protected KnowledgeRecord()
    {
    }

    protected KnowledgeRecord(Guid id, KnowledgeStatus status, DateTimeOffset createdAt, Guid? actorId)
    {
        Id = id == Guid.Empty ? throw new ArgumentException("A knowledge identifier is required.", nameof(id)) : id;
        Status = status;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        CreatedBy = actorId;
        UpdatedBy = actorId;
        Version = 1;
    }

    public Guid Id { get; private set; }

    public int Version { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public Guid? CreatedBy { get; private set; }

    public Guid? UpdatedBy { get; private set; }

    public KnowledgeStatus Status { get; private set; }

    public void ChangeStatus(KnowledgeStatus status, DateTimeOffset updatedAt, Guid? actorId)
    {
        Status = status;
        Touch(updatedAt, actorId);
    }

    protected void Touch(DateTimeOffset updatedAt, Guid? actorId)
    {
        UpdatedAt = updatedAt;
        UpdatedBy = actorId;
        checked
        {
            Version++;
        }
    }
}
