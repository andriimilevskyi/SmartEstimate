using FluentValidation;

namespace SmartEstimate.Application.Estimates.DeleteEstimate;

/// <summary>
/// Validates an estimate identifier before deletion.
/// </summary>
public sealed class DeleteEstimateCommandValidator : AbstractValidator<DeleteEstimateCommand>
{
    public DeleteEstimateCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();
    }
}
