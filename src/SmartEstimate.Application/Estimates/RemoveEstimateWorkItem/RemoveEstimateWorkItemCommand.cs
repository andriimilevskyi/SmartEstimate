namespace SmartEstimate.Application.Estimates.RemoveEstimateWorkItem;

/// <summary>
/// Removes a work line from an estimate.
/// </summary>
public sealed record RemoveEstimateWorkItemCommand(Guid EstimateId, Guid ItemId);
