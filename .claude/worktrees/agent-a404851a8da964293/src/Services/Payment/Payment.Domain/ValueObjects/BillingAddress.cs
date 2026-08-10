namespace NovaCore.Payment.Domain.ValueObjects;

/// <summary>
/// Postal/administrative billing address for a BillingProfile. PaymentService-local, following
/// the same per-bounded-context precedent already set by Address being duplicated in
/// Inventory/User rather than shared.
/// </summary>
public sealed class BillingAddress : ValueObject
{
    public string Country { get; }
    public string? State { get; }
    public string? City { get; }
    public string Street { get; }
    public string? PostalCode { get; }

    private BillingAddress(string country, string? state, string? city, string street, string? postalCode)
    {
        Country = country;
        State = state;
        City = city;
        Street = street;
        PostalCode = postalCode;
    }

    public static BillingAddress Create(
        string country,
        string street,
        string? state = null,
        string? city = null,
        string? postalCode = null)
    {
        if (string.IsNullOrWhiteSpace(country))
            throw ExceptionFactory.RequiredField("Country cannot be empty.");

        if (string.IsNullOrWhiteSpace(street))
            throw ExceptionFactory.RequiredField("Street cannot be empty.");

        return new BillingAddress(country.Trim(), state?.Trim(), city?.Trim(), street.Trim(), postalCode?.Trim());
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Country;
        yield return State ?? string.Empty;
        yield return City ?? string.Empty;
        yield return Street;
        yield return PostalCode ?? string.Empty;
    }

    public override string ToString()
        => string.Join(", ", new[] { Street, City, State, Country, PostalCode }
            .Where(part => !string.IsNullOrWhiteSpace(part)));
}
