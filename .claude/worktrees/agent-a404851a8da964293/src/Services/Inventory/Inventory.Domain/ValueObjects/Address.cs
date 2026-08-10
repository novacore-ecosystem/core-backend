namespace NovaCore.Inventory.Domain.ValueObjects;

public sealed class Address : ValueObject
{
    public string Country { get; }
    public string StateOrProvince { get; }
    public string City { get; }
    public string District { get; }
    public string Ward { get; }
    public string Street { get; }
    public string PostalCode { get; }

    private Address(
        string country,
        string stateOrProvince,
        string city,
        string district,
        string ward,
        string street,
        string postalCode)
    {
        Country = country;
        StateOrProvince = stateOrProvince;
        City = city;
        District = district;
        Ward = ward;
        Street = street;
        PostalCode = postalCode;
    }

    public static bool IsValid(string country, string city, string street) =>
        GetValidationError(country, city, street) is null;

    public static bool TryCreate(
        string country,
        string stateOrProvince,
        string city,
        string district,
        string ward,
        string street,
        string postalCode,
        out Address? address)
    {
        if (GetValidationError(country, city, street) is not null)
        {
            address = null;
            return false;
        }

        address = new Address(
            country.Trim(),
            stateOrProvince.Trim(),
            city.Trim(),
            district.Trim(),
            ward.Trim(),
            street.Trim(),
            postalCode.Trim());
        return true;
    }

    public static Address Create(
        string country,
        string stateOrProvince,
        string city,
        string district,
        string ward,
        string street,
        string postalCode)
    {
        var error = GetValidationError(country, city, street);
        if (error is not null)
            throw error;

        return new Address(
            country.Trim(),
            stateOrProvince.Trim(),
            city.Trim(),
            district.Trim(),
            ward.Trim(),
            street.Trim(),
            postalCode.Trim());
    }

    private static InvalidArgumentException? GetValidationError(string country, string city, string street)
    {
        if (country.IsNullOrWhiteSpace())
            return ExceptionFactory.RequiredNotEmpty("Country is required.");

        if (city.IsNullOrWhiteSpace())
            return ExceptionFactory.RequiredNotEmpty("City is required.");

        if (street.IsNullOrWhiteSpace())
            return ExceptionFactory.RequiredNotEmpty("Street is required.");

        return null;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Country;
        yield return StateOrProvince;
        yield return City;
        yield return District;
        yield return Ward;
        yield return Street;
        yield return PostalCode;
    }
}
