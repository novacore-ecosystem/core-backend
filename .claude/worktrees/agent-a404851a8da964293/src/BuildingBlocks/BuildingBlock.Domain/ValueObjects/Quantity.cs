using NovaCore.BuildingBlock.Domain.Abstractions;
using NovaCore.BuildingBlock.Domain.Exceptions;

namespace NovaCore.BuildingBlock.Domain.ValueObjects;

public sealed class Quantity : ValueObject
{
    public int Value { get; }

    private Quantity(int val)
    {
        Value = val;
    }

    public static Quantity Create(int val)
    {
        if (!IsValid(val))
            throw ExceptionFactory.InvalidRange("Quantity cannot be negative.");

        return new Quantity(val);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static bool IsValid(int quantity)
    {
        return quantity >= 0;
    }
}
