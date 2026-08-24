using FluentValidation;

namespace SmartEstimate.Application.Pricing;

public sealed class PriceWriteRequestValidator : AbstractValidator<PriceWriteRequest>
{
    public PriceWriteRequestValidator()
    {
        RuleFor(request => request.TargetId).NotEmpty();
        RuleFor(request => request.Amount).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, true);
        RuleFor(request => request.Currency)
            .NotEmpty()
            .Length(3)
            .Matches("^[A-Za-z]{3}$");
        RuleFor(request => request.RegionCode).MaximumLength(64);
        RuleFor(request => request.SupplierName).MaximumLength(256);
        RuleFor(request => request.Notes).MaximumLength(1_000);
    }
}
