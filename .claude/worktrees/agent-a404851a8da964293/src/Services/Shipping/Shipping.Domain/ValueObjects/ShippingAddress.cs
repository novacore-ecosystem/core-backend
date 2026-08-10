namespace NovaCore.Shipping.Domain.ValueObjects;

/// <summary>
/// Postal/administrative address used by every shipping-side party (sender, receiver, pickup
/// point, provider office). Deliberately named ShippingAddress rather than Address: this is a
/// ShippingService-local Value Object, following the same per-bounded-context precedent
/// User/Inventory/Payment already set - it is not, and must not become, a shared type.
/// </summary>
public sealed class ShippingAddress : ValueObject
{
    public string Country { get; }
    public string? Province { get; }
    public string? District { get; }
    public string? Ward { get; }
    public string Street { get; }
    public string? PostalCode { get; }

    private ShippingAddress(string country, string? province, string? district, string? ward, string street, string? postalCode)
    {
        Country = country;
        Province = province;
        District = district;
        Ward = ward;
        Street = street;
        PostalCode = postalCode;
    }

    public static ShippingAddress Create(
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

        return new ShippingAddress(
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
