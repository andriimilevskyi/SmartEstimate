using FluentValidation;

namespace SmartEstimate.Application.Estimates.AddEstimateMaterialItem;

/// <summary>
/// Validates input for adding a catalog material to an estimate.
/// </summary>
public sealed class AddEstimateMaterialItemCommandValidator : AbstractValidator<AddEstimateMaterialItemCommand>
{
    public AddEstimateMaterialItemCommandValidator()
    {
        RuleFor(command => command.EstimateId)
            .NotEmpty();

        RuleFor(command => command.ZoneId)
            .NotEmpty();

        RuleFor(command => command.MaterialId)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(command => command.Quantity)
            .GreaterThan(decimal.Zero)
            .PrecisionScale(18, 3, false);

        RuleFor(command => command.UnitPrice)
            .GreaterThanOrEqualTo(decimal.Zero)
            .PrecisionScale(18, 2, false);

        RuleFor(command => command.Notes)
            .MaximumLength(2_000)
            .When(command => command.Notes is not null);
    }
}
