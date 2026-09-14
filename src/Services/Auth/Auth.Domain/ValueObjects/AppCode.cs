using System.Text.RegularExpressions;

namespace NovaCore.Auth.Domain.ValueObjects;

/// <summary>
/// Lowercase snake_case stable identifier for an App (e.g. "storefront_web"). The frontend
/// hardcodes this before starting the authentication flow, so it is never regenerated once
/// issued. Mirrors RoleCode/TenantCode's shape.
/// </summary>
public sealed partial class AppCode : StringValueObject
{
    private const int MaxLength = 100;

    private AppCode(string value) : base(value) { }

    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out AppCode? appCode)
    {
        if (GetValidationError(value) is not null)
        {
            appCode = null;
            return false;
        }

        appCode = new AppCode(Normalize(value!));
        return true;
    }

    public static AppCode Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new AppCode(Normalize(value));
    }

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();

    private static InvalidArgumentException? GetValidationError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ExceptionFactory.RequiredField("App code cannot be empty.");

        var normalized = Normalize(value);

        if (normalized.Length > MaxLength)
            return ExceptionFactory.ValueTooLarge($"App code cannot exceed {MaxLength} characters.");

        if (!AppCodeFormat().IsMatch(normalized))
            return ExceptionFactory.InvalidFormat("App code must be lowercase snake_case (letters, digits, underscores), e.g. \"storefront_web\".");

        return null;
    }

    [GeneratedRegex("^[a-z][a-z0-9]*(_[a-z0-9]+)*$")]
    private static partial Regex AppCodeFormat();
}
