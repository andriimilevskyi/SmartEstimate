namespace SmartEstimate.Application.Estimates.ReorderEstimateZones;

/// <summary>
/// Replaces the visible order of all estimate zones.
/// </summary>
public sealed record ReorderEstimateZonesCommand(Guid EstimateId, IReadOnlyCollection<Guid> ZoneIds);
