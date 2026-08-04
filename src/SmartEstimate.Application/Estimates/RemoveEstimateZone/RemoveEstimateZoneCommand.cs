namespace SmartEstimate.Application.Estimates.RemoveEstimateZone;

/// <summary>
/// Removes an estimate zone and its line items.
/// </summary>
public sealed record RemoveEstimateZoneCommand(Guid EstimateId, Guid ZoneId);
