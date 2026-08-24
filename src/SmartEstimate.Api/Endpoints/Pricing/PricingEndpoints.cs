using System.Diagnostics;
using SmartEstimate.Application.Pricing;
using SmartEstimate.Application.Pricing.Abstractions;
using SmartEstimate.Contracts.Common;
using SmartEstimate.Domain.Pricing;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Api.Endpoints.Pricing;

public static class PricingEndpoints
{
    public static IEndpointRouteBuilder MapPricingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/pricing").WithTags("Pricing");

        group.MapGet("/catalog", GetCatalogAsync)
            .WithName("GetPricingCatalog")
            .WithSummary("List priced or unpriced Knowledge items");

        group.MapPost("/prices", CreatePriceAsync)
            .WithName("CreateCatalogPrice")
            .WithSummary("Add a catalog price");

        group.MapPut("/prices/{id:guid}", UpdatePriceAsync)
            .WithName("UpdateCatalogPrice")
            .WithSummary("Create a new effective version of a catalog price");

        group.MapDelete("/prices/{id:guid}", ArchivePriceAsync)
            .WithName("ArchiveCatalogPrice")
            .WithSummary("Archive a catalog price");

        group.MapGet("/history/{targetType}/{targetId:guid}", GetHistoryAsync)
            .WithName("GetCatalogPriceHistory")
            .WithSummary("Get price history for a material or construction work");

        group.MapGet("/resolve/{targetType}/{targetId:guid}", ResolveAsync)
            .WithName("ResolveCatalogPrice")
            .WithSummary("Resolve the current price for a material or construction work");

        return endpoints;
    }

    private static Task<IResult> GetCatalogAsync(
        PricingManagementService service,
        HttpContext context,
        CancellationToken cancellationToken,
        PriceTargetType targetType = PriceTargetType.Material,
        int page = 1,
        int pageSize = 25,
        string? search = null,
        Guid? categoryId = null,
        string? currency = null,
        string? supplier = null,
        string? regionCode = null,
        bool missingOnly = false) =>
        FromResultAsync(
            service.GetCatalogAsync(
                new PricingCatalogQuery(
                    targetType,
                    page,
                    pageSize,
                    search,
                    categoryId,
                    currency,
                    supplier,
                    regionCode,
                    missingOnly,
                    PricingApiLocale.Resolve(context)),
                cancellationToken),
            context);

    private static async Task<IResult> CreatePriceAsync(
        PriceWriteRequest request,
        PricingManagementService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await service.CreatePriceAsync(request, cancellationToken);
        return result.IsSuccess && result.Value is { } value
            ? Results.Created($"/api/v1/pricing/prices/{value.Id}", ApiResponse<PriceSummaryResponse>.FromData(value))
            : Error(result.Error, context);
    }

    private static Task<IResult> UpdatePriceAsync(
        Guid id,
        PriceWriteRequest request,
        PricingManagementService service,
        HttpContext context,
        CancellationToken cancellationToken) =>
        FromResultAsync(service.UpdatePriceAsync(id, request, cancellationToken), context);

    private static async Task<IResult> ArchivePriceAsync(
        Guid id,
        PricingManagementService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await service.ArchivePriceAsync(id, cancellationToken);
        return result.IsSuccess ? Results.NoContent() : Error(result.Error, context);
    }

    private static Task<IResult> GetHistoryAsync(
        PriceTargetType targetType,
        Guid targetId,
        PricingManagementService service,
        HttpContext context,
        CancellationToken cancellationToken) =>
        FromResultAsync(service.GetHistoryAsync(targetType, targetId, cancellationToken), context);

    private static async Task<IResult> ResolveAsync(
        PriceTargetType targetType,
        Guid targetId,
        IPriceResolver resolver,
        HttpContext context,
        CancellationToken cancellationToken,
        string currency = "UAH",
        string? regionCode = null,
        Guid? supplierId = null,
        string? supplierName = null,
        DateTimeOffset? date = null)
    {
        var resolved = await resolver.GetCurrentPriceAsync(
            new PriceTarget(targetType, targetId),
            currency,
            regionCode,
            supplierId,
            supplierName,
            date ?? DateTimeOffset.UtcNow,
            cancellationToken);

        return resolved is null
            ? Results.NotFound(ApiResponse<object>.FromError(new ApiError("PriceNotFound", "No matching price was found.", Activity.Current?.Id ?? context.TraceIdentifier)))
            : Results.Ok(ApiResponse<ResolvedPriceResponse>.FromData(resolved));
    }

    private static async Task<IResult> FromResultAsync<T>(Task<Result<T>> task, HttpContext context)
    {
        var result = await task;
        return result.IsSuccess && result.Value is { } value
            ? Results.Ok(ApiResponse<T>.FromData(value))
            : Error(result.Error, context);
    }

    private static IResult Error(Error error, HttpContext context) => Results.Json(
        ApiResponse<object>.FromError(new ApiError(error.Code, error.Message, Activity.Current?.Id ?? context.TraceIdentifier)),
        statusCode: error.Code switch
        {
            "PriceNotFound" or "PriceTargetNotFound" => StatusCodes.Status404NotFound,
            "PriceTargetInactive" or "ValidationError" => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status400BadRequest
        });
}
