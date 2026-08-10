namespace NovaCore.User.Domain.ValueObjects;

/// <summary>
/// A person's given/middle/family name plus a display FullName. FullName defaults to the
/// concatenation of the three parts but can be overridden, since simple concatenation does not
/// hold for every naming culture (e.g. family-name-first order).
/// </summary>
public sealed class PersonalName : ValueObject
{
    public string FirstName { get; }
    public string? MiddleName { get; }
    public string LastName { get; }
    public string FullName { get; }

    private PersonalName(string firstName, string? middleName, string lastName, string fullName)
    {
        FirstName = firstName;
        MiddleName = middleName;
        LastName = lastName;
        FullName = fullName;
    }

    public static PersonalName Create(string firstName, string? middleName, string lastName, string? fullName = null)
    {
        ValidateNamePart(firstName, "First name");
        ValidateNamePart(lastName, "Last name");

        var resolvedFullName = string.IsNullOrWhiteSpace(fullName)
            ? ComposeFullName(firstName, middleName, lastName)
            : fullName.Trim();

        return new PersonalName(firstName.Trim(), middleName?.Trim(), lastName.Trim(), resolvedFullName);
    }

    private static string ComposeFullName(string firstName, string? middleName, string lastName)
        => string.Join(' ', new[] { firstName, middleName, lastName }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part!.Trim()));

    private static void ValidateNamePart(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw ExceptionFactory.RequiredField($"{fieldName} cannot be empty.");
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return MiddleName ?? string.Empty;
        yield return LastName;
        yield return FullName;
    }

    public override string ToString() => FullName;
}
