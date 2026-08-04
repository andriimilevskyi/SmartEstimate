using FluentValidation;

namespace SmartEstimate.Application.Estimates.DuplicateEstimateMaterialItem;

/// <summary>
/// Validates a material-line duplication request.
/// </summary>
public sealed class DuplicateEstimateMaterialItemCommandValidator : AbstractValidator<DuplicateEstimateMaterialItemCommand>
{
    public DuplicateEstimateMaterialItemCommandValidator()
    {
        RuleFor(command => command.EstimateId).NotEmpty();
        RuleFor(command => command.ItemId).NotEmpty();
    }
}
