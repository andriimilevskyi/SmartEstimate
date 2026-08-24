using FluentValidation;
using SmartEstimate.Application.Abstractions.Persistence;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Application.Estimates.GetEstimates;

/// <summary>
/// Handles retrieval of a page of active estimates.
/// </summary>
public sealed class GetEstimatesHandler(
    IEstimateRepository repository,
    IValidator<GetEstimatesQuery> validator,
    EstimateResponseFactory responseFactory)
{
    public async Task<Result<PagedEstimatesResponse>> HandleAsync(
        GetEstimatesQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var validationResult = await validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<PagedEstimatesResponse>.Failure(EstimateErrors.Validation(validationResult));
        }

        var page = await repository.GetPageAsync(
            new EstimateListQuery(query.Page, query.PageSize, query.Search, query.Status, query.CustomerId, query.ObjectId),
            cancellationToken);
        var items = await responseFactory.CreateSummariesAsync(page.Items, cancellationToken);

        return Result<PagedEstimatesResponse>.Success(new PagedEstimatesResponse(
            items,
            query.Page,
            query.PageSize,
            page.TotalCount));
    }
}
