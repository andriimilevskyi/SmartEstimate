namespace SmartEstimate.Domain.Estimates.ValueObjects;

/// <summary>
/// Represents the human-readable identifier of an estimate.
/// </summary>
public sealed record EstimateNumber
{
    public EstimateNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Estimate number is required.", nameof(value));
        }

        var normalized = value.Trim();
        if (normalized.Length > 64)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Estimate number cannot exceed 64 characters.");
        }

        Value = normalized;
    }

    public string Value { get; }

    public override string ToString() => Value;
}
