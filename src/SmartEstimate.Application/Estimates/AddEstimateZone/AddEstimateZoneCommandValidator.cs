using FluentValidation;

namespace SmartEstimate.Application.Estimates.AddEstimateZone;

/// <summary>
/// Validates a request to add an estimate zone.
/// </summary>
public sealed class AddEstimateZoneCommandValidator : AbstractValidator<AddEstimateZoneCommand>
{
    public AddEstimateZoneCommandValidator()
    {
        RuleFor(command => command.EstimateId).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().MaximumLength(128);
    }
}
