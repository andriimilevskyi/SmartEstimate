namespace SmartEstimate.Application.Estimates.UpdateEstimateZone;

/// <summary>
/// Renames a user-defined estimate zone.
/// </summary>
public sealed record UpdateEstimateZoneCommand(Guid EstimateId, Guid ZoneId, string Name);
