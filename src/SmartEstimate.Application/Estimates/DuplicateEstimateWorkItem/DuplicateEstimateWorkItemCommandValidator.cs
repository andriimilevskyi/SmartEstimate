using FluentValidation;

namespace SmartEstimate.Application.Estimates.DuplicateEstimateWorkItem;

/// <summary>
/// Validates a work-line duplication request.
/// </summary>
public sealed class DuplicateEstimateWorkItemCommandValidator : AbstractValidator<DuplicateEstimateWorkItemCommand>
{
    public DuplicateEstimateWorkItemCommandValidator()
    {
        RuleFor(command => command.EstimateId).NotEmpty();
        RuleFor(command => command.ItemId).NotEmpty();
    }
}
