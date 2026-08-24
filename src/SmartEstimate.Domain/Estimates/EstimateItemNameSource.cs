namespace SmartEstimate.Domain.Estimates;

/// <summary>
/// Describes whether an estimate line name came from a localized catalog snapshot, user input, or legacy data.
/// </summary>
public enum EstimateItemNameSource
{
    Legacy,
    KnowledgeSnapshot,
    Custom
}
