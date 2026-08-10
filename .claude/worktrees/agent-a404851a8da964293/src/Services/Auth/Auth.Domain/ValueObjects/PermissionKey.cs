using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.Auth.Domain.ValueObjects;

/// <summary>
/// Permission identifier baked into JWT claims and checked by every service's
/// RequirePermissions(...) endpoint declaration. Permission keys are code-first, not user input -
/// declared once in Permissions - so validation is set membership against Permissions.SupportedValues,
/// not a runtime format/regex check.
/// </summary>
public sealed class PermissionKey : StringValueObject
{
    private PermissionKey(string value) : base(value) { }

    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out PermissionKey? permissionKey)
    {
        if (GetValidationError(value) is not null)
        {
            permissionKey = null;
            return false;
        }

        permissionKey = new PermissionKey(Normalize(value!));
        return true;
    }

    public static PermissionKey Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new PermissionKey(Normalize(value));
    }

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();

    private static InvalidArgumentException? GetValidationError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ExceptionFactory.RequiredField("Permission key cannot be empty.");

        if (!Permissions.SupportedValues.Contains(Normalize(value)))
            return ExceptionFactory.InvalidRange($"\"{value}\" is not a supported permission key.");

        return null;
    }
}
