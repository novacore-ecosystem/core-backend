using NovaCore.Order.Domain.Regexes;

namespace NovaCore.Order.Domain.ValueObjects;

public sealed class OrderNumber : StringValueObject
{
    private OrderNumber(string val) : base(val) { }

    public static OrderNumber Create()
    {
        var suffix = Guid.NewGuid().ToString("N")[..4].ToUpperInvariant();
        var newOrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{suffix}";
        return new OrderNumber(newOrderNumber);
    }
    public static OrderNumber Create(string val)
    {
        var normalizedVal = val?.Trim().ToUpperInvariant()
            ?? throw ExceptionFactory.InvalidRange("Order number is not valid.");

        if (!IsValid(normalizedVal))
            throw ExceptionFactory.InvalidRange("Order number is not valid.");

        return new OrderNumber(normalizedVal);
    }

    public static bool IsValid(string val)
        => val.IsNotNullOrWhiteSpace()
            && val.Length <= 20
            && val.StartsWith("ORD-")
            && CommonRegexes.OrderNumber().IsMatch(val);
}
