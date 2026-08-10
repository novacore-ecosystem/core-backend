namespace NovaCore.Inventory.Domain.ValueObjects;

public sealed class HumidityRange : ValueObject
{
    public decimal Minimum { get; }
    public decimal Maximum { get; }

    private HumidityRange(decimal minimum, decimal maximum)
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    public static bool IsValid(decimal minimum, decimal maximum) =>
        GetValidationError(minimum, maximum) is null;

    public static bool TryCreate(decimal minimum, decimal maximum, out HumidityRange? humidityRange)
    {
        if (GetValidationError(minimum, maximum) is not null)
        {
            humidityRange = null;
            return false;
        }

        humidityRange = new HumidityRange(minimum, maximum);
        return true;
    }

    public static HumidityRange Create(decimal minimum, decimal maximum)
    {
        var error = GetValidationError(minimum, maximum);
        if (error is not null)
            throw error;

        return new HumidityRange(minimum, maximum);
    }

    private static InvalidArgumentException? GetValidationError(decimal minimum, decimal maximum)
    {
        if (minimum is < 0 or > 100)
            return ExceptionFactory.InvalidRange("Minimum humidity must be between 0 and 100.");

        if (maximum is < 0 or > 100)
            return ExceptionFactory.InvalidRange("Maximum humidity must be between 0 and 100.");

        if (minimum > maximum)
            return ExceptionFactory.InvalidRange("Minimum humidity must not exceed maximum humidity.");

        return null;
    }

    public bool Contains(decimal humidity) =>
        humidity >= Minimum && humidity <= Maximum;

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Minimum;
        yield return Maximum;
    }
}
