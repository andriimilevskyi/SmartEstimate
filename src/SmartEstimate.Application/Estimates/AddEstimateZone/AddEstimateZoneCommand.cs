namespace SmartEstimate.Application.Estimates.AddEstimateZone;

/// <summary>
/// Adds a user-defined zone to an estimate.
/// </summary>
public sealed record AddEstimateZoneCommand(Guid EstimateId, string Name);
