namespace NovaCore.User.Domain.ValueObjects;

/// <summary>Geographic coordinates pinning a UserAddress on a map, e.g. for delivery routing.</summary>
public sealed class GeoLocation : ValueObject
{
    private const decimal MinLatitude = -90m;
    private const decimal MaxLatitude = 90m;
    private const decimal MinLongitude = -180m;
    private const decimal MaxLongitude = 180m;

    public decimal Latitude { get; }
    public decimal Longitude { get; }

    private GeoLocation(decimal latitude, decimal longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public static GeoLocation Create(decimal latitude, decimal longitude)
    {
        if (latitude < MinLatitude || latitude > MaxLatitude)
            throw ExceptionFactory.InvalidRange($"Latitude must be between {MinLatitude} and {MaxLatitude}.");

        if (longitude < MinLongitude || longitude > MaxLongitude)
            throw ExceptionFactory.InvalidRange($"Longitude must be between {MinLongitude} and {MaxLongitude}.");

        return new GeoLocation(latitude, longitude);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Latitude;
        yield return Longitude;
    }

    public override string ToString() => $"{Latitude}, {Longitude}";
}
