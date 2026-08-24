namespace SmartEstimate.Domain.Pricing;

public sealed record PriceTarget
{
    public PriceTarget(PriceTargetType type, Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A price target identifier is required.", nameof(id));
        }

        Type = type;
        Id = id;
    }

    public PriceTargetType Type { get; }
    public Guid Id { get; }
}
