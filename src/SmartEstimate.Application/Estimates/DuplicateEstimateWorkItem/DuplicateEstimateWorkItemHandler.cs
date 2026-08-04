using FluentValidation;
using MapsterMapper;
using SmartEstimate.Application.Abstractions.Persistence;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Application.Estimates.DuplicateEstimateWorkItem;

/// <summary>
/// Duplicates a work line through the owning Estimate aggregate.
/// </summary>
public sealed class DuplicateEstimateWorkItemHandler(
    IEstimateRepository repository,
    IValidator<DuplicateEstimateWorkItemCommand> validator,
    IMapper mapper)
{
    public async Task<Result<EstimateDetailsResponse>> HandleAsync(
        DuplicateEstimateWorkItemCommand command,
        CancellationToken cancellationToken)
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

        if (!estimate.WorkItems.Any(item => item.Id == command.ItemId))
        {
            return Result<EstimateDetailsResponse>.Failure(EstimateErrors.WorkItemNotFound(command.EstimateId, command.ItemId));
        }

        estimate.DuplicateWorkItem(command.ItemId, DateTimeOffset.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return Result<EstimateDetailsResponse>.Success(mapper.Map<EstimateDetailsResponse>(estimate));
    }
}
