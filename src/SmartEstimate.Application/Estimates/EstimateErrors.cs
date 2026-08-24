using FluentValidation.Results;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Application.Estimates;

/// <summary>
/// Errors shared by Estimate vertical slices.
/// </summary>
internal static class EstimateErrors
{
    public static Error Conflict(string estimateNumber) => new(
        "EstimateNumberAlreadyExists",
        $"An estimate with number '{estimateNumber}' already exists.");

    public static Error NotFound(Guid id) => new(
        "EstimateNotFound",
        $"Estimate '{id}' was not found.");

    public static Error PermanentDeleteRequiresSoftDelete() => new(
        "EstimatePermanentDeleteRequiresSoftDelete",
        "Estimate must be soft-deleted before it can be permanently deleted.");

    public static Error WorkItemNotFound(Guid estimateId, Guid itemId) => new(
        "EstimateWorkItemNotFound",
        $"Work item '{itemId}' was not found in estimate '{estimateId}'.");

    public static Error MaterialItemNotFound(Guid estimateId, Guid itemId) => new(
        "EstimateMaterialItemNotFound",
        $"Material item '{itemId}' was not found in estimate '{estimateId}'.");

    public static Error ZoneNotFound(Guid estimateId, Guid zoneId) => new(
        "EstimateZoneNotFound",
        $"Zone '{zoneId}' was not found in estimate '{estimateId}'.");

    public static Error ObjectNotFound(Guid objectId) => new(
        "ObjectNotFound",
        $"Object '{objectId}' was not found.");

    public static Error ConstructionWorkNotFound(string constructionWorkId) => new(
        "ConstructionWorkNotFound",
        $"Construction work '{constructionWorkId}' was not found in the knowledge catalog.");

    public static Error MaterialNotFound(string materialId) => new(
        "MaterialNotFound",
        $"Material '{materialId}' was not found in the knowledge catalog.");

    public static Error UnitNotFound(string unitId) => new(
        "KnowledgeUnitNotFound",
        $"Measurement unit '{unitId}' was not found in the knowledge catalog.");

    public static Error Validation(ValidationResult validationResult)
    {
        ArgumentNullException.ThrowIfNull(validationResult);

        var message = string.Join(
            " ",
            validationResult.Errors.Select(error => $"{error.PropertyName}: {error.ErrorMessage}"));

        return new Error("ValidationError", message);
    }
}
