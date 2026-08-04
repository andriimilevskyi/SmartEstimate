using FluentValidation;
using SmartEstimate.Application.Abstractions.Persistence;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Application.Estimates.RemoveEstimateWorkItem;

/// <summary>
/// Removes a work line through the owning Estimate aggregate.
/// </summary>
public sealed class RemoveEstimateWorkItemHandler(
    IEstimateRepository repository,
    IValidator<RemoveEstimateWorkItemCommand> validator)
{
    public async Task<Result> HandleAsync(
        RemoveEstimateWorkItemCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure(EstimateErrors.Validation(validationResult));
        }

        var estimate = await repository.GetByIdAsync(command.EstimateId, cancellationToken);
        if (estimate is null)
        {
            return Result.Failure(EstimateErrors.NotFound(command.EstimateId));
        }

        if (!estimate.WorkItems.Any(item => item.Id == command.ItemId))
        {
            return Result.Failure(EstimateErrors.WorkItemNotFound(command.EstimateId, command.ItemId));
        }

        estimate.RemoveWorkItem(command.ItemId, DateTimeOffset.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
