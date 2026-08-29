namespace NovaCore.BuildingBlock.SharedKernel.Authorization;

/// <summary>
/// Decorates a Permissions.cs const string with its structural authorization metadata - which
/// provider categories are allowed to receive a grant for it. Discovered by PermissionRegistry at
/// startup. Deliberately carries nothing else (no display name/description/localization) - that
/// metadata belongs to the DB-backed PermissionDefinition, not this code-owned contract.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class PermissionDefinitionAttribute : Attribute
{
    // Public read-write, not init-only or positional - C#'s named-attribute-argument syntax
    // (Providers = ...) requires a plain settable property.
    public PermissionProviderName Providers { get; set; }
}
