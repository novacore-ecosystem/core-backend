using System.Text.RegularExpressions;

namespace NovaCore.Auth.Domain.ValueObjects;

/// <summary>
/// Lowercase snake_case internal identifier for a PermissionGroup (e.g. "product_management").
/// Mirrors RoleCode's format-validated StringValueObject shape.
/// </summary>
public sealed partial class PermissionGroupCode : StringValueObject
{
    private const int MaxLength = 100;

    private PermissionGroupCode(string value) : base(value) { }

    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out PermissionGroupCode? code)
    {
        if (GetValidationError(value) is not null)
        {
            code = null;
            return false;
        }

        code = new PermissionGroupCode(Normalize(value!));
        return true;
    }

    public static PermissionGroupCode Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new PermissionGroupCode(Normalize(value));
    }

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();

    private static InvalidArgumentException? GetValidationError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ExceptionFactory.RequiredField("Permission group code cannot be empty.");

        var normalized = Normalize(value);

        if (normalized.Length > MaxLength)
            return ExceptionFactory.ValueTooLarge($"Permission group code cannot exceed {MaxLength} characters.");

        if (!PermissionGroupCodeFormat().IsMatch(normalized))
            return ExceptionFactory.InvalidFormat("Permission group code must be lowercase snake_case (letters, digits, underscores), e.g. \"product_management\".");

        return null;
    }

    [GeneratedRegex("^[a-z][a-z0-9]*(_[a-z0-9]+)*$")]
    private static partial Regex PermissionGroupCodeFormat();
}
