using FluentValidation;
using SmartEstimate.Application.Abstractions.Persistence;
using SmartEstimate.Domain.Estimates;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Application.Estimates.UpdateEstimateStatus;

public sealed class UpdateEstimateStatusHandler(
    IEstimateRepository repository,
    IValidator<UpdateEstimateStatusCommand> validator,
    EstimateResponseFactory responseFactory)
{
    public async Task<Result<EstimateDetailsResponse>> HandleAsync(
        UpdateEstimateStatusCommand command,
        CancellationToken cancellationToken,
        EstimateDisplayLocale locale = EstimateDisplayLocale.Uk)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<EstimateDetailsResponse>.Failure(EstimateErrors.Validation(validation));
        }

        var estimate = await repository.GetByIdAsync(command.EstimateId, cancellationToken);
        if (estimate is null)
        {
            return Result<EstimateDetailsResponse>.Failure(EstimateErrors.NotFound(command.EstimateId));
        }

        estimate.ChangeStatus(command.Status, DateTimeOffset.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return Result<EstimateDetailsResponse>.Success(await responseFactory.CreateDetailsAsync(estimate, locale, cancellationToken));
    }
}
