namespace NovaCore.Order.Domain.ValueObjects;

public sealed class DiscountRule : ValueObject
{
    public DiscountMethod Method { get; }

    /// <summary>
    /// The configured discount value.
    ///
    /// FixedAmount:
    ///     Monetary amount to deduct.
    ///
    /// Percentage:
    ///     Percentage value (0 - 100).
    /// </summary>
    public decimal Value { get; }

    /// <summary>
    /// Maximum discount amount.
    ///
    /// Only applicable when <see cref="Method"/> is Percentage.
    /// Null means unlimited.
    /// </summary>
    public Money? MaximumDiscount { get; }

    /// <summary>
    /// Minimum order amount required for this discount.
    ///
    /// Null means no minimum requirement.
    /// </summary>
    public Money? MinimumSpend { get; }

    private DiscountRule(
        DiscountMethod method,
        decimal value,
        Money? maximumDiscount,
        Money? minimumSpend)
    {
        Method = method;
        Value = value;
        MaximumDiscount = maximumDiscount;
        MinimumSpend = minimumSpend;
    }

    public static DiscountRule FixedAmount(
        Money amount,
        Money? minimumSpend = null)
    {
        ArgumentNullException.ThrowIfNull(amount);

        return new DiscountRule(
            DiscountMethod.FixedAmount,
            amount.Value,
            null,
            minimumSpend);
    }

    public static DiscountRule Percentage(
        decimal percentage,
        Money? maximumDiscount = null,
        Money? minimumSpend = null)
    {
        if (!IsValidPercentage(percentage))
            throw ExceptionFactory.InvalidRange("Discount percentage must be between 0 and 100.");

        return new DiscountRule(
            DiscountMethod.Percentage,
            percentage,
            maximumDiscount,
            minimumSpend);
    }

    public Money GetFixedAmount()
    {
        if (Method != DiscountMethod.FixedAmount)
            throw ExceptionFactory.InvalidState("Discount is not a fixed amount.");

        return Money.Create(Value);
    }

    public decimal GetPercentage()
    {
        if (Method != DiscountMethod.Percentage)
            throw ExceptionFactory.InvalidState("Discount is not a percentage.");

        return Value;
    }

    public bool IsFixedAmount()
    {
        return Method == DiscountMethod.FixedAmount;
    }

    public bool IsPercentage()
    {
        return Method == DiscountMethod.Percentage;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Method;
        yield return Value;

        yield return MaximumDiscount ?? Money.Create(0);
        yield return MinimumSpend ?? Money.Create(0);
    }

    public static bool IsValidPercentage(decimal percentage)
    {
        return percentage >= 0m && percentage <= 100m;
    }
}
