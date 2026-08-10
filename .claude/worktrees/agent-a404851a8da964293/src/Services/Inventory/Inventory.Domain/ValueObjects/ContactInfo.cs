using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Inventory.Domain.ValueObjects;

public sealed class ContactInfo : ValueObject
{
    public string ContactName { get; }
    public PhoneNumber? PhoneNumber { get; }
    public Email? Email { get; }

    private ContactInfo(string contactName, PhoneNumber? phoneNumber, Email? email)
    {
        ContactName = contactName;
        PhoneNumber = phoneNumber;
        Email = email;
    }

    public static bool IsValid(string contactName, string? phoneNumber, string? email) =>
        GetValidationError(contactName, phoneNumber, email) is null;

    public static bool TryCreate(
        string contactName,
        string? phoneNumber,
        string? email,
        out ContactInfo? contactInfo)
    {
        if (GetValidationError(contactName, phoneNumber, email) is not null)
        {
            contactInfo = null;
            return false;
        }

        contactInfo = new ContactInfo(
            contactName.Trim(),
            string.IsNullOrWhiteSpace(phoneNumber) ? null : PhoneNumber.Create(phoneNumber),
            string.IsNullOrWhiteSpace(email) ? null : Email.Create(email));
        return true;
    }

    public static ContactInfo Create(string contactName, string? phoneNumber = null, string? email = null)
    {
        var error = GetValidationError(contactName, phoneNumber, email);
        if (error is not null)
            throw error;

        return new ContactInfo(
            contactName.Trim(),
            string.IsNullOrWhiteSpace(phoneNumber) ? null : PhoneNumber.Create(phoneNumber),
            string.IsNullOrWhiteSpace(email) ? null : Email.Create(email));
    }

    private static InvalidArgumentException? GetValidationError(string contactName, string? phoneNumber, string? email)
    {
        if (contactName.IsNullOrWhiteSpace())
            return ExceptionFactory.RequiredNotEmpty("Contact name is required.");

        if (!string.IsNullOrWhiteSpace(phoneNumber) && !PhoneNumber.IsValid(phoneNumber))
            return ExceptionFactory.InvalidFormat("Phone number is not valid.");

        if (!string.IsNullOrWhiteSpace(email) && !Email.IsValid(email))
            return ExceptionFactory.InvalidFormat("Email is not valid.");

        return null;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return ContactName;
        yield return PhoneNumber?.Value ?? string.Empty;
        yield return Email?.Value ?? string.Empty;
    }
}
