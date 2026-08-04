using Microsoft.EntityFrameworkCore;
using SmartEstimate.Application.Knowledge.Abstractions;
using SmartEstimate.Domain.Knowledge;

namespace SmartEstimate.Infrastructure.Persistence.Repositories;

#pragma warning disable IDE0011

/// <summary>EF Core implementation of all Knowledge aggregate repository contracts.</summary>
public sealed class KnowledgeRepository(SmartEstimateDbContext dbContext) :
    ICategoryRepository,
    IConstructionWorkRepository,
    IMaterialRepository,
    IUnitRepository
{
    async Task<KnowledgeCategory?> ICategoryRepository.GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.KnowledgeCategories.SingleOrDefaultAsync(value => value.Id == id, cancellationToken);

    async Task<IReadOnlyCollection<KnowledgeCategory>> ICategoryRepository.ListAsync(KnowledgeListQuery query, CancellationToken cancellationToken) =>
        await CategoryQuery(query).OrderByCategory(query.Sort).Page(query).ToArrayAsync(cancellationToken);

    Task<int> ICategoryRepository.CountAsync(KnowledgeListQuery query, CancellationToken cancellationToken) => CategoryQuery(query).CountAsync(cancellationToken);

    Task<bool> ICategoryRepository.ExistsWithNameAsync(string ukrainianName, Guid? excludingId, CancellationToken cancellationToken) =>
        dbContext.KnowledgeCategories.AnyAsync(value => EF.Functions.ILike(value.Name.Uk, ukrainianName.Trim()) && (!excludingId.HasValue || value.Id != excludingId), cancellationToken);

    async Task<bool> ICategoryRepository.IsReferencedAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.KnowledgeCategories.AnyAsync(value => value.ParentCategoryId == id && value.Status != KnowledgeStatus.Archived, cancellationToken)
        || await dbContext.ConstructionWorks.AnyAsync(value => value.CategoryId == id && value.Status != KnowledgeStatus.Archived, cancellationToken)
        || await dbContext.KnowledgeMaterials.AnyAsync(value => value.CategoryId == id && value.Status != KnowledgeStatus.Archived, cancellationToken);

    Task ICategoryRepository.AddAsync(KnowledgeCategory category, CancellationToken cancellationToken) => dbContext.KnowledgeCategories.AddAsync(category, cancellationToken).AsTask();
    Task ICategoryRepository.SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);

    async Task<ConstructionWork?> IConstructionWorkRepository.GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.ConstructionWorks.SingleOrDefaultAsync(value => value.Id == id, cancellationToken);

    async Task<IReadOnlyCollection<ConstructionWork>> IConstructionWorkRepository.ListAsync(KnowledgeListQuery query, CancellationToken cancellationToken) =>
        await WorkQuery(query).OrderByWork(query.Sort).Page(query).ToArrayAsync(cancellationToken);

    Task<int> IConstructionWorkRepository.CountAsync(KnowledgeListQuery query, CancellationToken cancellationToken) => WorkQuery(query).CountAsync(cancellationToken);

    Task<bool> IConstructionWorkRepository.ExistsWithNameAsync(string ukrainianName, Guid? excludingId, CancellationToken cancellationToken) =>
        dbContext.ConstructionWorks.AnyAsync(value => EF.Functions.ILike(value.Name.Uk, ukrainianName.Trim()) && (!excludingId.HasValue || value.Id != excludingId), cancellationToken);

    Task<bool> IConstructionWorkRepository.IsReferencedAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(false);
    Task IConstructionWorkRepository.AddAsync(ConstructionWork work, CancellationToken cancellationToken) => dbContext.ConstructionWorks.AddAsync(work, cancellationToken).AsTask();
    Task IConstructionWorkRepository.SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);

    async Task<KnowledgeMaterial?> IMaterialRepository.GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.KnowledgeMaterials.SingleOrDefaultAsync(value => value.Id == id, cancellationToken);

    async Task<IReadOnlyCollection<KnowledgeMaterial>> IMaterialRepository.ListAsync(KnowledgeListQuery query, CancellationToken cancellationToken) =>
        await MaterialQuery(query).OrderByMaterial(query.Sort).Page(query).ToArrayAsync(cancellationToken);

    Task<int> IMaterialRepository.CountAsync(KnowledgeListQuery query, CancellationToken cancellationToken) => MaterialQuery(query).CountAsync(cancellationToken);

    Task<bool> IMaterialRepository.ExistsWithNameAsync(string ukrainianName, Guid? excludingId, CancellationToken cancellationToken) =>
        dbContext.KnowledgeMaterials.AnyAsync(value => EF.Functions.ILike(value.Name.Uk, ukrainianName.Trim()) && (!excludingId.HasValue || value.Id != excludingId), cancellationToken);

    Task<bool> IMaterialRepository.IsReferencedAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(false);
    Task IMaterialRepository.AddAsync(KnowledgeMaterial material, CancellationToken cancellationToken) => dbContext.KnowledgeMaterials.AddAsync(material, cancellationToken).AsTask();
    Task IMaterialRepository.SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);

    async Task<MeasurementUnit?> IUnitRepository.GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.MeasurementUnits.SingleOrDefaultAsync(value => value.Id == id, cancellationToken);

    async Task<IReadOnlyCollection<MeasurementUnit>> IUnitRepository.ListAsync(KnowledgeListQuery query, CancellationToken cancellationToken) =>
        await UnitQuery(query).OrderByUnit(query.Sort).Page(query).ToArrayAsync(cancellationToken);

    Task<int> IUnitRepository.CountAsync(KnowledgeListQuery query, CancellationToken cancellationToken) => UnitQuery(query).CountAsync(cancellationToken);

    Task<bool> IUnitRepository.ExistsWithNameAsync(string ukrainianName, Guid? excludingId, CancellationToken cancellationToken) =>
        dbContext.MeasurementUnits.AnyAsync(value => EF.Functions.ILike(value.Name.Uk, ukrainianName.Trim()) && (!excludingId.HasValue || value.Id != excludingId), cancellationToken);

    Task<bool> IUnitRepository.ExistsWithSymbolAsync(string symbol, Guid? excludingId, CancellationToken cancellationToken) =>
        dbContext.MeasurementUnits.AnyAsync(value => EF.Functions.ILike(value.Symbol, symbol.Trim()) && (!excludingId.HasValue || value.Id != excludingId), cancellationToken);

    async Task<bool> IUnitRepository.IsReferencedAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.ConstructionWorks.AnyAsync(value => value.UnitId == id && value.Status != KnowledgeStatus.Archived, cancellationToken)
        || await dbContext.KnowledgeMaterials.AnyAsync(value => value.UnitId == id && value.Status != KnowledgeStatus.Archived, cancellationToken);

    Task IUnitRepository.AddAsync(MeasurementUnit unit, CancellationToken cancellationToken) => dbContext.MeasurementUnits.AddAsync(unit, cancellationToken).AsTask();
    Task IUnitRepository.SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);

    private IQueryable<KnowledgeCategory> CategoryQuery(KnowledgeListQuery query)
    {
        var source = dbContext.KnowledgeCategories.AsQueryable();
        if (query.ActiveOnly) source = source.Where(value => value.Status == KnowledgeStatus.Active);
        else if (query.Status is { } status) source = source.Where(value => value.Status == status);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            source = source.Where(value => EF.Functions.ILike(value.Name.Uk, term) || EF.Functions.ILike(value.Name.En, term) || EF.Functions.ILike(value.Name.De, term));
        }
        return source;
    }

    private IQueryable<ConstructionWork> WorkQuery(KnowledgeListQuery query)
    {
        var source = dbContext.ConstructionWorks.AsQueryable();
        if (query.ActiveOnly) source = source.Where(value => value.Status == KnowledgeStatus.Active);
        else if (query.Status is { } status) source = source.Where(value => value.Status == status);
        if (query.CategoryId is { } categoryId) source = source.Where(value => value.CategoryId == categoryId);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            source = source.Where(value => EF.Functions.ILike(value.Name.Uk, term) || EF.Functions.ILike(value.Name.En, term) || EF.Functions.ILike(value.Name.De, term) || EF.Functions.ILike(value.Tags, term));
        }
        return source;
    }

    private IQueryable<KnowledgeMaterial> MaterialQuery(KnowledgeListQuery query)
    {
        var source = dbContext.KnowledgeMaterials.AsQueryable();
        if (query.ActiveOnly) source = source.Where(value => value.Status == KnowledgeStatus.Active);
        else if (query.Status is { } status) source = source.Where(value => value.Status == status);
        if (query.CategoryId is { } categoryId) source = source.Where(value => value.CategoryId == categoryId);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            source = source.Where(value => EF.Functions.ILike(value.Name.Uk, term) || EF.Functions.ILike(value.Name.En, term) || EF.Functions.ILike(value.Name.De, term) || EF.Functions.ILike(value.Tags, term));
        }
        return source;
    }

    private IQueryable<MeasurementUnit> UnitQuery(KnowledgeListQuery query)
    {
        var source = dbContext.MeasurementUnits.AsQueryable();
        if (query.ActiveOnly) source = source.Where(value => value.Status == KnowledgeStatus.Active);
        else if (query.Status is { } status) source = source.Where(value => value.Status == status);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            source = source.Where(value => EF.Functions.ILike(value.Name.Uk, term) || EF.Functions.ILike(value.Name.En, term) || EF.Functions.ILike(value.Name.De, term) || EF.Functions.ILike(value.Symbol, term));
        }
        return source;
    }
}

