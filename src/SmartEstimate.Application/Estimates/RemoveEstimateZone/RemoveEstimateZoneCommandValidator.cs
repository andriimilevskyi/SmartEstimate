using FluentValidation;

namespace SmartEstimate.Application.Estimates.RemoveEstimateZone;

/// <summary>
/// Validates a request to remove an estimate zone.
/// </summary>
public sealed class RemoveEstimateZoneCommandValidator : AbstractValidator<RemoveEstimateZoneCommand>
{
    public RemoveEstimateZoneCommandValidator()
    {
        RuleFor(command => command.EstimateId).NotEmpty();
        RuleFor(command => command.ZoneId).NotEmpty();
    }
}
