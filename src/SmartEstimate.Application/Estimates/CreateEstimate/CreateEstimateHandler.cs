using FluentValidation;
using SmartEstimate.Application.Abstractions.Persistence;
using SmartEstimate.Application.Business.Abstractions;
using SmartEstimate.Domain.Estimates;
using SmartEstimate.Domain.Estimates.ValueObjects;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Application.Estimates.CreateEstimate;

/// <summary>
/// Handles creation of a new Estimate aggregate.
/// </summary>
public sealed class CreateEstimateHandler(
    IEstimateRepository repository,
    IEstimateObjectRepository objects,
    IValidator<CreateEstimateCommand> validator,
    EstimateResponseFactory responseFactory)
{
    public async Task<Result<EstimateDetailsResponse>> HandleAsync(
        CreateEstimateCommand command,
        CancellationToken cancellationToken,
        EstimateDisplayLocale locale = EstimateDisplayLocale.Uk)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<EstimateDetailsResponse>.Failure(EstimateErrors.Validation(validationResult));
        }

        var number = new EstimateNumber(command.EstimateNumber);
        if (await repository.ExistsByNumberAsync(number, cancellationToken))
        {
            return Result<EstimateDetailsResponse>.Failure(EstimateErrors.Conflict(number.Value));
        }

        if (await objects.GetByIdAsync(command.ObjectId, cancellationToken) is null)
        {
            return Result<EstimateDetailsResponse>.Failure(EstimateErrors.ObjectNotFound(command.ObjectId));
        }

        var now = DateTimeOffset.UtcNow;
        var estimate = Estimate.Create(
            number,
            command.ObjectId,
            command.Currency,
            command.Notes,
            command.Zones,
            now);

        foreach (var item in command.WorkItems ?? [])
        {
            estimate.AddWorkItem(
                item.Name,
                new Quantity(item.Quantity),
                new MeasurementUnit(item.MeasurementUnit),
                new Money(item.UnitPrice, command.Currency),
                item.Notes,
                now);
        }

        foreach (var item in command.MaterialItems ?? [])
        {
            estimate.AddMaterialItem(
                item.Name,
                new Quantity(item.Quantity),
                new MeasurementUnit(item.MeasurementUnit),
                new Money(item.UnitPrice, command.Currency),
                item.Notes,
                now);
        }

        await repository.AddAsync(estimate, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Result<EstimateDetailsResponse>.Success(await responseFactory.CreateDetailsAsync(estimate, locale, cancellationToken));
    }
}
