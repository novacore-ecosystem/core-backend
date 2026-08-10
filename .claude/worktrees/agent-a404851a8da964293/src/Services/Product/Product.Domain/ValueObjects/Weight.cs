namespace NovaCore.Product.Domain.ValueObjects;

public sealed class Weight : ValueObject
{
    public decimal Value { get; }
    public WeightUnit Unit { get; }

    private Weight(decimal value, WeightUnit unit)
    {
        if (value < 0)
            throw ExceptionFactory.InvalidRange("Weight cannot be negative.");

        Value = value;
        Unit = unit;
    }

    public static Weight Create(decimal value, WeightUnit unit)
        => new(value, unit);

    public static Weight FromKilograms(decimal value)
        => new(value, WeightUnit.Kilogram);

    public static Weight FromGrams(decimal value)
        => new(value, WeightUnit.Gram);

    public static Weight FromPounds(decimal value)
        => new(value, WeightUnit.Pound);

    public static Weight FromOunces(decimal value)
        => new(value, WeightUnit.Ounce);

    public decimal ToKilograms() => ToGrams() / 1000m;

    public decimal ToGrams() =>
        Unit switch
        {
            WeightUnit.Kilogram => Value * 1000m,
            WeightUnit.Gram => Value,
            WeightUnit.Pound => Value * 453.59237m,
            WeightUnit.Ounce => Value * 28.349523125m,
            _ => throw new NotSupportedException($"Unsupported unit: {Unit}")
        };

    public decimal ToPounds() => ToGrams() / 453.59237m;

    public decimal ToOunces() => ToGrams() / 28.349523125m;

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Unit;
    }

    public override string ToString() => $"{Value} {Unit}";
}
