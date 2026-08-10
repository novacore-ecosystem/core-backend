namespace NovaCore.Product.Domain.ValueObjects;

/// <summary>A length of time (e.g. warranty coverage) expressed as a value plus unit.</summary>
public sealed class Duration : ValueObject
{
    public int Value { get; }
    public DurationUnit Unit { get; }

    private Duration(int value, DurationUnit unit)
    {
        Value = value;
        Unit = unit;
    }

    public static bool IsValid(int value) => GetValidationError(value) is null;

    public static bool TryCreate(int value, DurationUnit unit, out Duration? duration)
    {
        if (GetValidationError(value) is not null)
        {
            duration = null;
            return false;
        }

        duration = new Duration(value, unit);
        return true;
    }

    public static Duration Create(int value, DurationUnit unit)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new Duration(value, unit);
    }

    private static InvalidArgumentException? GetValidationError(int value)
    {
        if (value <= 0)
            return ExceptionFactory.InvalidRange("Duration must be greater than zero.");

        return null;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Unit;
    }

    public override string ToString() => $"{Value} {Unit}";
}
