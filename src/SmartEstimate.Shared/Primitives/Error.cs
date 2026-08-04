namespace SmartEstimate.Shared.Primitives;

/// <summary>
/// Describes a non-exceptional error returned by an application use case.
/// </summary>
public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}
