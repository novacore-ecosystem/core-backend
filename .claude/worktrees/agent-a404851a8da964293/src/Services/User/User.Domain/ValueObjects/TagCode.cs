using System.Text.RegularExpressions;

namespace NovaCore.User.Domain.ValueObjects;

/// <summary>Uppercase snake_case internal key for a UserTag (e.g. "VIP_CUSTOMER"). Language-independent
/// and never translated - the localized display text lives on UserTagTranslation.DisplayName.</summary>
public sealed partial class TagCode : StringValueObject
{
    private const int MaxLength = 100;

    private TagCode(string value) : base(value) { }

    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out TagCode? tagCode)
    {
        if (GetValidationError(value) is not null)
        {
            tagCode = null;
            return false;
        }

        tagCode = new TagCode(Normalize(value!));
        return true;
    }

    public static TagCode Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new TagCode(Normalize(value));
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();

    private static InvalidArgumentException? GetValidationError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ExceptionFactory.RequiredField("Tag code cannot be empty.");

        var normalized = Normalize(value);

        if (normalized.Length > MaxLength)
            return ExceptionFactory.ValueTooLarge($"Tag code cannot exceed {MaxLength} characters.");

        if (!TagCodeFormat().IsMatch(normalized))
            return ExceptionFactory.InvalidFormat("Tag code must be uppercase snake_case (letters, digits, underscores), e.g. \"VIP_CUSTOMER\".");

        return null;
    }

    [GeneratedRegex("^[A-Z][A-Z0-9]*(_[A-Z0-9]+)*$")]
    private static partial Regex TagCodeFormat();
}
