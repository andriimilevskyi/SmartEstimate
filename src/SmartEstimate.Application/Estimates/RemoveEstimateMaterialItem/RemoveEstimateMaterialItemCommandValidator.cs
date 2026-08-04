using FluentValidation;

namespace SmartEstimate.Application.Estimates.RemoveEstimateMaterialItem;

/// <summary>
/// Validates a material-line removal request.
/// </summary>
public sealed class RemoveEstimateMaterialItemCommandValidator : AbstractValidator<RemoveEstimateMaterialItemCommand>
{
    public RemoveEstimateMaterialItemCommandValidator()
    {
        RuleFor(command => command.EstimateId)
            .NotEmpty();

        RuleFor(command => command.ItemId)
            .NotEmpty();
    }
}
