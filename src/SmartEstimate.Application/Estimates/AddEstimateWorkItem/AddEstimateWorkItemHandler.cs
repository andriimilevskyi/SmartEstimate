using FluentValidation;
using MapsterMapper;
using SmartEstimate.Application.Abstractions.Persistence;
using SmartEstimate.Application.Knowledge.Abstractions;
using SmartEstimate.Domain.Estimates.ValueObjects;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Application.Estimates.AddEstimateWorkItem;

/// <summary>
/// Resolves an active construction work from PostgreSQL and adds its immutable snapshot to an Estimate.
/// </summary>
public sealed class AddEstimateWorkItemHandler(
    IEstimateRepository repository,
    IConstructionWorkRepository constructionWorks,
    IUnitRepository units,
    IValidator<AddEstimateWorkItemCommand> validator,
    IMapper mapper)
{
    public async Task<Result<EstimateDetailsResponse>> HandleAsync(
        AddEstimateWorkItemCommand command,
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

        if (!estimate.Zones.Any(zone => zone.Id == command.ZoneId))
        {
            return Result<EstimateDetailsResponse>.Failure(EstimateErrors.ZoneNotFound(command.EstimateId, command.ZoneId));
        }

        if (!Guid.TryParse(command.ConstructionWorkId, out var constructionWorkId))
        {
            return Result<EstimateDetailsResponse>.Failure(
                EstimateErrors.ConstructionWorkNotFound(command.ConstructionWorkId));
        }

        var constructionWork = await constructionWorks.GetByIdAsync(constructionWorkId, cancellationToken);
        if (constructionWork is null || constructionWork.Status != SmartEstimate.Domain.Knowledge.KnowledgeStatus.Active)
        {
            return Result<EstimateDetailsResponse>.Failure(EstimateErrors.ConstructionWorkNotFound(command.ConstructionWorkId));
        }

        var unit = await units.GetByIdAsync(constructionWork.UnitId, cancellationToken);
        if (unit is null || unit.Status != SmartEstimate.Domain.Knowledge.KnowledgeStatus.Active)
        {
            return Result<EstimateDetailsResponse>.Failure(EstimateErrors.UnitNotFound(constructionWork.UnitId.ToString()));
        }

        estimate.AddWorkItem(
            constructionWork.Name.Uk,
            new Quantity(command.Quantity),
            new MeasurementUnit(unit.Symbol),
            new Money(command.UnitPrice, estimate.Currency),
            command.Notes,
            DateTimeOffset.UtcNow,
            constructionWork.Id.ToString(),
            command.ZoneId);

        await repository.SaveChangesAsync(cancellationToken);

        return Result<EstimateDetailsResponse>.Success(mapper.Map<EstimateDetailsResponse>(estimate));
    }
}
