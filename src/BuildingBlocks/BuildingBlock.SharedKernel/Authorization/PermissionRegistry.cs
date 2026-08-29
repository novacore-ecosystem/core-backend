using System.Collections.Frozen;
using System.Reflection;

using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.BuildingBlock.SharedKernel.Authorization;

public sealed record PermissionDefinitionInfo(string Key, PermissionProviderName AllowedProviders);

/// <summary>
/// Immutable, in-memory catalog of every permission key the platform recognizes, discovered once
/// via reflection over Permissions.cs's [PermissionDefinition]-attributed const fields. This is the
/// code-owned source of permission existence and structural provider applicability - lookups never
/// touch PostgreSQL (see docs/services/auth-service.md's authorization-foundation phases).
/// Instance is a lazy static singleton so Auth.Domain (PermissionKey.Create) can use it without a
/// DI dependency; the same instance is also registered as a DI singleton for constructor injection
/// (grant validation, DbMigrator sync) - see AddSharedKernelAuthorization.
/// </summary>
public sealed class PermissionRegistry
{
    private readonly FrozenDictionary<string, PermissionDefinitionInfo> _definitions;

    private PermissionRegistry(FrozenDictionary<string, PermissionDefinitionInfo> definitions)
    {
        _definitions = definitions;
    }

    public static PermissionRegistry Instance { get; } = Discover(typeof(Permissions));

    public static PermissionRegistry Discover(Type catalogRoot)
    {
        var definitions = new Dictionary<string, PermissionDefinitionInfo>(StringComparer.Ordinal);
        DiscoverType(catalogRoot, definitions);
        return new PermissionRegistry(definitions.ToFrozenDictionary(StringComparer.Ordinal));
    }

    public PermissionDefinitionInfo? Get(string key) => _definitions.GetValueOrDefault(key);

    public IReadOnlyCollection<PermissionDefinitionInfo> GetAll() => _definitions.Values;

    public bool Contains(string key) => _definitions.ContainsKey(key);

    public PermissionProviderName GetAllowedProviders(string key) => Get(key)?.AllowedProviders ?? PermissionProviderName.None;

    public bool IsProviderAllowed(string key, PermissionProviderName provider)
    {
        var allowed = GetAllowedProviders(key);
        return allowed != PermissionProviderName.None && (allowed & provider) == provider;
    }

    private static void DiscoverType(Type type, Dictionary<string, PermissionDefinitionInfo> definitions)
    {
        const BindingFlags fieldFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;

        foreach (var field in type.GetFields(fieldFlags))
        {
            if (!field.IsLiteral || field.FieldType != typeof(string))
                continue;

            var attribute = field.GetCustomAttribute<PermissionDefinitionAttribute>();
            if (attribute is null)
                continue;

            var key = (string)field.GetRawConstantValue()!;
            if (!definitions.TryAdd(key, new PermissionDefinitionInfo(key, attribute.Providers)))
                throw new InvalidOperationException(
                    $"Duplicate permission key \"{key}\" declared on {type.FullName}.{field.Name}.");
        }

        foreach (var nested in type.GetNestedTypes(BindingFlags.Public))
            DiscoverType(nested, definitions);
    }
}
