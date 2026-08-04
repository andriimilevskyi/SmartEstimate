namespace SmartEstimate.Application.Estimates.DuplicateEstimateMaterialItem;

/// <summary>
/// Duplicates an existing material line in the same zone.
/// </summary>
public sealed record DuplicateEstimateMaterialItemCommand(Guid EstimateId, Guid ItemId);
