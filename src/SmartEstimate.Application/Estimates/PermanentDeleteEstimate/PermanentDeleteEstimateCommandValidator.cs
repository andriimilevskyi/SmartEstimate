using FluentValidation;

namespace SmartEstimate.Application.Estimates.PermanentDeleteEstimate;

public sealed class PermanentDeleteEstimateCommandValidator : AbstractValidator<PermanentDeleteEstimateCommand>
{
    public PermanentDeleteEstimateCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
