namespace SmartEstimate.Domain.Estimates.ValueObjects;

/// <summary>
/// Represents a non-negative monetary amount in an ISO 4217 currency.
/// </summary>
public sealed record Money
{
    public Money(decimal amount, string currency)
    {
        if (amount < decimal.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "A monetary amount cannot be negative.");
        }

        if (decimal.Round(amount, 2, MidpointRounding.AwayFromZero) != amount)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "A monetary amount cannot have more than two decimal places.");
        }

        Amount = amount;
        Currency = NormalizeCurrency(currency);
    }

    public decimal Amount { get; }

    public string Currency { get; }

    public static Money Zero(string currency) => new(decimal.Zero, currency);

    private static string NormalizeCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency is required.", nameof(currency));
        }

        var normalized = currency.Trim().ToUpperInvariant();
        if (normalized.Length != 3 || normalized.Any(character => character is < 'A' or > 'Z'))
        {
            throw new ArgumentException("Currency must be a three-letter ISO 4217 code.", nameof(currency));
        }

        return normalized;
    }
}
