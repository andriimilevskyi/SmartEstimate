using FluentValidation;

namespace SmartEstimate.Application.Estimates.GetEstimateById;

/// <summary>
/// Validates an estimate identifier.
/// </summary>
public sealed class GetEstimateByIdQueryValidator : AbstractValidator<GetEstimateByIdQuery>
{
    public GetEstimateByIdQueryValidator()
    {
        RuleFor(query => query.Id)
            .NotEmpty();
    }
}
