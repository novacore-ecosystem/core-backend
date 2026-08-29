namespace NovaCore.Auth.Domain.Enums;

/// <summary>
/// Definition lifecycle only - separate from whether a PermissionGrant referencing this key still
/// exists. Deprecated/Disabled does not cascade to grants; see PermissionDefinition.Deprecate/
/// Disable class doc.
/// </summary>
public enum PermissionDefinitionStatus : byte
{
    Active = 0,
    Deprecated = 1,
    Disabled = 2,
}
