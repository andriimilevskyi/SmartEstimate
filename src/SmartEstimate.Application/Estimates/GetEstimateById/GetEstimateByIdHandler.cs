using FluentValidation;
using SmartEstimate.Application.Abstractions.Persistence;
using SmartEstimate.Domain.Estimates;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Application.Estimates.GetEstimateById;

/// <summary>
/// Handles retrieval of an active estimate by identifier.
/// </summary>
public sealed class GetEstimateByIdHandler(
    IEstimateRepository repository,
    IValidator<GetEstimateByIdQuery> validator,
    EstimateResponseFactory responseFactory)
{
    public async Task<Result<EstimateDetailsResponse>> HandleAsync(
        GetEstimateByIdQuery query,
        CancellationToken cancellationToken,
        EstimateDisplayLocale locale = EstimateDisplayLocale.Uk)
    {
        ArgumentNullException.ThrowIfNull(query);

        var validationResult = await validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<EstimateDetailsResponse>.Failure(EstimateErrors.Validation(validationResult));
        }

        var estimate = await repository.GetByIdIncludingDeletedAsync(query.Id, cancellationToken);
        if (estimate is null)
        {
            return Result<EstimateDetailsResponse>.Failure(EstimateErrors.NotFound(query.Id));
        }

        return Result<EstimateDetailsResponse>.Success(await responseFactory.CreateDetailsAsync(estimate, locale, cancellationToken));
    }
}
