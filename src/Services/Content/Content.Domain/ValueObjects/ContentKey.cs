using System.Text.RegularExpressions;

namespace NovaCore.Content.Domain.ValueObjects;

/// <summary>
/// Lowercase, dot-segmented machine key identifying a ContentType/ContentWorkflowDefinition/
/// ContentTaxonomy (e.g. "article", "news.bulletin") - shared across all three since they're the
/// same concept (a stable, human-assigned key distinct from the row's surrogate Id).
/// </summary>
public sealed partial class ContentKey : StringValueObject
{
    private const int MaxLength = 100;

    private ContentKey(string value) : base(value) { }

    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out ContentKey? key)
    {
        if (GetValidationError(value) is not null)
        {
            key = null;
            return false;
        }

        key = new ContentKey(Normalize(value!));
        return true;
    }

    public static ContentKey Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new ContentKey(Normalize(value));
    }

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();

    private static InvalidArgumentException? GetValidationError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ExceptionFactory.RequiredField("Content key cannot be empty.");

        var normalized = Normalize(value);

        if (normalized.Length > MaxLength)
            return ExceptionFactory.ValueTooLarge($"Content key cannot exceed {MaxLength} characters.");

        if (!KeyFormat().IsMatch(normalized))
            return ExceptionFactory.InvalidFormat("Content key must be lowercase alphanumeric segments separated by dots, underscores, or hyphens.");

        return null;
    }

    [GeneratedRegex(@"^[a-z0-9]+([._-][a-z0-9]+)*$")]
    private static partial Regex KeyFormat();
}
