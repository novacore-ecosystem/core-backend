namespace NovaCore.Inventory.Domain.ValueObjects;

public sealed class WarehouseCapacity : ValueObject
{
    public decimal MaximumWeight { get; }
    public decimal MaximumVolume { get; }
    public int MaximumPallets { get; }
    public int MaximumLocations { get; }

    private WarehouseCapacity(
        decimal maximumWeight,
        decimal maximumVolume,
        int maximumPallets,
        int maximumLocations)
    {
        MaximumWeight = maximumWeight;
        MaximumVolume = maximumVolume;
        MaximumPallets = maximumPallets;
        MaximumLocations = maximumLocations;
    }

    public static bool IsValid(
        decimal maximumWeight,
        decimal maximumVolume,
        int maximumPallets,
        int maximumLocations) =>
        GetValidationError(maximumWeight, maximumVolume, maximumPallets, maximumLocations) is null;

    public static bool TryCreate(
        decimal maximumWeight,
        decimal maximumVolume,
        int maximumPallets,
        int maximumLocations,
        out WarehouseCapacity? warehouseCapacity)
    {
        if (GetValidationError(maximumWeight, maximumVolume, maximumPallets, maximumLocations) is not null)
        {
            warehouseCapacity = null;
            return false;
        }

        warehouseCapacity = new WarehouseCapacity(maximumWeight, maximumVolume, maximumPallets, maximumLocations);
        return true;
    }

    public static WarehouseCapacity Create(
        decimal maximumWeight,
        decimal maximumVolume,
        int maximumPallets,
        int maximumLocations)
    {
        var error = GetValidationError(maximumWeight, maximumVolume, maximumPallets, maximumLocations);
        if (error is not null)
            throw error;

        return new WarehouseCapacity(maximumWeight, maximumVolume, maximumPallets, maximumLocations);
    }

    private static InvalidArgumentException? GetValidationError(
        decimal maximumWeight,
        decimal maximumVolume,
        int maximumPallets,
        int maximumLocations)
    {
        if (maximumWeight < 0)
            return ExceptionFactory.InvalidRange("Maximum weight must not be negative.");

        if (maximumVolume < 0)
            return ExceptionFactory.InvalidRange("Maximum volume must not be negative.");

        if (maximumPallets < 0)
            return ExceptionFactory.InvalidRange("Maximum pallets must not be negative.");

        if (maximumLocations < 0)
            return ExceptionFactory.InvalidRange("Maximum locations must not be negative.");

        return null;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return MaximumWeight;
        yield return MaximumVolume;
        yield return MaximumPallets;
        yield return MaximumLocations;
    }
}
