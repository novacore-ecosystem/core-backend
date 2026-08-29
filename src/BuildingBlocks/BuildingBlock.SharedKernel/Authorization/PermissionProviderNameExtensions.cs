namespace NovaCore.BuildingBlock.SharedKernel.Authorization;

/// <summary>
/// Stable string<->enum conversion for PermissionProviderName - the canonical persisted contract
/// (Role.ProviderName/PermissionGrant.ProviderName columns) never depends on enum numeric values,
/// which are free to reorder/insert without a data migration.
/// </summary>
public static class PermissionProviderNameExtensions
{
    public static string ToName(this PermissionProviderName provider) => provider switch
    {
        PermissionProviderName.Role => "Role",
        PermissionProviderName.User => "User",
        PermissionProviderName.Client => "Client",
        PermissionProviderName.Guest => "Guest",
        PermissionProviderName.ServiceAccount => "ServiceAccount",
        _ => throw new ArgumentOutOfRangeException(
            nameof(provider),
            provider,
            "ToName() requires a single provider value, not None or a flag combination."),
    };

    public static PermissionProviderName ParseName(string name) => name switch
    {
        "Role" => PermissionProviderName.Role,
        "User" => PermissionProviderName.User,
        "Client" => PermissionProviderName.Client,
        "Guest" => PermissionProviderName.Guest,
        "ServiceAccount" => PermissionProviderName.ServiceAccount,
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unrecognized permission provider name."),
    };

    /// <summary>True only for a single, named bit - what every persisted ProviderName column
    /// requires. PermissionDefinitionAttribute.Providers is the one legitimate place a
    /// combination/None is allowed.</summary>
    public static bool IsSingleValue(this PermissionProviderName provider)
        => provider is PermissionProviderName.Role
            or PermissionProviderName.User
            or PermissionProviderName.Client
            or PermissionProviderName.Guest
            or PermissionProviderName.ServiceAccount;
}
