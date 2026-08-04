using SmartEstimate.Domain.Knowledge;

namespace SmartEstimate.Application.Knowledge;

public sealed record LocalizedTextResponse(string En, string Uk, string De);

public abstract record KnowledgeRecordResponse(
    Guid Id,
    int Version,
    KnowledgeStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Guid? CreatedBy,
    Guid? UpdatedBy);

public sealed record KnowledgeCategoryResponse(
    Guid Id,
    LocalizedTextResponse Name,
    string? Description,
    Guid? ParentCategoryId,
    int Version,
    KnowledgeStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Guid? CreatedBy,
    Guid? UpdatedBy) : KnowledgeRecordResponse(Id, Version, Status, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy);

public sealed record ConstructionWorkResponse(
    Guid Id,
    LocalizedTextResponse Name,
    string? Description,
    Guid CategoryId,
    Guid UnitId,
    IReadOnlyCollection<string> Tags,
    int Version,
    KnowledgeStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Guid? CreatedBy,
    Guid? UpdatedBy) : KnowledgeRecordResponse(Id, Version, Status, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy);

public sealed record KnowledgeMaterialResponse(
    Guid Id,
    LocalizedTextResponse Name,
    string? Description,
    Guid? CategoryId,
    Guid UnitId,
    IReadOnlyCollection<string> Tags,
    int Version,
    KnowledgeStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Guid? CreatedBy,
    Guid? UpdatedBy) : KnowledgeRecordResponse(Id, Version, Status, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy);

public sealed record KnowledgeUnitResponse(
    Guid Id,
    string Symbol,
    LocalizedTextResponse Name,
    int Version,
    KnowledgeStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Guid? CreatedBy,
    Guid? UpdatedBy) : KnowledgeRecordResponse(Id, Version, Status, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy);

public sealed record PagedKnowledgeResponse<TItem>(IReadOnlyCollection<TItem> Items, int Page, int PageSize, int TotalCount);
