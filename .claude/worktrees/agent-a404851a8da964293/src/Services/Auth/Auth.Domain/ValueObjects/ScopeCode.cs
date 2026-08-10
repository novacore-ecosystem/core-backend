using System.Text.RegularExpressions;

namespace NovaCore.Auth.Domain.ValueObjects;

/// <summary>
/// Lowercase snake_case identifier for a Scope (e.g. "branch_hanoi"), unique within its
/// owning Tenant (see ScopeConfig's composite unique index). Mirrors RoleCode/PositionCode's shape.
/// </summary>
public sealed partial class ScopeCode : StringValueObject
{
    private const int MaxLength = 100;

    private ScopeCode(string value) : base(value) { }

    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out ScopeCode? scopeCode)
    {
        if (GetValidationError(value) is not null)
        {
            scopeCode = null;
            return false;
        }

        scopeCode = new ScopeCode(Normalize(value!));
        return true;
    }

    public static ScopeCode Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new ScopeCode(Normalize(value));
    }

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();

    private static InvalidArgumentException? GetValidationError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ExceptionFactory.RequiredField("Scope code cannot be empty.");

        var normalized = Normalize(value);

        if (normalized.Length > MaxLength)
            return ExceptionFactory.ValueTooLarge($"Scope code cannot exceed {MaxLength} characters.");

        if (!ScopeCodeFormat().IsMatch(normalized))
            return ExceptionFactory.InvalidFormat("Scope code must be lowercase snake_case (letters, digits, underscores), e.g. \"branch_hanoi\".");

        return null;
    }

    [GeneratedRegex("^[a-z][a-z0-9]*(_[a-z0-9]+)*$")]
    private static partial Regex ScopeCodeFormat();
}
