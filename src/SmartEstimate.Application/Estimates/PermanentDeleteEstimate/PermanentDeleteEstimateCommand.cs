namespace SmartEstimate.Application.Estimates.PermanentDeleteEstimate;

/// <summary>
/// Permanently deletes an already soft-deleted Estimate aggregate.
/// </summary>
public sealed record PermanentDeleteEstimateCommand(Guid Id);
