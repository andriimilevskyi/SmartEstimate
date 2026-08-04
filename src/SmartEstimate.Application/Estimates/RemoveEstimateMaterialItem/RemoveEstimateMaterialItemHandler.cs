using FluentValidation;
using SmartEstimate.Application.Abstractions.Persistence;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Application.Estimates.RemoveEstimateMaterialItem;

/// <summary>
/// Removes a material line through the owning Estimate aggregate.
/// </summary>
public sealed class RemoveEstimateMaterialItemHandler(
    IEstimateRepository repository,
    IValidator<RemoveEstimateMaterialItemCommand> validator)
{
    public async Task<Result> HandleAsync(
        RemoveEstimateMaterialItemCommand command,
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

        if (!estimate.MaterialItems.Any(item => item.Id == command.ItemId))
        {
            return Result.Failure(EstimateErrors.MaterialItemNotFound(command.EstimateId, command.ItemId));
        }

        estimate.RemoveMaterialItem(command.ItemId, DateTimeOffset.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
