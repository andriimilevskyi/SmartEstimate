using System.Diagnostics;
using SmartEstimate.Application.Estimates;
using SmartEstimate.Application.Estimates.AddEstimateMaterialItem;
using SmartEstimate.Application.Estimates.AddEstimateZone;
using SmartEstimate.Application.Estimates.AddEstimateWorkItem;
using SmartEstimate.Application.Estimates.CreateEstimate;
using SmartEstimate.Application.Estimates.DeleteEstimate;
using SmartEstimate.Application.Estimates.DuplicateEstimateMaterialItem;
using SmartEstimate.Application.Estimates.DuplicateEstimateWorkItem;
using SmartEstimate.Application.Estimates.GetEstimateById;
using SmartEstimate.Application.Estimates.GetEstimates;
using SmartEstimate.Application.Estimates.RemoveEstimateMaterialItem;
using SmartEstimate.Application.Estimates.RemoveEstimateZone;
using SmartEstimate.Application.Estimates.RemoveEstimateWorkItem;
using SmartEstimate.Application.Estimates.ReorderEstimateZones;
using SmartEstimate.Application.Estimates.UpdateEstimateZone;
using SmartEstimate.Application.Estimates.UpdateEstimateMaterialItem;
using SmartEstimate.Application.Estimates.UpdateEstimateWorkItem;
using SmartEstimate.Contracts.Common;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Api.Endpoints.Estimates;

/// <summary>
/// Maps public HTTP endpoints for the Estimate Core vertical slice.
/// </summary>
public static class EstimateEndpoints
{
    /// <summary>
    /// Adds the versioned Estimate REST endpoints to the application pipeline.
    /// </summary>
    public static IEndpointRouteBuilder MapEstimateEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/api/v1/estimates")
            .WithTags("Estimates");

