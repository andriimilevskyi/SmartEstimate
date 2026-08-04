using FluentValidation;

namespace SmartEstimate.Application.Estimates.UpdateEstimateZone;

/// <summary>
/// Validates a request to rename an estimate zone.
/// </summary>
public sealed class UpdateEstimateZoneCommandValidator : AbstractValidator<UpdateEstimateZoneCommand>
{
    public UpdateEstimateZoneCommandValidator()
    {
        RuleFor(command => command.EstimateId).NotEmpty();
        RuleFor(command => command.ZoneId).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().MaximumLength(128);
    }
}
