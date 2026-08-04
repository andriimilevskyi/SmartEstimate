namespace SmartEstimate.Domain.Estimates.ValueObjects;

/// <summary>
/// Identifies the unit used to measure an estimate line item.
/// </summary>
public sealed record MeasurementUnit
{
    public MeasurementUnit(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Measurement unit is required.", nameof(value));
        }

        var normalized = value.Trim();
        if (normalized.Length > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Measurement unit cannot exceed 32 characters.");
        }

        Value = normalized;
    }

    public string Value { get; }

    public override string ToString() => Value;
}
