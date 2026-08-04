using FluentValidation;

namespace SmartEstimate.Application.Estimates.RemoveEstimateWorkItem;

/// <summary>
/// Validates a work-line removal request.
/// </summary>
public sealed class RemoveEstimateWorkItemCommandValidator : AbstractValidator<RemoveEstimateWorkItemCommand>
{
    public RemoveEstimateWorkItemCommandValidator()
    {
        RuleFor(command => command.EstimateId)
            .NotEmpty();

        RuleFor(command => command.ItemId)
            .NotEmpty();
    }
}
