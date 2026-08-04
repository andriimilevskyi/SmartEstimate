using FluentValidation;

namespace SmartEstimate.Application.Estimates.ReorderEstimateZones;

/// <summary>
/// Validates a zone reorder request.
/// </summary>
public sealed class ReorderEstimateZonesCommandValidator : AbstractValidator<ReorderEstimateZonesCommand>
{
    public ReorderEstimateZonesCommandValidator()
    {
        RuleFor(command => command.EstimateId).NotEmpty();
        RuleFor(command => command.ZoneIds).NotEmpty();
        RuleForEach(command => command.ZoneIds).NotEmpty();
    }
}
