using FluentValidation;
using MapsterMapper;
using SmartEstimate.Application.Abstractions.Persistence;
using SmartEstimate.Domain.Estimates;
using SmartEstimate.Domain.Estimates.ValueObjects;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Application.Estimates.CreateEstimate;

/// <summary>
/// Handles creation of a new Estimate aggregate.
/// </summary>
public sealed class CreateEstimateHandler(
    IEstimateRepository repository,
    IValidator<CreateEstimateCommand> validator,
    IMapper mapper)
{
    public async Task<Result<EstimateDetailsResponse>> HandleAsync(
        CreateEstimateCommand command,
        CancellationToken cancellationToken)
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

        var now = DateTimeOffset.UtcNow;
        var estimate = Estimate.Create(
            number,
            command.Currency,
            command.ObjectType,
            command.ObjectAddress,
            command.TotalArea,
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

        return Result<EstimateDetailsResponse>.Success(mapper.Map<EstimateDetailsResponse>(estimate));
    }
}
