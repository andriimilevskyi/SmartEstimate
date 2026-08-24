using FluentValidation.Results;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Application.Business;

internal static class BusinessErrors
{
    public static Error CustomerNotFound(Guid id) => new("CustomerNotFound", $"Customer '{id}' was not found.");

    public static Error ObjectNotFound(Guid id) => new("ObjectNotFound", $"Object '{id}' was not found.");

    public static Error CustomerHasObjects() => new(
        "CustomerHasObjects",
        "Customer cannot be permanently deleted because it has related objects.");

    public static Error ObjectHasEstimates() => new(
        "ObjectHasEstimates",
        "Object cannot be permanently deleted because it has related estimates.");

    public static Error Validation(ValidationResult validationResult)
    {
        var message = string.Join(
            " ",
            validationResult.Errors.Select(error => $"{error.PropertyName}: {error.ErrorMessage}"));

        return new Error("ValidationError", message);
    }
}
