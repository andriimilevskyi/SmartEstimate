namespace SmartEstimate.Contracts.Common;

/// <summary>
/// A stable API error contract. Endpoint-specific error codes are introduced with their features.
/// </summary>
public sealed record ApiError(string Code, string Message, string? TraceId = null);
