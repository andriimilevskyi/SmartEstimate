namespace SmartEstimate.Application.Estimates.DeleteEstimate;

/// <summary>
/// Soft-deletes an Estimate aggregate.
/// </summary>
public sealed record DeleteEstimateCommand(Guid Id);
