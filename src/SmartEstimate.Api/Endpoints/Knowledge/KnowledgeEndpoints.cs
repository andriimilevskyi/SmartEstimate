using System.Diagnostics;
using SmartEstimate.Application.Knowledge;
using SmartEstimate.Application.Knowledge.Abstractions;
using SmartEstimate.Domain.Knowledge;
using SmartEstimate.Contracts.Common;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Api.Endpoints.Knowledge;

/// <summary>Versioned PostgreSQL-backed Knowledge Studio API.</summary>
public static class KnowledgeEndpoints
{
    public static IEndpointRouteBuilder MapKnowledgeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/knowledge").WithTags("Knowledge Studio");

        MapCategories(group);
        MapConstructionWorks(group);
        MapMaterials(group);
        MapUnits(group);
        return endpoints;
    }

    private static void MapCategories(RouteGroupBuilder group)
    {
        group.MapGet("/categories", GetCategoriesAsync).WithName("GetKnowledgeCategories").WithSummary("List knowledge categories");
        group.MapGet("/categories/{id:guid}", GetCategoryAsync).WithName("GetKnowledgeCategory");
        group.MapPost("/categories", CreateCategoryAsync).WithName("CreateKnowledgeCategory");
        group.MapPut("/categories/{id:guid}", UpdateCategoryAsync).WithName("UpdateKnowledgeCategory");
        group.MapDelete("/categories/{id:guid}", ArchiveCategoryAsync).WithName("ArchiveKnowledgeCategory");
    }

    private static void MapConstructionWorks(RouteGroupBuilder group)
    {
        group.MapGet("/construction-works", GetConstructionWorksAsync).WithName("GetConstructionWorks").WithSummary("List construction works");
        group.MapGet("/construction-works/{id:guid}", GetConstructionWorkAsync).WithName("GetConstructionWork");
        group.MapPost("/construction-works", CreateConstructionWorkAsync).WithName("CreateConstructionWork");
        group.MapPut("/construction-works/{id:guid}", UpdateConstructionWorkAsync).WithName("UpdateConstructionWork");
        group.MapDelete("/construction-works/{id:guid}", ArchiveConstructionWorkAsync).WithName("ArchiveConstructionWork");
    }

    private static void MapMaterials(RouteGroupBuilder group)
    {
        group.MapGet("/materials", GetMaterialsAsync).WithName("GetKnowledgeMaterials").WithSummary("List knowledge materials");
        group.MapGet("/materials/{id:guid}", GetMaterialAsync).WithName("GetKnowledgeMaterial");
        group.MapPost("/materials", CreateMaterialAsync).WithName("CreateKnowledgeMaterial");
        group.MapPut("/materials/{id:guid}", UpdateMaterialAsync).WithName("UpdateKnowledgeMaterial");
        group.MapDelete("/materials/{id:guid}", ArchiveMaterialAsync).WithName("ArchiveKnowledgeMaterial");
    }

    private static void MapUnits(RouteGroupBuilder group)
    {
        group.MapGet("/units", GetUnitsAsync).WithName("GetKnowledgeUnits").WithSummary("List measurement units");
        group.MapGet("/units/{id:guid}", GetUnitAsync).WithName("GetKnowledgeUnit");
        group.MapPost("/units", CreateUnitAsync).WithName("CreateKnowledgeUnit");
        group.MapPut("/units/{id:guid}", UpdateUnitAsync).WithName("UpdateKnowledgeUnit");
        group.MapDelete("/units/{id:guid}", ArchiveUnitAsync).WithName("ArchiveMeasurementUnit");
    }

    private static Task<IResult> GetCategoriesAsync(KnowledgeManagementService service, HttpContext context, CancellationToken cancellationToken, int page = 1, int pageSize = 100, string? search = null, string? sort = null, KnowledgeStatus? status = null, bool activeOnly = true) =>
        FromResultAsync(service.GetCategoriesAsync(new(page, pageSize, search, sort, status, null, activeOnly), cancellationToken), context);
    private static Task<IResult> GetConstructionWorksAsync(KnowledgeManagementService service, HttpContext context, CancellationToken cancellationToken, int page = 1, int pageSize = 100, string? search = null, string? sort = null, KnowledgeStatus? status = null, Guid? categoryId = null, bool activeOnly = true) =>
        FromResultAsync(service.GetConstructionWorksAsync(new(page, pageSize, search, sort, status, categoryId, activeOnly), cancellationToken), context);
    private static Task<IResult> GetMaterialsAsync(KnowledgeManagementService service, HttpContext context, CancellationToken cancellationToken, int page = 1, int pageSize = 100, string? search = null, string? sort = null, KnowledgeStatus? status = null, Guid? categoryId = null, bool activeOnly = true) =>
        FromResultAsync(service.GetMaterialsAsync(new(page, pageSize, search, sort, status, categoryId, activeOnly), cancellationToken), context);
    private static Task<IResult> GetUnitsAsync(KnowledgeManagementService service, HttpContext context, CancellationToken cancellationToken, int page = 1, int pageSize = 100, string? search = null, string? sort = null, KnowledgeStatus? status = null, bool activeOnly = true) =>
        FromResultAsync(service.GetUnitsAsync(new(page, pageSize, search, sort, status, null, activeOnly), cancellationToken), context);

    private static Task<IResult> GetCategoryAsync(Guid id, KnowledgeManagementService service, HttpContext context, CancellationToken cancellationToken) => FromResultAsync(service.GetCategoryAsync(id, cancellationToken), context);
    private static Task<IResult> GetConstructionWorkAsync(Guid id, KnowledgeManagementService service, HttpContext context, CancellationToken cancellationToken) => FromResultAsync(service.GetConstructionWorkAsync(id, cancellationToken), context);
    private static Task<IResult> GetMaterialAsync(Guid id, KnowledgeManagementService service, HttpContext context, CancellationToken cancellationToken) => FromResultAsync(service.GetMaterialAsync(id, cancellationToken), context);
    private static Task<IResult> GetUnitAsync(Guid id, KnowledgeManagementService service, HttpContext context, CancellationToken cancellationToken) => FromResultAsync(service.GetUnitAsync(id, cancellationToken), context);

    private static async Task<IResult> CreateCategoryAsync(CategoryWriteRequest request, KnowledgeManagementService service, HttpContext context, CancellationToken cancellationToken) => await CreatedAsync(await service.CreateCategoryAsync(request, cancellationToken), "/api/v1/knowledge/categories", context);
    private static async Task<IResult> CreateConstructionWorkAsync(ConstructionWorkWriteRequest request, KnowledgeManagementService service, HttpContext context, CancellationToken cancellationToken) => await CreatedAsync(await service.CreateConstructionWorkAsync(request, cancellationToken), "/api/v1/knowledge/construction-works", context);
    private static async Task<IResult> CreateMaterialAsync(MaterialWriteRequest request, KnowledgeManagementService service, HttpContext context, CancellationToken cancellationToken) => await CreatedAsync(await service.CreateMaterialAsync(request, cancellationToken), "/api/v1/knowledge/materials", context);
    private static async Task<IResult> CreateUnitAsync(UnitWriteRequest request, KnowledgeManagementService service, HttpContext context, CancellationToken cancellationToken) => await CreatedAsync(await service.CreateUnitAsync(request, cancellationToken), "/api/v1/knowledge/units", context);

    private static Task<IResult> UpdateCategoryAsync(Guid id, CategoryWriteRequest request, KnowledgeManagementService service, HttpContext context, CancellationToken cancellationToken) => FromResultAsync(service.UpdateCategoryAsync(id, request, cancellationToken), context);
    private static Task<IResult> UpdateConstructionWorkAsync(Guid id, ConstructionWorkWriteRequest request, KnowledgeManagementService service, HttpContext context, CancellationToken cancellationToken) => FromResultAsync(service.UpdateConstructionWorkAsync(id, request, cancellationToken), context);
    private static Task<IResult> UpdateMaterialAsync(Guid id, MaterialWriteRequest request, KnowledgeManagementService service, HttpContext context, CancellationToken cancellationToken) => FromResultAsync(service.UpdateMaterialAsync(id, request, cancellationToken), context);
    private static Task<IResult> UpdateUnitAsync(Guid id, UnitWriteRequest request, KnowledgeManagementService service, HttpContext context, CancellationToken cancellationToken) => FromResultAsync(service.UpdateUnitAsync(id, request, cancellationToken), context);

    private static Task<IResult> ArchiveCategoryAsync(Guid id, KnowledgeManagementService service, HttpContext context, CancellationToken cancellationToken) => ArchiveAsync(service.ArchiveCategoryAsync(id, cancellationToken), context);
    private static Task<IResult> ArchiveConstructionWorkAsync(Guid id, KnowledgeManagementService service, HttpContext context, CancellationToken cancellationToken) => ArchiveAsync(service.ArchiveConstructionWorkAsync(id, cancellationToken), context);
    private static Task<IResult> ArchiveMaterialAsync(Guid id, KnowledgeManagementService service, HttpContext context, CancellationToken cancellationToken) => ArchiveAsync(service.ArchiveMaterialAsync(id, cancellationToken), context);
    private static Task<IResult> ArchiveUnitAsync(Guid id, KnowledgeManagementService service, HttpContext context, CancellationToken cancellationToken) => ArchiveAsync(service.ArchiveUnitAsync(id, cancellationToken), context);

    private static async Task<IResult> FromResultAsync<T>(Task<Result<T>> task, HttpContext context)
    {
        var result = await task;
        return result.IsSuccess && result.Value is { } value
            ? Results.Ok(ApiResponse<T>.FromData(value))
            : Error(result.Error, context);
    }

    private static Task<IResult> ArchiveAsync(Task<Result> task, HttpContext context) => ArchiveResultAsync(task, context);
    private static async Task<IResult> ArchiveResultAsync(Task<Result> task, HttpContext context)
    {
        var result = await task;
        return result.IsSuccess ? Results.NoContent() : Error(result.Error, context);
    }

    private static Task<IResult> CreatedAsync<T>(Result<T> result, string resourcePath, HttpContext context) where T : KnowledgeRecordResponse
    {
        return Task.FromResult<IResult>(result.IsSuccess && result.Value is { } value
            ? Results.Created($"{resourcePath}/{value.Id}", ApiResponse<T>.FromData(value))
            : Error(result.Error, context));
    }

    private static IResult Error(Error error, HttpContext context) => Results.Json(
        ApiResponse<object>.FromError(new ApiError(error.Code, error.Message, Activity.Current?.Id ?? context.TraceIdentifier)),
        statusCode: error.Code switch
        {
            "KnowledgeNotFound" => StatusCodes.Status404NotFound,
            "KnowledgeDuplicate" or "KnowledgeInUse" => StatusCodes.Status409Conflict,
            "ValidationError" or "KnowledgeInvalidReference" => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status400BadRequest
        });
}
