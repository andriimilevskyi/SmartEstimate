using System.Diagnostics;
using SmartEstimate.Application.Business;
using SmartEstimate.Application.Business.Abstractions;
using SmartEstimate.Contracts.Common;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Api.Endpoints.Business;

public static class BusinessEndpoints
{
    public static IEndpointRouteBuilder MapBusinessEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/overview", GetOverviewAsync)
            .WithTags("Overview")
            .WithName("GetOverview")
            .WithSummary("Get overview dashboard data");

        var customers = endpoints.MapGroup("/api/v1/customers").WithTags("Customers");
        customers.MapGet(string.Empty, GetCustomersAsync).WithName("GetCustomers");
        customers.MapGet("/{id:guid}", GetCustomerAsync).WithName("GetCustomer");
        customers.MapPost(string.Empty, CreateCustomerAsync).WithName("CreateCustomer");
        customers.MapPut("/{id:guid}", UpdateCustomerAsync).WithName("UpdateCustomer");
        customers.MapPatch("/{id:guid}/archive", ArchiveCustomerAsync).WithName("ArchiveCustomer");
        customers.MapPatch("/{id:guid}/restore", RestoreCustomerAsync).WithName("RestoreCustomer");
        customers.MapDelete("/{id:guid}", DeleteCustomerPermanentlyAsync).WithName("DeleteCustomerPermanently");

        var objects = endpoints.MapGroup("/api/v1/objects").WithTags("Objects");
        objects.MapGet(string.Empty, GetObjectsAsync).WithName("GetObjects");
        objects.MapGet("/{id:guid}", GetObjectAsync).WithName("GetObject");
        objects.MapPost(string.Empty, CreateObjectAsync).WithName("CreateObject");
        objects.MapPut("/{id:guid}", UpdateObjectAsync).WithName("UpdateObject");
        objects.MapPatch("/{id:guid}/archive", ArchiveObjectAsync).WithName("ArchiveObject");
        objects.MapPatch("/{id:guid}/restore", RestoreObjectAsync).WithName("RestoreObject");
        objects.MapDelete("/{id:guid}", DeleteObjectPermanentlyAsync).WithName("DeleteObjectPermanently");

