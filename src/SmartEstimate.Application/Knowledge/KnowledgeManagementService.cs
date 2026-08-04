using FluentValidation;
using SmartEstimate.Application.Knowledge.Abstractions;
using SmartEstimate.Domain.Knowledge;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Application.Knowledge;

#pragma warning disable IDE0011

public sealed record LocalizedTextInput(string Uk, string? En, string? De);
public sealed record CategoryWriteRequest(LocalizedTextInput Name, string? Description, Guid? ParentCategoryId, KnowledgeStatus Status);
public sealed record ConstructionWorkWriteRequest(LocalizedTextInput Name, string? Description, Guid CategoryId, Guid UnitId, IReadOnlyCollection<string>? Tags, KnowledgeStatus Status);
public sealed record MaterialWriteRequest(LocalizedTextInput Name, string? Description, Guid? CategoryId, Guid UnitId, IReadOnlyCollection<string>? Tags, KnowledgeStatus Status);
public sealed record UnitWriteRequest(string Symbol, LocalizedTextInput Name, KnowledgeStatus Status);

/// <summary>Application workflows for the PostgreSQL-backed Knowledge Studio.</summary>
public sealed class KnowledgeManagementService(
    ICategoryRepository categories,
    IConstructionWorkRepository constructionWorks,
    IMaterialRepository materials,
    IUnitRepository units,
    IValidator<CategoryWriteRequest> categoryValidator,
    IValidator<ConstructionWorkWriteRequest> constructionWorkValidator,
    IValidator<MaterialWriteRequest> materialValidator,
    IValidator<UnitWriteRequest> unitValidator)
{
    public async Task<Result<PagedKnowledgeResponse<KnowledgeCategoryResponse>>> GetCategoriesAsync(KnowledgeListQuery query, CancellationToken cancellationToken)
    {
        var records = await categories.ListAsync(query, cancellationToken);
        var count = await categories.CountAsync(query, cancellationToken);
        return Result<PagedKnowledgeResponse<KnowledgeCategoryResponse>>.Success(new(records.Select(Map).ToArray(), query.Page, query.PageSize, count));
    }

    public async Task<Result<PagedKnowledgeResponse<ConstructionWorkResponse>>> GetConstructionWorksAsync(KnowledgeListQuery query, CancellationToken cancellationToken)
    {
        var records = await constructionWorks.ListAsync(query, cancellationToken);
        var count = await constructionWorks.CountAsync(query, cancellationToken);
        return Result<PagedKnowledgeResponse<ConstructionWorkResponse>>.Success(new(records.Select(Map).ToArray(), query.Page, query.PageSize, count));
    }

    public async Task<Result<PagedKnowledgeResponse<KnowledgeMaterialResponse>>> GetMaterialsAsync(KnowledgeListQuery query, CancellationToken cancellationToken)
    {
        var records = await materials.ListAsync(query, cancellationToken);
        var count = await materials.CountAsync(query, cancellationToken);
        return Result<PagedKnowledgeResponse<KnowledgeMaterialResponse>>.Success(new(records.Select(Map).ToArray(), query.Page, query.PageSize, count));
    }

    public async Task<Result<PagedKnowledgeResponse<KnowledgeUnitResponse>>> GetUnitsAsync(KnowledgeListQuery query, CancellationToken cancellationToken)
    {
        var records = await units.ListAsync(query, cancellationToken);
        var count = await units.CountAsync(query, cancellationToken);
        return Result<PagedKnowledgeResponse<KnowledgeUnitResponse>>.Success(new(records.Select(Map).ToArray(), query.Page, query.PageSize, count));
    }

    public async Task<Result<KnowledgeCategoryResponse>> GetCategoryAsync(Guid id, CancellationToken cancellationToken) =>
        ToResult(await categories.GetByIdAsync(id, cancellationToken), id, Map);

    public async Task<Result<ConstructionWorkResponse>> GetConstructionWorkAsync(Guid id, CancellationToken cancellationToken) =>
        ToResult(await constructionWorks.GetByIdAsync(id, cancellationToken), id, Map);

    public async Task<Result<KnowledgeMaterialResponse>> GetMaterialAsync(Guid id, CancellationToken cancellationToken) =>
        ToResult(await materials.GetByIdAsync(id, cancellationToken), id, Map);

    public async Task<Result<KnowledgeUnitResponse>> GetUnitAsync(Guid id, CancellationToken cancellationToken) =>
        ToResult(await units.GetByIdAsync(id, cancellationToken), id, Map);

    public async Task<Result<KnowledgeCategoryResponse>> CreateCategoryAsync(CategoryWriteRequest request, CancellationToken cancellationToken)
    {
        var error = await ValidateAsync(request, categoryValidator, cancellationToken);
        if (error is not null) return Result<KnowledgeCategoryResponse>.Failure(error);
        if (await categories.ExistsWithNameAsync(request.Name.Uk, null, cancellationToken)) return Duplicate<KnowledgeCategoryResponse>("category name");
        if (request.ParentCategoryId is { } parentId && await categories.GetByIdAsync(parentId, cancellationToken) is null) return InvalidReference<KnowledgeCategoryResponse>("parent category");
        var category = KnowledgeCategory.Create(Guid.NewGuid(), ToLocalizedText(request.Name), request.Description, request.ParentCategoryId, request.Status, DateTimeOffset.UtcNow, null);
        await categories.AddAsync(category, cancellationToken);
        await categories.SaveChangesAsync(cancellationToken);
        return Result<KnowledgeCategoryResponse>.Success(Map(category));
    }

    public async Task<Result<KnowledgeCategoryResponse>> UpdateCategoryAsync(Guid id, CategoryWriteRequest request, CancellationToken cancellationToken)
    {
        var category = await categories.GetByIdAsync(id, cancellationToken);
        if (category is null) return NotFound<KnowledgeCategoryResponse>(id);
        var error = await ValidateAsync(request, categoryValidator, cancellationToken);
        if (error is not null) return Result<KnowledgeCategoryResponse>.Failure(error);
        if (await categories.ExistsWithNameAsync(request.Name.Uk, id, cancellationToken)) return Duplicate<KnowledgeCategoryResponse>("category name");
        if (request.ParentCategoryId is { } parentId && await categories.GetByIdAsync(parentId, cancellationToken) is null) return InvalidReference<KnowledgeCategoryResponse>("parent category");
        category.Update(ToLocalizedText(request.Name), request.Description, request.ParentCategoryId, request.Status, DateTimeOffset.UtcNow, null);
        await categories.SaveChangesAsync(cancellationToken);
        return Result<KnowledgeCategoryResponse>.Success(Map(category));
    }

    public async Task<Result<ConstructionWorkResponse>> CreateConstructionWorkAsync(ConstructionWorkWriteRequest request, CancellationToken cancellationToken)
    {
        var error = await ValidateAsync(request, constructionWorkValidator, cancellationToken);
        if (error is not null) return Result<ConstructionWorkResponse>.Failure(error);
        if (await constructionWorks.ExistsWithNameAsync(request.Name.Uk, null, cancellationToken)) return Duplicate<ConstructionWorkResponse>("construction work name");
        if (await categories.GetByIdAsync(request.CategoryId, cancellationToken) is null) return InvalidReference<ConstructionWorkResponse>("category");
        if (await units.GetByIdAsync(request.UnitId, cancellationToken) is null) return InvalidReference<ConstructionWorkResponse>("unit");
        var work = ConstructionWork.Create(Guid.NewGuid(), ToLocalizedText(request.Name), request.Description, request.CategoryId, request.UnitId, request.Tags, request.Status, DateTimeOffset.UtcNow, null);
        await constructionWorks.AddAsync(work, cancellationToken);
        await constructionWorks.SaveChangesAsync(cancellationToken);
        return Result<ConstructionWorkResponse>.Success(Map(work));
    }

    public async Task<Result<ConstructionWorkResponse>> UpdateConstructionWorkAsync(Guid id, ConstructionWorkWriteRequest request, CancellationToken cancellationToken)
    {
        var work = await constructionWorks.GetByIdAsync(id, cancellationToken);
        if (work is null) return NotFound<ConstructionWorkResponse>(id);
        var error = await ValidateAsync(request, constructionWorkValidator, cancellationToken);
        if (error is not null) return Result<ConstructionWorkResponse>.Failure(error);
        if (await constructionWorks.ExistsWithNameAsync(request.Name.Uk, id, cancellationToken)) return Duplicate<ConstructionWorkResponse>("construction work name");
        if (await categories.GetByIdAsync(request.CategoryId, cancellationToken) is null) return InvalidReference<ConstructionWorkResponse>("category");
        if (await units.GetByIdAsync(request.UnitId, cancellationToken) is null) return InvalidReference<ConstructionWorkResponse>("unit");
        work.Update(ToLocalizedText(request.Name), request.Description, request.CategoryId, request.UnitId, request.Tags, request.Status, DateTimeOffset.UtcNow, null);
        await constructionWorks.SaveChangesAsync(cancellationToken);
        return Result<ConstructionWorkResponse>.Success(Map(work));
    }

    public async Task<Result<KnowledgeMaterialResponse>> CreateMaterialAsync(MaterialWriteRequest request, CancellationToken cancellationToken)
    {
        var error = await ValidateAsync(request, materialValidator, cancellationToken);
        if (error is not null) return Result<KnowledgeMaterialResponse>.Failure(error);
        if (await materials.ExistsWithNameAsync(request.Name.Uk, null, cancellationToken)) return Duplicate<KnowledgeMaterialResponse>("material name");
        if (request.CategoryId is { } categoryId && await categories.GetByIdAsync(categoryId, cancellationToken) is null) return InvalidReference<KnowledgeMaterialResponse>("category");
        if (await units.GetByIdAsync(request.UnitId, cancellationToken) is null) return InvalidReference<KnowledgeMaterialResponse>("unit");
        var material = KnowledgeMaterial.Create(Guid.NewGuid(), ToLocalizedText(request.Name), request.Description, request.CategoryId, request.UnitId, request.Tags, request.Status, DateTimeOffset.UtcNow, null);
        await materials.AddAsync(material, cancellationToken);
        await materials.SaveChangesAsync(cancellationToken);
        return Result<KnowledgeMaterialResponse>.Success(Map(material));
    }

    public async Task<Result<KnowledgeMaterialResponse>> UpdateMaterialAsync(Guid id, MaterialWriteRequest request, CancellationToken cancellationToken)
    {
        var material = await materials.GetByIdAsync(id, cancellationToken);
        if (material is null) return NotFound<KnowledgeMaterialResponse>(id);
        var error = await ValidateAsync(request, materialValidator, cancellationToken);
        if (error is not null) return Result<KnowledgeMaterialResponse>.Failure(error);
        if (await materials.ExistsWithNameAsync(request.Name.Uk, id, cancellationToken)) return Duplicate<KnowledgeMaterialResponse>("material name");
        if (request.CategoryId is { } categoryId && await categories.GetByIdAsync(categoryId, cancellationToken) is null) return InvalidReference<KnowledgeMaterialResponse>("category");
        if (await units.GetByIdAsync(request.UnitId, cancellationToken) is null) return InvalidReference<KnowledgeMaterialResponse>("unit");
        material.Update(ToLocalizedText(request.Name), request.Description, request.CategoryId, request.UnitId, request.Tags, request.Status, DateTimeOffset.UtcNow, null);
        await materials.SaveChangesAsync(cancellationToken);
        return Result<KnowledgeMaterialResponse>.Success(Map(material));
    }

    public async Task<Result<KnowledgeUnitResponse>> CreateUnitAsync(UnitWriteRequest request, CancellationToken cancellationToken)
    {
        var error = await ValidateAsync(request, unitValidator, cancellationToken);
        if (error is not null) return Result<KnowledgeUnitResponse>.Failure(error);
        if (await units.ExistsWithNameAsync(request.Name.Uk, null, cancellationToken) || await units.ExistsWithSymbolAsync(request.Symbol, null, cancellationToken)) return Duplicate<KnowledgeUnitResponse>("unit name or symbol");
        var unit = MeasurementUnit.Create(Guid.NewGuid(), request.Symbol, ToLocalizedText(request.Name), request.Status, DateTimeOffset.UtcNow, null);
        await units.AddAsync(unit, cancellationToken);
        await units.SaveChangesAsync(cancellationToken);
        return Result<KnowledgeUnitResponse>.Success(Map(unit));
    }

    public async Task<Result<KnowledgeUnitResponse>> UpdateUnitAsync(Guid id, UnitWriteRequest request, CancellationToken cancellationToken)
    {
        var unit = await units.GetByIdAsync(id, cancellationToken);
        if (unit is null) return NotFound<KnowledgeUnitResponse>(id);
        var error = await ValidateAsync(request, unitValidator, cancellationToken);
        if (error is not null) return Result<KnowledgeUnitResponse>.Failure(error);
        if (await units.ExistsWithNameAsync(request.Name.Uk, id, cancellationToken) || await units.ExistsWithSymbolAsync(request.Symbol, id, cancellationToken)) return Duplicate<KnowledgeUnitResponse>("unit name or symbol");
        unit.Update(request.Symbol, ToLocalizedText(request.Name), request.Status, DateTimeOffset.UtcNow, null);
        await units.SaveChangesAsync(cancellationToken);
        return Result<KnowledgeUnitResponse>.Success(Map(unit));
    }

    public async Task<Result> ArchiveCategoryAsync(Guid id, CancellationToken cancellationToken) => await ArchiveAsync(await categories.GetByIdAsync(id, cancellationToken), await categories.IsReferencedAsync(id, cancellationToken), categories.SaveChangesAsync, id, cancellationToken);
    public async Task<Result> ArchiveConstructionWorkAsync(Guid id, CancellationToken cancellationToken) => await ArchiveAsync(await constructionWorks.GetByIdAsync(id, cancellationToken), await constructionWorks.IsReferencedAsync(id, cancellationToken), constructionWorks.SaveChangesAsync, id, cancellationToken);
    public async Task<Result> ArchiveMaterialAsync(Guid id, CancellationToken cancellationToken) => await ArchiveAsync(await materials.GetByIdAsync(id, cancellationToken), await materials.IsReferencedAsync(id, cancellationToken), materials.SaveChangesAsync, id, cancellationToken);
    public async Task<Result> ArchiveUnitAsync(Guid id, CancellationToken cancellationToken) => await ArchiveAsync(await units.GetByIdAsync(id, cancellationToken), await units.IsReferencedAsync(id, cancellationToken), units.SaveChangesAsync, id, cancellationToken);

    private static async Task<Result> ArchiveAsync(KnowledgeRecord? record, bool isReferenced, Func<CancellationToken, Task> save, Guid id, CancellationToken cancellationToken)
    {
        if (record is null) return Result.Failure(new Error("KnowledgeNotFound", $"Knowledge record '{id}' was not found."));
        if (isReferenced) return Result.Failure(new Error("KnowledgeInUse", "The knowledge record is referenced and cannot be archived."));
        record.ChangeStatus(KnowledgeStatus.Archived, DateTimeOffset.UtcNow, null);
        await save(cancellationToken);
        return Result.Success();
    }

    private static async Task<Error?> ValidateAsync<T>(T request, IValidator<T> validator, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        return validation.IsValid ? null : new Error("ValidationError", string.Join(" ", validation.Errors.Select(error => error.ErrorMessage)));
    }

    private static LocalizedText ToLocalizedText(LocalizedTextInput input) => new(input.Uk, input.En, input.De);
    private static Result<TResponse> ToResult<TRecord, TResponse>(TRecord? record, Guid id, Func<TRecord, TResponse> map) where TRecord : class => record is null ? NotFound<TResponse>(id) : Result<TResponse>.Success(map(record));
    private static Result<T> NotFound<T>(Guid id) => Result<T>.Failure(new Error("KnowledgeNotFound", $"Knowledge record '{id}' was not found."));
    private static Result<T> Duplicate<T>(string property) => Result<T>.Failure(new Error("KnowledgeDuplicate", $"A record with this {property} already exists."));
    private static Result<T> InvalidReference<T>(string property) => Result<T>.Failure(new Error("KnowledgeInvalidReference", $"The referenced {property} does not exist."));
    private static LocalizedTextResponse Map(LocalizedText name) => new(name.En, name.Uk, name.De);
    private static KnowledgeCategoryResponse Map(KnowledgeCategory value) => new(value.Id, Map(value.Name), value.Description, value.ParentCategoryId, value.Version, value.Status, value.CreatedAt, value.UpdatedAt, value.CreatedBy, value.UpdatedBy);
    private static ConstructionWorkResponse Map(ConstructionWork value) => new(value.Id, Map(value.Name), value.Description, value.CategoryId, value.UnitId, value.TagValues, value.Version, value.Status, value.CreatedAt, value.UpdatedAt, value.CreatedBy, value.UpdatedBy);
    private static KnowledgeMaterialResponse Map(KnowledgeMaterial value) => new(value.Id, Map(value.Name), value.Description, value.CategoryId, value.UnitId, value.TagValues, value.Version, value.Status, value.CreatedAt, value.UpdatedAt, value.CreatedBy, value.UpdatedBy);
    private static KnowledgeUnitResponse Map(MeasurementUnit value) => new(value.Id, value.Symbol, Map(value.Name), value.Version, value.Status, value.CreatedAt, value.UpdatedAt, value.CreatedBy, value.UpdatedBy);
}

#pragma warning restore IDE0011
