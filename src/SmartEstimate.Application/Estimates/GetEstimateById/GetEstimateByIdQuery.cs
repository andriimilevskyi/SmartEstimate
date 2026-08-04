namespace SmartEstimate.Application.Estimates.GetEstimateById;

/// <summary>
/// Requests one active estimate by its identifier.
/// </summary>
public sealed record GetEstimateByIdQuery(Guid Id);
