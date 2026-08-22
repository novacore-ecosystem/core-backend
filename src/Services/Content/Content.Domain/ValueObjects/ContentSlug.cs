using System.Text.RegularExpressions;

namespace NovaCore.Content.Domain.ValueObjects;

/// <summary>Lowercase kebab-case URL slug identifying a Content item.</summary>
public sealed partial class ContentSlug : StringValueObject
{
    private const int MaxLength = 200;

    private ContentSlug(string value) : base(value) { }

    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out ContentSlug? slug)
    {
        if (GetValidationError(value) is not null)
        {
            slug = null;
            return false;
        }

        slug = new ContentSlug(Normalize(value!));
        return true;
    }

    public static ContentSlug Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new ContentSlug(Normalize(value));
    }

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();

    private static InvalidArgumentException? GetValidationError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ExceptionFactory.RequiredField("Content slug cannot be empty.");

        var normalized = Normalize(value);

        if (normalized.Length > MaxLength)
            return ExceptionFactory.ValueTooLarge($"Content slug cannot exceed {MaxLength} characters.");

        if (!SlugFormat().IsMatch(normalized))
            return ExceptionFactory.InvalidFormat("Content slug must be lowercase kebab-case (letters, digits, hyphens).");

        return null;
    }

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex SlugFormat();
}
