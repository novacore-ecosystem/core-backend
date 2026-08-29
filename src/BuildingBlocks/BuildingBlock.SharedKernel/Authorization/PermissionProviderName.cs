namespace NovaCore.BuildingBlock.SharedKernel.Authorization;

/// <summary>
/// Every authorization-context category a permission can be granted to, or a Role can be
/// classified under. Flags so PermissionDefinitionAttribute.Providers can declare a combination
/// (e.g. Role | User) - a single persisted value (Role.ProviderName, PermissionGrant.ProviderName)
/// is always exactly one bit, never a combination or None. Persisted as the stable string from
/// ToName(), never the raw int - see PermissionProviderNameExtensions.
/// </summary>
[Flags]
public enum PermissionProviderName
{
    None = 0,
    Role = 1 << 0,
    User = 1 << 1,
    Client = 1 << 2,
    Guest = 1 << 3,
    ServiceAccount = 1 << 4,
}
