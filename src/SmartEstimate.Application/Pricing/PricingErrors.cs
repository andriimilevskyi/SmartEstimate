using FluentValidation.Results;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Application.Pricing;

internal static class PricingErrors
{
    public static Error Validation(ValidationResult validation) =>
        new("ValidationError", string.Join("; ", validation.Errors.Select(error => error.ErrorMessage)));

    public static Error PriceNotFound(Guid id) => new("PriceNotFound", $"Price '{id}' was not found.");

    public static Error TargetNotFound(Guid id) => new("PriceTargetNotFound", $"Pricing target '{id}' was not found.");

    public static Error TargetInactive(Guid id) => new("PriceTargetInactive", $"Pricing target '{id}' is not active.");
}
