using System.Text.RegularExpressions;

namespace NovaCore.Auth.Domain.ValueObjects;

/// <summary>
/// Lowercase snake_case internal identifier for a Role (e.g. "warehouse_manager"). Language-independent
/// and never translated - the localized display text lives on RoleTranslation.DisplayName. Mirrors
/// User.Domain's RoleKey shape; kept separate because Auth's Role is JWT-claim authority, not the
/// business-role model User.Domain owns.
/// </summary>
public sealed partial class RoleCode : StringValueObject
{
    private const int MaxLength = 100;

    private RoleCode(string value) : base(value) { }

    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out RoleCode? roleCode)
    {
        if (GetValidationError(value) is not null)
        {
            roleCode = null;
            return false;
        }

        roleCode = new RoleCode(Normalize(value!));
        return true;
    }

    public static RoleCode Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new RoleCode(Normalize(value));
    }

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();

    private static InvalidArgumentException? GetValidationError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ExceptionFactory.RequiredField("Role code cannot be empty.");

        var normalized = Normalize(value);

        if (normalized.Length > MaxLength)
            return ExceptionFactory.ValueTooLarge($"Role code cannot exceed {MaxLength} characters.");

        if (!RoleCodeFormat().IsMatch(normalized))
            return ExceptionFactory.InvalidFormat("Role code must be lowercase snake_case (letters, digits, underscores), e.g. \"warehouse_manager\".");

        return null;
    }

    [GeneratedRegex("^[a-z][a-z0-9]*(_[a-z0-9]+)*$")]
    private static partial Regex RoleCodeFormat();
}
