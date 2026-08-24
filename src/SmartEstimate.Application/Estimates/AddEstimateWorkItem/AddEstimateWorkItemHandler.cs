using FluentValidation;
using SmartEstimate.Application.Abstractions.Persistence;
using SmartEstimate.Application.Knowledge.Abstractions;
using SmartEstimate.Application.Pricing.Abstractions;
using SmartEstimate.Domain.Estimates;
using SmartEstimate.Domain.Estimates.ValueObjects;
using SmartEstimate.Domain.Pricing;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Application.Estimates.AddEstimateWorkItem;

/// <summary>
/// Resolves an active construction work from PostgreSQL and adds its immutable snapshot to an Estimate.
/// </summary>
public sealed class AddEstimateWorkItemHandler(
    IEstimateRepository repository,
    IConstructionWorkRepository constructionWorks,
    IUnitRepository units,
    IPriceResolver priceResolver,
    IValidator<AddEstimateWorkItemCommand> validator,
    EstimateResponseFactory responseFactory)
{
    public async Task<Result<EstimateDetailsResponse>> HandleAsync(
        AddEstimateWorkItemCommand command,
        CancellationToken cancellationToken,
        EstimateDisplayLocale locale = EstimateDisplayLocale.Uk)
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

        var capturedAt = DateTimeOffset.UtcNow;
        var resolvedPrice = command.UnitPrice.HasValue
            ? null
            : await priceResolver.GetCurrentPriceAsync(
                new PriceTarget(PriceTargetType.ConstructionWork, constructionWork.Id),
                estimate.Currency,
                null,
                null,
                null,
                capturedAt,
                cancellationToken);
        var unitPrice = command.UnitPrice ?? resolvedPrice?.Amount ?? decimal.Zero;

        estimate.AddWorkItem(
            constructionWork.Name.Uk,
            new Quantity(command.Quantity),
            new MeasurementUnit(unit.Symbol),
            new Money(unitPrice, estimate.Currency),
            command.Notes,
            capturedAt,
            constructionWork.Id.ToString(),
            command.ZoneId,
            resolvedPrice?.PriceId,
            resolvedPrice is null ? null : capturedAt,
            command.UnitPrice.HasValue,
            new LocalizedNameSnapshot(constructionWork.Name.Uk, constructionWork.Name.En, constructionWork.Name.De),
            EstimateItemNameSource.KnowledgeSnapshot);

        await repository.SaveChangesAsync(cancellationToken);

        return Result<EstimateDetailsResponse>.Success(await responseFactory.CreateDetailsAsync(estimate, locale, cancellationToken));
    }
}