#pragma warning restore IDE0011

internal static class KnowledgeRepositoryQueryExtensions
{
    public static IQueryable<TRecord> Page<TRecord>(this IQueryable<TRecord> source, KnowledgeListQuery query) where TRecord : KnowledgeRecord =>
        source.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize);

    public static IQueryable<KnowledgeCategory> OrderByCategory(this IQueryable<KnowledgeCategory> source, string? sort) => sort switch
    {
        "-createdAt" => source.OrderByDescending(value => value.CreatedAt),
        "createdAt" => source.OrderBy(value => value.CreatedAt),
        "-name" => source.OrderByDescending(value => value.Name.Uk),
        _ => source.OrderBy(value => value.Name.Uk)
    };

    public static IQueryable<ConstructionWork> OrderByWork(this IQueryable<ConstructionWork> source, string? sort) => sort switch
    {
        "-createdAt" => source.OrderByDescending(value => value.CreatedAt),
        "createdAt" => source.OrderBy(value => value.CreatedAt),
        "-name" => source.OrderByDescending(value => value.Name.Uk),
        _ => source.OrderBy(value => value.Name.Uk)
    };

    public static IQueryable<KnowledgeMaterial> OrderByMaterial(this IQueryable<KnowledgeMaterial> source, string? sort) => sort switch
    {
        "-createdAt" => source.OrderByDescending(value => value.CreatedAt),
        "createdAt" => source.OrderBy(value => value.CreatedAt),
        "-name" => source.OrderByDescending(value => value.Name.Uk),
        _ => source.OrderBy(value => value.Name.Uk)
    };

    public static IQueryable<MeasurementUnit> OrderByUnit(this IQueryable<MeasurementUnit> source, string? sort) => sort switch
    {
        "-createdAt" => source.OrderByDescending(value => value.CreatedAt),
        "createdAt" => source.OrderBy(value => value.CreatedAt),
        "-name" => source.OrderByDescending(value => value.Name.Uk),
        _ => source.OrderBy(value => value.Name.Uk)
    };
}
