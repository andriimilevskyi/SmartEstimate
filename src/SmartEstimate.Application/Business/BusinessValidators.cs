using FluentValidation;

namespace SmartEstimate.Application.Business;

public sealed class CreateCustomerRequestValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(256);
        RuleFor(request => request.Phone).MaximumLength(64).When(request => request.Phone is not null);
        RuleFor(request => request.Email).EmailAddress().MaximumLength(256).When(request => !string.IsNullOrWhiteSpace(request.Email));
        RuleFor(request => request.Note).MaximumLength(2_000).When(request => request.Note is not null);
    }
}

public sealed class CreateEstimateObjectRequestValidator : AbstractValidator<CreateEstimateObjectRequest>
{
    public CreateEstimateObjectRequestValidator()
    {
        RuleFor(request => request.CustomerId).NotEmpty();
        RuleFor(request => request.Name).NotEmpty().MaximumLength(256);
        RuleFor(request => request.ObjectType).IsInEnum();
        RuleFor(request => request.Address).MaximumLength(512).When(request => request.Address is not null);
        RuleFor(request => request.TotalArea).GreaterThan(decimal.Zero).PrecisionScale(18, 2, false).When(request => request.TotalArea is not null);
        RuleFor(request => request.Description).MaximumLength(2_000).When(request => request.Description is not null);
    }
}
