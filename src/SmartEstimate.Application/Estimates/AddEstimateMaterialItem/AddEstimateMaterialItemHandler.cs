using FluentValidation;
using MapsterMapper;
using SmartEstimate.Application.Abstractions.Persistence;
using SmartEstimate.Application.Knowledge.Abstractions;
using SmartEstimate.Domain.Estimates.ValueObjects;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Application.Estimates.AddEstimateMaterialItem;

/// <summary>
/// Resolves an active material from PostgreSQL and adds its immutable snapshot to an Estimate.
/// </summary>
public sealed class AddEstimateMaterialItemHandler(
    IEstimateRepository repository,
    IMaterialRepository materials,
    IUnitRepository units,
    IValidator<AddEstimateMaterialItemCommand> validator,
    IMapper mapper)
{
    public async Task<Result<EstimateDetailsResponse>> HandleAsync(
        AddEstimateMaterialItemCommand command,
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

        if (!Guid.TryParse(command.MaterialId, out var materialId))
        {
            return Result<EstimateDetailsResponse>.Failure(EstimateErrors.MaterialNotFound(command.MaterialId));
        }

        var material = await materials.GetByIdAsync(materialId, cancellationToken);
        if (material is null || material.Status != SmartEstimate.Domain.Knowledge.KnowledgeStatus.Active)
        {
            return Result<EstimateDetailsResponse>.Failure(EstimateErrors.MaterialNotFound(command.MaterialId));
        }

        var unit = await units.GetByIdAsync(material.UnitId, cancellationToken);
        if (unit is null || unit.Status != SmartEstimate.Domain.Knowledge.KnowledgeStatus.Active)
        {
            return Result<EstimateDetailsResponse>.Failure(EstimateErrors.UnitNotFound(material.UnitId.ToString()));
        }

        estimate.AddMaterialItem(
            material.Name.Uk,
            new Quantity(command.Quantity),
            new MeasurementUnit(unit.Symbol),
            new Money(command.UnitPrice, estimate.Currency),
            command.Notes,
            DateTimeOffset.UtcNow,
            material.Id.ToString(),
            command.ZoneId);

        await repository.SaveChangesAsync(cancellationToken);

        return Result<EstimateDetailsResponse>.Success(mapper.Map<EstimateDetailsResponse>(estimate));
    }
}
