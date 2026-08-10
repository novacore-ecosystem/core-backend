namespace NovaCore.Payment.Domain.ValueObjects;

/// <summary>
/// Currency-aware money amount, local to PaymentService. The shared
/// NovaCore.BuildingBlock.Domain.ValueObjects.Money is a bare non-negative decimal with no
/// currency - insufficient for a service that must be currency-aware by design - so this is a
/// deliberate PaymentService-local Value Object, following the same per-bounded-context precedent
/// already set by Address being duplicated in Inventory/User rather than shared.
/// </summary>
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, Currency currency)
    {
        if (!IsValid(amount))
            throw ExceptionFactory.InvalidRange("Money amount must be greater than or equal to zero.");

        return new Money(amount, currency);
    }

    public static Money Create(decimal amount, string currencyCode)
        => Create(amount, Currency.Create(currencyCode));

    public static bool IsValid(decimal amount) => amount >= 0;

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount} {Currency.Value}";
}
