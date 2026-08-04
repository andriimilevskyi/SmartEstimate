using FluentValidation;

namespace SmartEstimate.Application.Estimates.UpdateEstimateWorkItem;

/// <summary>
/// Validates editable work-line input before the aggregate is changed.
/// </summary>
public sealed class UpdateEstimateWorkItemCommandValidator : AbstractValidator<UpdateEstimateWorkItemCommand>
{
    public UpdateEstimateWorkItemCommandValidator()
    {
        RuleFor(command => command.EstimateId)
            .NotEmpty();

        RuleFor(command => command.ItemId)
            .NotEmpty();

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
