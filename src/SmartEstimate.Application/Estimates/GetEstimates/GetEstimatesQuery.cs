namespace SmartEstimate.Application.Estimates.GetEstimates;

/// <summary>
/// Requests a paged collection of active estimates.
/// </summary>
public sealed record GetEstimatesQuery(int Page = 1, int PageSize = 20);
