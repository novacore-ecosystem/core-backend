using System.Text.RegularExpressions;

namespace NovaCore.User.Domain.ValueObjects;

/// <summary>Lowercase snake_case internal key for a UserRole (e.g. "warehouse_manager"). Language-independent,
/// never translated, and never changed after creation - the localized display text lives on
/// UserRoleTranslation.DisplayName. Mirrors TagCode's format-validated StringValueObject shape.</summary>
public sealed partial class RoleKey : StringValueObject
{
    private const int MaxLength = 100;

    private RoleKey(string value) : base(value) { }

    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out RoleKey? roleKey)
    {
        if (GetValidationError(value) is not null)
        {
            roleKey = null;
            return false;
        }

        roleKey = new RoleKey(Normalize(value!));
        return true;
    }

    public static RoleKey Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new RoleKey(Normalize(value));
    }

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();

    private static InvalidArgumentException? GetValidationError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ExceptionFactory.RequiredField("Role key cannot be empty.");

        var normalized = Normalize(value);

        if (normalized.Length > MaxLength)
            return ExceptionFactory.ValueTooLarge($"Role key cannot exceed {MaxLength} characters.");

        if (!RoleKeyFormat().IsMatch(normalized))
            return ExceptionFactory.InvalidFormat("Role key must be lowercase snake_case (letters, digits, underscores), e.g. \"warehouse_manager\".");

        return null;
    }

    [GeneratedRegex("^[a-z][a-z0-9]*(_[a-z0-9]+)*$")]
    private static partial Regex RoleKeyFormat();
}
