namespace SmartEstimate.Application.Estimates.RemoveEstimateMaterialItem;

/// <summary>
/// Removes a material line from an estimate.
/// </summary>
public sealed record RemoveEstimateMaterialItemCommand(Guid EstimateId, Guid ItemId);
