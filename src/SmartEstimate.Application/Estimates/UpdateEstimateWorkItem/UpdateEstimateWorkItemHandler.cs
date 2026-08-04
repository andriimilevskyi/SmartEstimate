using FluentValidation;
using MapsterMapper;
using SmartEstimate.Application.Abstractions.Persistence;
using SmartEstimate.Domain.Estimates.ValueObjects;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Application.Estimates.UpdateEstimateWorkItem;

/// <summary>
/// Updates an existing work line through the owning Estimate aggregate.
/// </summary>
public sealed class UpdateEstimateWorkItemHandler(
    IEstimateRepository repository,
    IValidator<UpdateEstimateWorkItemCommand> validator,
    IMapper mapper)
{
    public async Task<Result<EstimateDetailsResponse>> HandleAsync(
        UpdateEstimateWorkItemCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<EstimateDetailsResponse>.Failure(EstimateErrors.Validation(validationResult));
        }

        var estimate = await repository.GetByIdAsync(command.EstimateId, cancellationToken);
        if (estimate is null)
        {
            return Result<EstimateDetailsResponse>.Failure(EstimateErrors.NotFound(command.EstimateId));
        }

        if (!estimate.WorkItems.Any(item => item.Id == command.ItemId))
        {
            return Result<EstimateDetailsResponse>.Failure(
                EstimateErrors.WorkItemNotFound(command.EstimateId, command.ItemId));
        }

        estimate.UpdateWorkItem(
            command.ItemId,
            new Quantity(command.Quantity),
            new Money(command.UnitPrice, estimate.Currency),
            command.Notes,
            DateTimeOffset.UtcNow);

        await repository.SaveChangesAsync(cancellationToken);

        return Result<EstimateDetailsResponse>.Success(mapper.Map<EstimateDetailsResponse>(estimate));
    }
}
