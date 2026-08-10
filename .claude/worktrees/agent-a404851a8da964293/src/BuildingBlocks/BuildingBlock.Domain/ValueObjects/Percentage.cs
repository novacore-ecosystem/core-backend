using NovaCore.BuildingBlock.Domain.Abstractions;
using NovaCore.BuildingBlock.Domain.Exceptions;

namespace NovaCore.BuildingBlock.Domain.ValueObjects;

public sealed class Percentage : ValueObject
{
    public decimal Value { get; }

    private Percentage(decimal value)
    {
        Value = value;
    }

    public static Percentage Create(decimal value)
    {
        if (value < 0 || value > 100)
            throw ExceptionFactory.InvalidRange("Percentage must be between 0 and 100.");

        return new Percentage(value);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => $"{Value}%";
}
