using FluentValidation;

namespace SmartEstimate.Application.Knowledge;

public sealed class CategoryWriteRequestValidator : AbstractValidator<CategoryWriteRequest>
{
    public CategoryWriteRequestValidator()
    {
        RuleFor(request => request.Name).SetValidator(new LocalizedTextInputValidator());
        RuleFor(request => request.Description).MaximumLength(4_000);
    }
}

public sealed class ConstructionWorkWriteRequestValidator : AbstractValidator<ConstructionWorkWriteRequest>
{
    public ConstructionWorkWriteRequestValidator()
    {
        RuleFor(request => request.Name).SetValidator(new LocalizedTextInputValidator());
        RuleFor(request => request.CategoryId).NotEmpty();
        RuleFor(request => request.UnitId).NotEmpty();
        RuleFor(request => request.Description).MaximumLength(4_000);
        RuleForEach(request => request.Tags).NotEmpty().MaximumLength(64)
            .When(request => request.Tags is not null);
    }
}

public sealed class MaterialWriteRequestValidator : AbstractValidator<MaterialWriteRequest>
{
    public MaterialWriteRequestValidator()
    {
        RuleFor(request => request.Name).SetValidator(new LocalizedTextInputValidator());
        RuleFor(request => request.UnitId).NotEmpty();
        RuleFor(request => request.Description).MaximumLength(4_000);
        RuleForEach(request => request.Tags).NotEmpty().MaximumLength(64)
            .When(request => request.Tags is not null);
    }
}

public sealed class UnitWriteRequestValidator : AbstractValidator<UnitWriteRequest>
{
    public UnitWriteRequestValidator()
    {
        RuleFor(request => request.Symbol).NotEmpty().MaximumLength(16);
        RuleFor(request => request.Name).SetValidator(new LocalizedTextInputValidator());
    }
}

public sealed class LocalizedTextInputValidator : AbstractValidator<LocalizedTextInput>
{
    public LocalizedTextInputValidator()
    {
        RuleFor(value => value.Uk).NotEmpty().MaximumLength(256);
        RuleFor(value => value.En).MaximumLength(256);
        RuleFor(value => value.De).MaximumLength(256);
    }
}
