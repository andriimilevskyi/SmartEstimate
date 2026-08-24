using FluentValidation;
using SmartEstimate.Application.Abstractions.Persistence;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Application.Estimates.PermanentDeleteEstimate;

/// <summary>
/// Permanently deletes an already soft-deleted Estimate aggregate and its in-aggregate children.
/// </summary>
public sealed class PermanentDeleteEstimateHandler(
    IEstimateRepository repository,
    IValidator<PermanentDeleteEstimateCommand> validator)
{
    public async Task<Result> HandleAsync(PermanentDeleteEstimateCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure(EstimateErrors.Validation(validationResult));
        }

        var estimate = await repository.GetByIdIncludingDeletedAsync(command.Id, cancellationToken);
        if (estimate is null)
        {
            return Result.Failure(EstimateErrors.NotFound(command.Id));
        }

        if (!estimate.IsDeleted)
        {
            return Result.Failure(EstimateErrors.PermanentDeleteRequiresSoftDelete());
        }

        await repository.RemoveAsync(estimate, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
