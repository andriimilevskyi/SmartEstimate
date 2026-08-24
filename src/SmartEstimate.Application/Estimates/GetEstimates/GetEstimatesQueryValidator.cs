using FluentValidation;

namespace SmartEstimate.Application.Estimates.GetEstimates;

/// <summary>
/// Validates pagination inputs for the estimates collection.
/// </summary>
public sealed class GetEstimatesQueryValidator : AbstractValidator<GetEstimatesQuery>
{
    public GetEstimatesQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(query => query.Search)
            .MaximumLength(256)
            .When(query => query.Search is not null);
    }
}
