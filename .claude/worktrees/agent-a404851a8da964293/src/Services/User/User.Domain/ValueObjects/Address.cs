namespace NovaCore.User.Domain.ValueObjects;

/// <summary>Postal/administrative address, shared by every UserAddress regardless of AddressType.</summary>
public sealed class Address : ValueObject
{
    public string Country { get; }
    public string? Province { get; }
    public string? District { get; }
    public string? Ward { get; }
    public string Street { get; }
    public string? PostalCode { get; }

    private Address(string country, string? province, string? district, string? ward, string street, string? postalCode)
    {
        Country = country;
        Province = province;
        District = district;
        Ward = ward;
        Street = street;
        PostalCode = postalCode;
    }

    public static Address Create(
        string country,
        string street,
        string? province = null,
        string? district = null,
        string? ward = null,
        string? postalCode = null)
    {
        if (string.IsNullOrWhiteSpace(country))
            throw ExceptionFactory.RequiredField("Country cannot be empty.");

        if (string.IsNullOrWhiteSpace(street))
            throw ExceptionFactory.RequiredField("Street cannot be empty.");

        return new Address(
            country.Trim(),
            province?.Trim(),
            district?.Trim(),
            ward?.Trim(),
            street.Trim(),
            postalCode?.Trim());
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Country;
        yield return Province ?? string.Empty;
        yield return District ?? string.Empty;
        yield return Ward ?? string.Empty;
        yield return Street;
        yield return PostalCode ?? string.Empty;
    }

    public override string ToString()
        => string.Join(", ", new[] { Street, Ward, District, Province, Country, PostalCode }
            .Where(part => !string.IsNullOrWhiteSpace(part)));
}
