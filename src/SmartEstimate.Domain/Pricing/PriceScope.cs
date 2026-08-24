namespace SmartEstimate.Domain.Pricing;

public sealed record PriceScope
{
    public PriceScope(string currency, string? regionCode, Guid? supplierId, string? supplierName)
    {
        Currency = NormalizeCurrency(currency);
        RegionCode = NormalizeRegion(regionCode);
        SupplierId = supplierId == Guid.Empty ? null : supplierId;
        SupplierName = NormalizeSupplierName(supplierName);
    }

    public string Currency { get; }
    public string? RegionCode { get; }
    public Guid? SupplierId { get; }
    public string? SupplierName { get; }

    public bool HasRegion => RegionCode is not null;
    public bool HasSupplier => SupplierId is not null || SupplierName is not null;

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

    private static string? NormalizeRegion(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim().ToUpperInvariant();
        if (normalized.Length > 64)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Region code cannot exceed 64 characters.");
        }

        return normalized;
    }

    private static string? NormalizeSupplierName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > 256)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Supplier name cannot exceed 256 characters.");
        }

        return normalized;
    }
}
