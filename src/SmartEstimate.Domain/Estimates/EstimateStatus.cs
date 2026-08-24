namespace SmartEstimate.Domain.Estimates;

/// <summary>
/// Simple commercial lifecycle of an estimate.
/// </summary>
public enum EstimateStatus
{
    Draft = 0,
    InProgress = 1,
    Sent = 2,
    Approved = 3,
    Completed = 4,
    Archived = 5
}
