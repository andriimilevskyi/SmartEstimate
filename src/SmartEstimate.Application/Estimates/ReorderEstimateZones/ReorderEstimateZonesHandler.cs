using FluentValidation;
using MapsterMapper;
using SmartEstimate.Application.Abstractions.Persistence;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Application.Estimates.ReorderEstimateZones;

/// <summary>
/// Reorders zones through the owning Estimate aggregate.
/// </summary>
public sealed class ReorderEstimateZonesHandler(
    IEstimateRepository repository,
    IValidator<ReorderEstimateZonesCommand> validator,
    IMapper mapper)
{
    public async Task<Result<EstimateDetailsResponse>> HandleAsync(
        ReorderEstimateZonesCommand command,
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

        estimate.ReorderZones(command.ZoneIds, DateTimeOffset.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return Result<EstimateDetailsResponse>.Success(mapper.Map<EstimateDetailsResponse>(estimate));
    }
}
