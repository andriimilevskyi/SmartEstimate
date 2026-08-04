using FluentValidation.Results;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Application.Knowledge;

/// <summary>
/// Creates expected errors for Knowledge catalogue query validation.
/// </summary>
internal static class KnowledgeErrors
{
    public static Error Validation(ValidationResult validationResult)
    {
        ArgumentNullException.ThrowIfNull(validationResult);

        var message = string.Join(
            " ",
            validationResult.Errors.Select(error => $"{error.PropertyName}: {error.ErrorMessage}"));

        return new Error("ValidationError", message);
    }
}
