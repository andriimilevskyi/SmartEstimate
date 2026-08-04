using FluentValidation;
using MapsterMapper;
using SmartEstimate.Application.Abstractions.Persistence;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Application.Estimates.UpdateEstimateZone;

/// <summary>
/// Renames a zone through the owning Estimate aggregate.
/// </summary>
public sealed class UpdateEstimateZoneHandler(
    IEstimateRepository repository,
    IValidator<UpdateEstimateZoneCommand> validator,
    IMapper mapper)
{
    public async Task<Result<EstimateDetailsResponse>> HandleAsync(
        UpdateEstimateZoneCommand command,
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

        if (!estimate.Zones.Any(zone => zone.Id == command.ZoneId))
        {
            return Result<EstimateDetailsResponse>.Failure(EstimateErrors.ZoneNotFound(command.EstimateId, command.ZoneId));
        }

        estimate.RenameZone(command.ZoneId, command.Name, DateTimeOffset.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return Result<EstimateDetailsResponse>.Success(mapper.Map<EstimateDetailsResponse>(estimate));
    }
}
