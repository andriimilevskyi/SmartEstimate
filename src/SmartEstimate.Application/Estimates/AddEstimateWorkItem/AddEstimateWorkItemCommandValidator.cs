using FluentValidation;

namespace SmartEstimate.Application.Estimates.AddEstimateWorkItem;

/// <summary>
/// Validates input for adding a catalog construction work to an estimate.
/// </summary>
public sealed class AddEstimateWorkItemCommandValidator : AbstractValidator<AddEstimateWorkItemCommand>
{
    public AddEstimateWorkItemCommandValidator()
    {
        RuleFor(command => command.EstimateId)
            .NotEmpty();

        RuleFor(command => command.ZoneId)
            .NotEmpty();

        RuleFor(command => command.ConstructionWorkId)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(command => command.Quantity)
            .GreaterThan(decimal.Zero)
            .PrecisionScale(18, 3, false);

        RuleFor(command => command.UnitPrice)
            .GreaterThanOrEqualTo(decimal.Zero)
            .PrecisionScale(18, 2, false)
            .When(command => command.UnitPrice.HasValue);

        RuleFor(command => command.Notes)
            .MaximumLength(2_000)
            .When(command => command.Notes is not null);
    }
}