        group.MapPost(string.Empty, CreateAsync)
            .WithName("CreateEstimate")
            .WithSummary("Create an estimate")
            .WithDescription("Creates an estimate and its optional initial work and material lines.")
            .Produces<ApiResponse<EstimateDetailsResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict)
            .Produces<ApiResponse<object>>(StatusCodes.Status422UnprocessableEntity);

        group.MapGet(string.Empty, GetListAsync)
            .WithName("GetEstimates")
            .WithSummary("Get estimates")
            .WithDescription("Returns a page of active estimates.")
            .Produces<ApiResponse<PagedEstimatesResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetEstimateById")
            .WithSummary("Get an estimate")
            .WithDescription("Returns one active estimate including its work and material lines.")
            .Produces<ApiResponse<EstimateDetailsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status422UnprocessableEntity);

        group.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteEstimate")
            .WithSummary("Delete an estimate")
            .WithDescription("Soft-deletes an active estimate.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/zones", AddZoneAsync)
            .WithName("AddEstimateZone")
            .WithSummary("Add an estimate zone")
            .Produces<ApiResponse<EstimateDetailsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status422UnprocessableEntity);

        group.MapPatch("/{id:guid}/zones/{zoneId:guid}", UpdateZoneAsync)
            .WithName("UpdateEstimateZone")
            .WithSummary("Rename an estimate zone")
            .Produces<ApiResponse<EstimateDetailsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/zones/reorder", ReorderZonesAsync)
            .WithName("ReorderEstimateZones")
            .WithSummary("Replace estimate zone order")
            .Produces<ApiResponse<EstimateDetailsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status422UnprocessableEntity);

        group.MapDelete("/{id:guid}/zones/{zoneId:guid}", DeleteZoneAsync)
            .WithName("DeleteEstimateZone")
            .WithSummary("Delete an estimate zone and its line items")
            .Produces<ApiResponse<EstimateDetailsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/work-items", AddWorkItemAsync)
            .WithName("AddEstimateWorkItem")
            .WithSummary("Add a catalog work to an estimate")
            .WithDescription("Copies the selected active Knowledge work name and measurement unit into the estimate.")
            .Produces<ApiResponse<EstimateDetailsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status422UnprocessableEntity);

        group.MapPatch("/{id:guid}/work-items/{itemId:guid}", UpdateWorkItemAsync)
            .WithName("UpdateEstimateWorkItem")
            .WithSummary("Update an estimate work line")
            .WithDescription("Updates quantity, unit price, and notes; aggregate totals are recalculated automatically.")
            .Produces<ApiResponse<EstimateDetailsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status422UnprocessableEntity);

        group.MapDelete("/{id:guid}/work-items/{itemId:guid}", DeleteWorkItemAsync)
            .WithName("DeleteEstimateWorkItem")
            .WithSummary("Delete an estimate work line")
            .WithDescription("Removes a work line and recalculates aggregate totals.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/work-items/{itemId:guid}/duplicate", DuplicateWorkItemAsync)
            .WithName("DuplicateEstimateWorkItem")
            .WithSummary("Duplicate an estimate work line")
            .Produces<ApiResponse<EstimateDetailsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/material-items", AddMaterialItemAsync)
            .WithName("AddEstimateMaterialItem")
            .WithSummary("Add a catalog material to an estimate")
            .WithDescription("Copies the selected active Knowledge material name and measurement unit into the estimate.")
            .Produces<ApiResponse<EstimateDetailsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status422UnprocessableEntity);

        group.MapPatch("/{id:guid}/material-items/{itemId:guid}", UpdateMaterialItemAsync)
            .WithName("UpdateEstimateMaterialItem")
            .WithSummary("Update an estimate material line")
            .WithDescription("Updates quantity, unit price, and notes; aggregate totals are recalculated automatically.")
            .Produces<ApiResponse<EstimateDetailsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status422UnprocessableEntity);

        group.MapDelete("/{id:guid}/material-items/{itemId:guid}", DeleteMaterialItemAsync)
            .WithName("DeleteEstimateMaterialItem")
            .WithSummary("Delete an estimate material line")
            .WithDescription("Removes a material line and recalculates aggregate totals.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/material-items/{itemId:guid}/duplicate", DuplicateMaterialItemAsync)
            .WithName("DuplicateEstimateMaterialItem")
            .WithSummary("Duplicate an estimate material line")
            .Produces<ApiResponse<EstimateDetailsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status422UnprocessableEntity);

        return endpoints;
    }

    private static async Task<IResult> CreateAsync(
        CreateEstimateRequest request,
        CreateEstimateHandler handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await handler.HandleAsync(
            new CreateEstimateCommand(
                request.EstimateNumber,
                request.Currency,
                request.ObjectType,
                request.ObjectAddress,
                request.TotalArea,
                request.Notes,
                request.Zones,
                request.WorkItems?.Select(MapLineItem).ToArray(),
                request.MaterialItems?.Select(MapLineItem).ToArray()),
            cancellationToken);

        return FromResult(
            result,
            httpContext,
            response => Results.Created(
                $"/api/v1/estimates/{response.Id}",
                ApiResponse<EstimateDetailsResponse>.FromData(response)));
    }

    private static async Task<IResult> GetListAsync(
        GetEstimatesHandler handler,
        HttpContext httpContext,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20)
    {
        var result = await handler.HandleAsync(new GetEstimatesQuery(page, pageSize), cancellationToken);

        return FromResult(
            result,
            httpContext,
            response => Results.Ok(ApiResponse<PagedEstimatesResponse>.FromData(response)));
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        GetEstimateByIdHandler handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetEstimateByIdQuery(id), cancellationToken);

        return FromResult(
            result,
            httpContext,
            response => Results.Ok(ApiResponse<EstimateDetailsResponse>.FromData(response)));
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        DeleteEstimateHandler handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new DeleteEstimateCommand(id), cancellationToken);
        return result.IsSuccess
            ? Results.NoContent()
            : ExpectedError(result.Error, httpContext);
    }

    private static async Task<IResult> AddZoneAsync(
        Guid id,
        EstimateZoneRequest request,
        AddEstimateZoneHandler handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new AddEstimateZoneCommand(id, request.Name), cancellationToken);
        return FromResult(result, httpContext, response => Results.Ok(ApiResponse<EstimateDetailsResponse>.FromData(response)));
    }

    private static async Task<IResult> UpdateZoneAsync(
        Guid id,
        Guid zoneId,
        EstimateZoneRequest request,
        UpdateEstimateZoneHandler handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new UpdateEstimateZoneCommand(id, zoneId, request.Name), cancellationToken);
        return FromResult(result, httpContext, response => Results.Ok(ApiResponse<EstimateDetailsResponse>.FromData(response)));
    }

    private static async Task<IResult> ReorderZonesAsync(
        Guid id,
        ReorderEstimateZonesRequest request,
        ReorderEstimateZonesHandler handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new ReorderEstimateZonesCommand(id, request.ZoneIds), cancellationToken);
        return FromResult(result, httpContext, response => Results.Ok(ApiResponse<EstimateDetailsResponse>.FromData(response)));
    }

    private static async Task<IResult> DeleteZoneAsync(
        Guid id,
        Guid zoneId,
        RemoveEstimateZoneHandler handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new RemoveEstimateZoneCommand(id, zoneId), cancellationToken);
        return FromResult(result, httpContext, response => Results.Ok(ApiResponse<EstimateDetailsResponse>.FromData(response)));
    }

    private static async Task<IResult> AddWorkItemAsync(
        Guid id,
        AddEstimateWorkItemRequest request,
        AddEstimateWorkItemHandler handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await handler.HandleAsync(
            new AddEstimateWorkItemCommand(
                id,
                request.ZoneId,
                request.ConstructionWorkId,
                request.Quantity,
                request.UnitPrice,
                request.Notes),
            cancellationToken);

        return FromResult(result, httpContext, response => Results.Ok(ApiResponse<EstimateDetailsResponse>.FromData(response)));
    }

    private static async Task<IResult> UpdateWorkItemAsync(
        Guid id,
        Guid itemId,
        UpdateEstimateLineItemRequest request,
        UpdateEstimateWorkItemHandler handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await handler.HandleAsync(
            new UpdateEstimateWorkItemCommand(
                id,
                itemId,
                request.Quantity,
                request.UnitPrice,
                request.Notes),
            cancellationToken);

        return FromResult(result, httpContext, response => Results.Ok(ApiResponse<EstimateDetailsResponse>.FromData(response)));
    }

    private static async Task<IResult> DeleteWorkItemAsync(
        Guid id,
        Guid itemId,
        RemoveEstimateWorkItemHandler handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new RemoveEstimateWorkItemCommand(id, itemId), cancellationToken);
        return result.IsSuccess
            ? Results.NoContent()
            : ExpectedError(result.Error, httpContext);
    }

    private static async Task<IResult> DuplicateWorkItemAsync(
        Guid id,
        Guid itemId,
        DuplicateEstimateWorkItemHandler handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new DuplicateEstimateWorkItemCommand(id, itemId), cancellationToken);
        return FromResult(result, httpContext, response => Results.Ok(ApiResponse<EstimateDetailsResponse>.FromData(response)));
    }

    private static async Task<IResult> AddMaterialItemAsync(
        Guid id,
        AddEstimateMaterialItemRequest request,
        AddEstimateMaterialItemHandler handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await handler.HandleAsync(
            new AddEstimateMaterialItemCommand(
                id,
                request.ZoneId,
                request.MaterialId,
                request.Quantity,
                request.UnitPrice,
                request.Notes),
            cancellationToken);

        return FromResult(result, httpContext, response => Results.Ok(ApiResponse<EstimateDetailsResponse>.FromData(response)));
    }

    private static async Task<IResult> UpdateMaterialItemAsync(
        Guid id,
        Guid itemId,
        UpdateEstimateLineItemRequest request,
        UpdateEstimateMaterialItemHandler handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await handler.HandleAsync(
            new UpdateEstimateMaterialItemCommand(
                id,
                itemId,
                request.Quantity,
                request.UnitPrice,
                request.Notes),
            cancellationToken);

        return FromResult(result, httpContext, response => Results.Ok(ApiResponse<EstimateDetailsResponse>.FromData(response)));
    }

    private static async Task<IResult> DeleteMaterialItemAsync(
        Guid id,
        Guid itemId,
        RemoveEstimateMaterialItemHandler handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new RemoveEstimateMaterialItemCommand(id, itemId), cancellationToken);
        return result.IsSuccess
            ? Results.NoContent()
            : ExpectedError(result.Error, httpContext);
    }

    private static async Task<IResult> DuplicateMaterialItemAsync(
        Guid id,
        Guid itemId,
        DuplicateEstimateMaterialItemHandler handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new DuplicateEstimateMaterialItemCommand(id, itemId), cancellationToken);
        return FromResult(result, httpContext, response => Results.Ok(ApiResponse<EstimateDetailsResponse>.FromData(response)));
    }

    private static CreateEstimateLineItemCommand MapLineItem(CreateEstimateLineItemRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new CreateEstimateLineItemCommand(
            request.Name,
            request.Quantity,
            request.MeasurementUnit,
            request.UnitPrice,
            request.Notes);
    }

    private static IResult FromResult<TResponse>(
        Result<TResponse> result,
        HttpContext httpContext,
        Func<TResponse, IResult> onSuccess)
        where TResponse : class
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(onSuccess);

        if (result.IsSuccess && result.Value is { } response)
        {
            return onSuccess(response);
        }

        return ExpectedError(result.Error, httpContext);
    }

    private static IResult ExpectedError(Error error, HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(error);
        ArgumentNullException.ThrowIfNull(httpContext);

        var payload = ApiResponse<object>.FromError(new ApiError(
            error.Code,
            error.Message,
            Activity.Current?.Id ?? httpContext.TraceIdentifier));

        return Results.Json(payload, statusCode: GetStatusCode(error));
    }

    private static int GetStatusCode(Error error) => error.Code switch
    {
        "ValidationError" => StatusCodes.Status422UnprocessableEntity,
        "EstimateNotFound" => StatusCodes.Status404NotFound,
        "EstimateZoneNotFound" => StatusCodes.Status404NotFound,
        "EstimateWorkItemNotFound" => StatusCodes.Status404NotFound,
        "EstimateMaterialItemNotFound" => StatusCodes.Status404NotFound,
        "ConstructionWorkNotFound" => StatusCodes.Status404NotFound,
        "MaterialNotFound" => StatusCodes.Status404NotFound,
        "KnowledgeUnitNotFound" => StatusCodes.Status404NotFound,
        "EstimateNumberAlreadyExists" => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status400BadRequest
    };
}
