using FluentValidation;

namespace SmartEstimate.Application.Estimates.CreateEstimate;

/// <summary>
/// Validates an incoming create-estimate command before it reaches the domain aggregate.
/// </summary>
public sealed class CreateEstimateCommandValidator : AbstractValidator<CreateEstimateCommand>
{
    public CreateEstimateCommandValidator()
    {
        RuleFor(command => command.EstimateNumber)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(command => command.ObjectId)
            .NotEmpty();

        RuleFor(command => command.Currency)
            .NotEmpty()
            .Length(3)
            .Matches("^[A-Za-z]{3}$");

        RuleFor(command => command.Notes)
            .MaximumLength(2_000)
            .When(command => command.Notes is not null);

        RuleFor(command => command.Zones)
            .NotEmpty();

        RuleForEach(command => command.Zones)
            .NotEmpty()
            .MaximumLength(128);

        RuleForEach(command => command.WorkItems)
            .SetValidator(new CreateEstimateLineItemCommandValidator("Construction work"));

        RuleForEach(command => command.MaterialItems)
            .SetValidator(new CreateEstimateLineItemCommandValidator("Material"));
    }

    private sealed class CreateEstimateLineItemCommandValidator : AbstractValidator<CreateEstimateLineItemCommand>
    {
        public CreateEstimateLineItemCommandValidator(string itemDescription)
        {
            RuleFor(item => item.Name)
                .NotEmpty()
                .MaximumLength(256)
                .WithName($"{itemDescription} name");

            RuleFor(item => item.Quantity)
                .GreaterThan(decimal.Zero)
                .PrecisionScale(18, 3, false);

            RuleFor(item => item.MeasurementUnit)
                .NotEmpty()
                .MaximumLength(32);

            RuleFor(item => item.UnitPrice)
                .GreaterThanOrEqualTo(decimal.Zero)
                .PrecisionScale(18, 2, false);

            RuleFor(item => item.Notes)
                .MaximumLength(2_000)
                .When(item => item.Notes is not null);
        }
    }
}
