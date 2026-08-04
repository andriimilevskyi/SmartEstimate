using SmartEstimate.Domain.Knowledge;

namespace SmartEstimate.Application.Knowledge.Abstractions;

/// <summary>Common filter for Knowledge Studio collections.</summary>
public sealed record KnowledgeListQuery(
    int Page,
    int PageSize,
    string? Search = null,
    string? Sort = null,
    KnowledgeStatus? Status = null,
    Guid? CategoryId = null,
    bool ActiveOnly = false);

public interface ICategoryRepository
{
    Task<KnowledgeCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<KnowledgeCategory>> ListAsync(KnowledgeListQuery query, CancellationToken cancellationToken);
    Task<int> CountAsync(KnowledgeListQuery query, CancellationToken cancellationToken);
    Task<bool> ExistsWithNameAsync(string ukrainianName, Guid? excludingId, CancellationToken cancellationToken);
    Task<bool> IsReferencedAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(KnowledgeCategory category, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IConstructionWorkRepository
{
    Task<ConstructionWork?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ConstructionWork>> ListAsync(KnowledgeListQuery query, CancellationToken cancellationToken);
    Task<int> CountAsync(KnowledgeListQuery query, CancellationToken cancellationToken);
    Task<bool> ExistsWithNameAsync(string ukrainianName, Guid? excludingId, CancellationToken cancellationToken);
    Task<bool> IsReferencedAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(ConstructionWork work, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IMaterialRepository
{
    Task<KnowledgeMaterial?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<KnowledgeMaterial>> ListAsync(KnowledgeListQuery query, CancellationToken cancellationToken);
    Task<int> CountAsync(KnowledgeListQuery query, CancellationToken cancellationToken);
    Task<bool> ExistsWithNameAsync(string ukrainianName, Guid? excludingId, CancellationToken cancellationToken);
    Task<bool> IsReferencedAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(KnowledgeMaterial material, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IUnitRepository
{
    Task<MeasurementUnit?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<MeasurementUnit>> ListAsync(KnowledgeListQuery query, CancellationToken cancellationToken);
    Task<int> CountAsync(KnowledgeListQuery query, CancellationToken cancellationToken);
    Task<bool> ExistsWithNameAsync(string ukrainianName, Guid? excludingId, CancellationToken cancellationToken);
    Task<bool> ExistsWithSymbolAsync(string symbol, Guid? excludingId, CancellationToken cancellationToken);
    Task<bool> IsReferencedAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(MeasurementUnit unit, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Marker abstraction for future portable Knowledge import. Implementations must
/// validate input and write through the repositories above.
/// </summary>
public interface IKnowledgeImportService
{
}

/// <summary>
/// Marker abstraction for future portable Knowledge export. Implementations read
/// PostgreSQL through repositories and never make YAML a runtime source of truth.
/// </summary>
public interface IKnowledgeExportService
{
}
