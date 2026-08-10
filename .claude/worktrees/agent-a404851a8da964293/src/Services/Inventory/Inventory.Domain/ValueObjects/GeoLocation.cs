namespace NovaCore.Inventory.Domain.ValueObjects;

public sealed class GeoLocation : ValueObject
{
    public decimal Latitude { get; }
    public decimal Longitude { get; }

    private GeoLocation(decimal latitude, decimal longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public static bool IsValid(decimal latitude, decimal longitude) =>
        GetValidationError(latitude, longitude) is null;

    public static bool TryCreate(decimal latitude, decimal longitude, out GeoLocation? geoLocation)
    {
        if (GetValidationError(latitude, longitude) is not null)
        {
            geoLocation = null;
            return false;
        }

        geoLocation = new GeoLocation(latitude, longitude);
        return true;
    }

    public static GeoLocation Create(decimal latitude, decimal longitude)
    {
        var error = GetValidationError(latitude, longitude);
        if (error is not null)
            throw error;

        return new GeoLocation(latitude, longitude);
    }

    private static InvalidArgumentException? GetValidationError(decimal latitude, decimal longitude)
    {
        if (latitude is < -90 or > 90)
            return ExceptionFactory.InvalidRange("Latitude must be between -90 and 90.");

        if (longitude is < -180 or > 180)
            return ExceptionFactory.InvalidRange("Longitude must be between -180 and 180.");

        return null;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Latitude;
        yield return Longitude;
    }
}
