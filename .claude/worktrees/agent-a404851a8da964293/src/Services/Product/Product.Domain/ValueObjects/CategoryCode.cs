using System.Text.RegularExpressions;

namespace NovaCore.Product.Domain.ValueObjects;

/// <summary>Unique business code identifying a ProductCategory.</summary>
public sealed partial class CategoryCode : StringValueObject
{
    private const int MaxLength = 50;

    private CategoryCode(string value) : base(value) { }

    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out CategoryCode? code)
    {
        if (GetValidationError(value) is not null)
        {
            code = null;
            return false;
        }

        code = new CategoryCode(Normalize(value!));
        return true;
    }

    public static CategoryCode Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new CategoryCode(Normalize(value));
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();

    private static InvalidArgumentException? GetValidationError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ExceptionFactory.RequiredField("Category code cannot be empty.");

        var normalized = Normalize(value);

        if (normalized.Length > MaxLength)
            return ExceptionFactory.ValueTooLarge($"Category code cannot exceed {MaxLength} characters.");

        if (!CodeFormat().IsMatch(normalized))
            return ExceptionFactory.InvalidFormat("Category code may only contain letters, digits, and hyphens.");

        return null;
    }

    [GeneratedRegex("^[A-Z0-9-]+$")]
    private static partial Regex CodeFormat();
}
