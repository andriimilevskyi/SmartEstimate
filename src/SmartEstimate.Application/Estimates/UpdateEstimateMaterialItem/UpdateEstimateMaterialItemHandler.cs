using FluentValidation;
using SmartEstimate.Application.Abstractions.Persistence;
using SmartEstimate.Domain.Estimates;
using SmartEstimate.Domain.Estimates.ValueObjects;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Application.Estimates.UpdateEstimateMaterialItem;

/// <summary>
/// Updates an existing material line through the owning Estimate aggregate.
/// </summary>
public sealed class UpdateEstimateMaterialItemHandler(
    IEstimateRepository repository,
    IValidator<UpdateEstimateMaterialItemCommand> validator,
    EstimateResponseFactory responseFactory)
{
    public async Task<Result<EstimateDetailsResponse>> HandleAsync(
        UpdateEstimateMaterialItemCommand command,
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

        if (!estimate.MaterialItems.Any(item => item.Id == command.ItemId))
        {
            return Result<EstimateDetailsResponse>.Failure(
                EstimateErrors.MaterialItemNotFound(command.EstimateId, command.ItemId));
        }

        estimate.UpdateMaterialItem(
            command.ItemId,
            new Quantity(command.Quantity),
            new Money(command.UnitPrice, estimate.Currency),
            command.Notes,
            DateTimeOffset.UtcNow);

        await repository.SaveChangesAsync(cancellationToken);

        return Result<EstimateDetailsResponse>.Success(await responseFactory.CreateDetailsAsync(estimate, locale, cancellationToken));
    }
}
