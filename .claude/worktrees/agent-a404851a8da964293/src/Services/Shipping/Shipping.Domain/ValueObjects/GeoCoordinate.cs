namespace NovaCore.Shipping.Domain.ValueObjects;

/// <summary>
/// WGS-84 latitude/longitude pair. Used for a VerifiedShippingAddress' resolved location and for
/// each TransportationTracking ping. Stored as two columns via OwnsOne - never as a PostGIS type,
/// since nothing in this service does geospatial querying (routing/TMS is explicitly out of scope).
/// </summary>
public sealed class GeoCoordinate : ValueObject
{
    public decimal Latitude { get; }
    public decimal Longitude { get; }

    private GeoCoordinate(decimal latitude, decimal longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public static GeoCoordinate Create(decimal latitude, decimal longitude)
    {
        if (latitude is < -90m or > 90m)
            throw ExceptionFactory.InvalidRange("Latitude must be between -90 and 90.");

        if (longitude is < -180m or > 180m)
            throw ExceptionFactory.InvalidRange("Longitude must be between -180 and 180.");

        return new GeoCoordinate(latitude, longitude);
    }

    public static bool IsValid(decimal latitude, decimal longitude)
        => latitude is >= -90m and <= 90m && longitude is >= -180m and <= 180m;

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Latitude;
        yield return Longitude;
    }

    public override string ToString() => $"{Latitude},{Longitude}";
}
