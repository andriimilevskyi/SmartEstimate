using FluentValidation;

namespace SmartEstimate.Application.Estimates.UpdateEstimateStatus;

public sealed class UpdateEstimateStatusCommandValidator : AbstractValidator<UpdateEstimateStatusCommand>
{
    public UpdateEstimateStatusCommandValidator()
    {
        RuleFor(command => command.EstimateId).NotEmpty();
        RuleFor(command => command.Status).IsInEnum();
    }
}