        return endpoints;
    }

    private static Task<IResult> GetOverviewAsync(
        BusinessManagementService service,
        HttpContext context,
        CancellationToken cancellationToken) =>
        FromResultAsync(service.GetOverviewAsync(cancellationToken), context);

    private static Task<IResult> GetCustomersAsync(
        BusinessManagementService service,
        HttpContext context,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 100,
        string? search = null,
        string status = "active") =>
        FromResultAsync(service.GetCustomersAsync(new CustomerListQuery(page, pageSize, search, ParseStatus(status)), cancellationToken), context);

    private static Task<IResult> GetCustomerAsync(
        Guid id,
        BusinessManagementService service,
        HttpContext context,
        CancellationToken cancellationToken) =>
        FromResultAsync(service.GetCustomerAsync(id, cancellationToken), context);

    private static Task<IResult> GetObjectsAsync(
        BusinessManagementService service,
        HttpContext context,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 100,
        string? search = null,
        Guid? customerId = null,
        string status = "active") =>
        FromResultAsync(service.GetObjectsAsync(new EstimateObjectListQuery(page, pageSize, search, customerId, ParseStatus(status)), cancellationToken), context);

    private static Task<IResult> GetObjectAsync(
        Guid id,
        BusinessManagementService service,
        HttpContext context,
        CancellationToken cancellationToken) =>
        FromResultAsync(service.GetObjectAsync(id, cancellationToken), context);

    private static async Task<IResult> CreateCustomerAsync(
        CreateCustomerRequest request,
        BusinessManagementService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateCustomerAsync(request, cancellationToken);
        return result.IsSuccess && result.Value is { } value
            ? Results.Created($"/api/v1/customers/{value.Id}", ApiResponse<CustomerResponse>.FromData(value))
            : Error(result.Error, context);
    }

    private static async Task<IResult> UpdateCustomerAsync(
        Guid id,
        CreateCustomerRequest request,
        BusinessManagementService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateCustomerAsync(id, request, cancellationToken);
        return result.IsSuccess && result.Value is { } value
            ? Results.Ok(ApiResponse<CustomerResponse>.FromData(value))
            : Error(result.Error, context);
    }

    private static async Task<IResult> ArchiveCustomerAsync(
        Guid id,
        BusinessManagementService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await service.ArchiveCustomerAsync(id, cancellationToken);
        return result.IsSuccess && result.Value is { } value
            ? Results.Ok(ApiResponse<CustomerResponse>.FromData(value))
            : Error(result.Error, context);
    }

    private static async Task<IResult> RestoreCustomerAsync(
        Guid id,
        BusinessManagementService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await service.RestoreCustomerAsync(id, cancellationToken);
        return result.IsSuccess && result.Value is { } value
            ? Results.Ok(ApiResponse<CustomerResponse>.FromData(value))
            : Error(result.Error, context);
    }

    private static async Task<IResult> DeleteCustomerPermanentlyAsync(
        Guid id,
        BusinessManagementService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await service.DeleteCustomerPermanentlyAsync(id, cancellationToken);
        return result.IsSuccess ? Results.NoContent() : Error(result.Error, context);
    }

    private static async Task<IResult> CreateObjectAsync(
        CreateEstimateObjectRequest request,
        BusinessManagementService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateObjectAsync(request, cancellationToken);
        return result.IsSuccess && result.Value is { } value
            ? Results.Created($"/api/v1/objects/{value.Id}", ApiResponse<EstimateObjectResponse>.FromData(value))
            : Error(result.Error, context);
    }

    private static async Task<IResult> UpdateObjectAsync(
        Guid id,
        CreateEstimateObjectRequest request,
        BusinessManagementService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateObjectAsync(id, request, cancellationToken);
        return result.IsSuccess && result.Value is { } value
            ? Results.Ok(ApiResponse<EstimateObjectResponse>.FromData(value))
            : Error(result.Error, context);
    }

    private static async Task<IResult> ArchiveObjectAsync(
        Guid id,
        BusinessManagementService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await service.ArchiveObjectAsync(id, cancellationToken);
        return result.IsSuccess && result.Value is { } value
            ? Results.Ok(ApiResponse<EstimateObjectResponse>.FromData(value))
            : Error(result.Error, context);
    }

    private static async Task<IResult> RestoreObjectAsync(
        Guid id,
        BusinessManagementService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await service.RestoreObjectAsync(id, cancellationToken);
        return result.IsSuccess && result.Value is { } value
            ? Results.Ok(ApiResponse<EstimateObjectResponse>.FromData(value))
            : Error(result.Error, context);
    }

    private static async Task<IResult> DeleteObjectPermanentlyAsync(
        Guid id,
        BusinessManagementService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await service.DeleteObjectPermanentlyAsync(id, cancellationToken);
        return result.IsSuccess ? Results.NoContent() : Error(result.Error, context);
    }

    private static async Task<IResult> FromResultAsync<T>(Task<Result<T>> task, HttpContext context)
    {
        var result = await task;
        return result.IsSuccess && result.Value is { } value
            ? Results.Ok(ApiResponse<T>.FromData(value))
            : Error(result.Error, context);
    }

    private static BusinessRecordStatus ParseStatus(string? status) =>
        status?.Trim().ToLowerInvariant() switch
        {
            "archived" => BusinessRecordStatus.Archived,
            "all" => BusinessRecordStatus.All,
            _ => BusinessRecordStatus.Active
        };

    private static IResult Error(Error error, HttpContext context) => Results.Json(
        ApiResponse<object>.FromError(new ApiError(error.Code, error.Message, Activity.Current?.Id ?? context.TraceIdentifier)),
        statusCode: error.Code switch
        {
            "CustomerNotFound" or "ObjectNotFound" => StatusCodes.Status404NotFound,
            "CustomerHasObjects" or "ObjectHasEstimates" => StatusCodes.Status409Conflict,
            "ValidationError" => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status400BadRequest
        });
}
