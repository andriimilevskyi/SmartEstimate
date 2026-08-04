namespace SmartEstimate.Domain.Estimates.ValueObjects;

/// <summary>
/// Represents a strictly positive quantity with up to three decimal places.
/// </summary>
public sealed record Quantity
{
    public Quantity(decimal value)
    {
        if (value <= decimal.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Quantity must be greater than zero.");
        }

        if (decimal.Round(value, 3, MidpointRounding.AwayFromZero) != value)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Quantity cannot have more than three decimal places.");
        }

        Value = value;
    }

    public decimal Value { get; }
}
