namespace SmartEstimate.Application.Estimates.DuplicateEstimateWorkItem;

/// <summary>
/// Duplicates an existing work line in the same zone.
/// </summary>
public sealed record DuplicateEstimateWorkItemCommand(Guid EstimateId, Guid ItemId);
