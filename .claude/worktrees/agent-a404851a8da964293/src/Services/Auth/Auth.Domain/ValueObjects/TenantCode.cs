using System.Text.RegularExpressions;

namespace NovaCore.Auth.Domain.ValueObjects;

/// <summary>
/// Lowercase snake_case unique identifier for a Tenant (e.g. "acme_corp"). Mirrors
/// RoleCode/PositionCode's shape - the stable identifier tenant-scoped lookups key off of.
/// </summary>
public sealed partial class TenantCode : StringValueObject
{
    private const int MaxLength = 100;

    private TenantCode(string value) : base(value) { }

    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out TenantCode? tenantCode)
    {
        if (GetValidationError(value) is not null)
        {
            tenantCode = null;
            return false;
        }

        tenantCode = new TenantCode(Normalize(value!));
        return true;
    }

    public static TenantCode Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new TenantCode(Normalize(value));
    }

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();

    private static InvalidArgumentException? GetValidationError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ExceptionFactory.RequiredField("Tenant code cannot be empty.");

        var normalized = Normalize(value);

        if (normalized.Length > MaxLength)
            return ExceptionFactory.ValueTooLarge($"Tenant code cannot exceed {MaxLength} characters.");

        if (!TenantCodeFormat().IsMatch(normalized))
            return ExceptionFactory.InvalidFormat("Tenant code must be lowercase snake_case (letters, digits, underscores), e.g. \"acme_corp\".");

        return null;
    }

    [GeneratedRegex("^[a-z][a-z0-9]*(_[a-z0-9]+)*$")]
    private static partial Regex TenantCodeFormat();
}
