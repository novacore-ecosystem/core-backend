namespace NovaCore.Payment.Domain.Entities.Operations;

/// <summary>Future multi-currency support - a point-in-time conversion rate between two currencies. Not consumed by any conversion logic yet.</summary>
public sealed class ExchangeRate : AggregateRoot<Guid>, IAuditable
{
    public Currency FromCurrency { get; private set; } = default!;
    public Currency ToCurrency { get; private set; } = default!;
    public decimal Rate { get; private set; }
    public DateTime EffectiveAt { get; private set; }

    private ExchangeRate() { }

    public static ExchangeRate Create(Currency fromCurrency, Currency toCurrency, decimal rate, DateTime effectiveAt)
    {
        if (rate <= 0)
            throw ExceptionFactory.InvalidRange("Exchange rate must be greater than zero.");

        return new ExchangeRate
        {
            Id = Guid.CreateVersion7(),
            FromCurrency = fromCurrency,
            ToCurrency = toCurrency,
            Rate = rate,
            EffectiveAt = effectiveAt,
        };
    }
}
