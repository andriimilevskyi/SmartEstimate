using SmartEstimate.Domain.Estimates;

namespace SmartEstimate.Application.Estimates.UpdateEstimateStatus;

public sealed record UpdateEstimateStatusCommand(Guid EstimateId, EstimateStatus Status);
