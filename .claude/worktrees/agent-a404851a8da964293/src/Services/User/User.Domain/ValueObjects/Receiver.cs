namespace NovaCore.User.Domain.ValueObjects;

/// <summary>The person a shipment at a UserAddress is handed to, distinct from the account owner.</summary>
public sealed class Receiver : ValueObject
{
    public string FullName { get; }
    public string PhoneNumber { get; }
    public string? Company { get; }

    private Receiver(string fullName, string phoneNumber, string? company)
    {
        FullName = fullName;
        PhoneNumber = phoneNumber;
        Company = company;
    }

    public static Receiver Create(string fullName, string phoneNumber, string? company = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw ExceptionFactory.RequiredField("Receiver full name cannot be empty.");

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw ExceptionFactory.RequiredField("Receiver phone number cannot be empty.");

        return new Receiver(fullName.Trim(), phoneNumber.Trim(), company?.Trim());
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return FullName;
        yield return PhoneNumber;
        yield return Company ?? string.Empty;
    }

    public override string ToString() => $"{FullName} ({PhoneNumber})";
}
