namespace NovaCore.Inventory.Domain.ValueObjects;

public sealed class TemperatureRange : ValueObject
{
    public decimal Minimum { get; }
    public decimal Maximum { get; }

    private TemperatureRange(decimal minimum, decimal maximum)
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    public static bool IsValid(decimal minimum, decimal maximum) =>
        GetValidationError(minimum, maximum) is null;

    public static bool TryCreate(decimal minimum, decimal maximum, out TemperatureRange? temperatureRange)
    {
        if (GetValidationError(minimum, maximum) is not null)
        {
            temperatureRange = null;
            return false;
        }

        temperatureRange = new TemperatureRange(minimum, maximum);
        return true;
    }

    public static TemperatureRange Create(decimal minimum, decimal maximum)
    {
        var error = GetValidationError(minimum, maximum);
        if (error is not null)
            throw error;

        return new TemperatureRange(minimum, maximum);
    }

    private static InvalidArgumentException? GetValidationError(decimal minimum, decimal maximum)
    {
        if (minimum > maximum)
            return ExceptionFactory.InvalidRange("Minimum temperature must not exceed maximum temperature.");

        return null;
    }

    public bool Contains(decimal temperature) =>
        temperature >= Minimum && temperature <= Maximum;

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Minimum;
        yield return Maximum;
    }
}
