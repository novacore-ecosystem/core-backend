using NovaCore.BuildingBlock.Domain.Abstractions;
using NovaCore.BuildingBlock.Domain.Exceptions;

namespace NovaCore.BuildingBlock.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Value { get; }

    private Money(decimal val)
    {
        Value = val;
    }

    public static Money Create(decimal val)
    {
        if (!IsValid(val))
            throw ExceptionFactory.InvalidRange("Money must be greater than or equal to zero.");

        return new Money(val);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static bool IsValid(decimal val)
    {
        return val >= 0;
    }
}
