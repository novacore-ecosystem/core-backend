using System.Text.RegularExpressions;

namespace NovaCore.Product.Domain.ValueObjects;

/// <summary>Lowercase kebab-case URL slug, shared across every variation of a Product.</summary>
public sealed partial class Slug : StringValueObject
{
    private const int MaxLength = 200;

    private Slug(string value) : base(value) { }

    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out Slug? slug)
    {
        if (GetValidationError(value) is not null)
        {
            slug = null;
            return false;
        }

        slug = new Slug(Normalize(value!));
        return true;
    }

    public static Slug Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new Slug(Normalize(value));
    }

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();

    private static InvalidArgumentException? GetValidationError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ExceptionFactory.RequiredField("Slug cannot be empty.");

        var normalized = Normalize(value);

        if (normalized.Length > MaxLength)
            return ExceptionFactory.ValueTooLarge($"Slug cannot exceed {MaxLength} characters.");

        if (!SlugFormat().IsMatch(normalized))
            return ExceptionFactory.InvalidFormat("Slug must be lowercase kebab-case (letters, digits, hyphens).");

        return null;
    }

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex SlugFormat();
}
