using FluentValidation;
using SmartEstimate.Application.Abstractions.Persistence;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Application.Estimates.DeleteEstimate;

/// <summary>
/// Handles soft deletion of an Estimate aggregate.
/// </summary>
public sealed class DeleteEstimateHandler(
    IEstimateRepository repository,
    IValidator<DeleteEstimateCommand> validator)
{
    public async Task<Result> HandleAsync(DeleteEstimateCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure(EstimateErrors.Validation(validationResult));
        }

        var estimate = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (estimate is null)
        {
            return Result.Failure(EstimateErrors.NotFound(command.Id));
        }

        estimate.Delete(DateTimeOffset.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
