using FluentValidation;
using MapsterMapper;
using SmartEstimate.Application.Abstractions.Persistence;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Application.Estimates.DuplicateEstimateMaterialItem;

/// <summary>
/// Duplicates a material line through the owning Estimate aggregate.
/// </summary>
public sealed class DuplicateEstimateMaterialItemHandler(
    IEstimateRepository repository,
    IValidator<DuplicateEstimateMaterialItemCommand> validator,
    IMapper mapper)
{
    public async Task<Result<EstimateDetailsResponse>> HandleAsync(
        DuplicateEstimateMaterialItemCommand command,
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

        if (!estimate.MaterialItems.Any(item => item.Id == command.ItemId))
        {
            return Result<EstimateDetailsResponse>.Failure(EstimateErrors.MaterialItemNotFound(command.EstimateId, command.ItemId));
        }

        estimate.DuplicateMaterialItem(command.ItemId, DateTimeOffset.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return Result<EstimateDetailsResponse>.Success(mapper.Map<EstimateDetailsResponse>(estimate));
    }
}
