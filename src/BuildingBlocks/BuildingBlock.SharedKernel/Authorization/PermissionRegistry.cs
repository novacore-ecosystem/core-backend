using System.Collections.Frozen;
using System.Reflection;

using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.BuildingBlock.SharedKernel.Authorization;

public sealed record PermissionDefinitionInfo(string Key, PermissionProviderName AllowedProviders, string? GroupCode);

public sealed record PermissionGroupInfo(string Code, IReadOnlyList<string> PermissionKeys);

/// <summary>
/// Immutable, in-memory catalog of every permission key the platform recognizes, discovered once
/// via reflection over Permissions.*.cs's [PermissionDefinition]-attributed const fields (grouped
/// by their nearest enclosing [PermissionGroup]-attributed class, if any). This is the code-owned
/// source of permission/group existence and structural provider applicability - lookups never
/// touch PostgreSQL (see docs/services/auth-service.md's authorization-foundation phases).
/// Instance is a lazy static singleton so Auth.Domain (PermissionKey.Create) can use it without a
/// DI dependency; the same instance is also registered as a DI singleton for constructor injection
/// (grant validation, DbMigrator sync) - see Auth.Persistence/DependencyInjection.cs.
///
/// Responsibility is deliberately narrow: discover, validate, index, expose. It does not resolve
/// effective permissions, evaluate Role/Position assignment, or touch Redis/localization/UI
/// state - those are other layers' concern (see docs/conventions/permission-catalog-conventions.md).
/// </summary>
public sealed class PermissionRegistry
{
    private readonly FrozenDictionary<string, PermissionDefinitionInfo> _definitions;
    private readonly FrozenDictionary<string, PermissionGroupInfo> _groups;

    private PermissionRegistry(
        FrozenDictionary<string, PermissionDefinitionInfo> definitions,
        FrozenDictionary<string, PermissionGroupInfo> groups)
    {
        _definitions = definitions;
        _groups = groups;
    }

    public static PermissionRegistry Instance { get; } = Discover(typeof(Permissions));

    public static PermissionRegistry Discover(Type catalogRoot)
    {
        var definitions = new Dictionary<string, PermissionDefinitionInfo>(StringComparer.Ordinal);
        var groupMembers = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        DiscoverType(catalogRoot, groupCode: null, definitions, groupMembers);

        var groups = groupMembers.ToFrozenDictionary(
            g => g.Key,
            g => new PermissionGroupInfo(g.Key, g.Value.AsReadOnly()),
            StringComparer.Ordinal);

        return new PermissionRegistry(definitions.ToFrozenDictionary(StringComparer.Ordinal), groups);
    }

    // ==================== Flat index ====================

    public PermissionDefinitionInfo? Get(string key) => _definitions.GetValueOrDefault(key);

    public IReadOnlyCollection<PermissionDefinitionInfo> GetAll() => _definitions.Values;

    public bool Contains(string key) => _definitions.ContainsKey(key);

    public PermissionProviderName GetAllowedProviders(string key) => Get(key)?.AllowedProviders ?? PermissionProviderName.None;

    public bool IsProviderAllowed(string key, PermissionProviderName provider)
    {
        var allowed = GetAllowedProviders(key);
        return allowed != PermissionProviderName.None && (allowed & provider) == provider;
    }

    // ==================== Group index ====================
    // Precomputed once during Discover(), not rebuilt per call - a group's membership is a
    // structural, startup-time fact of the catalog, not a runtime query.

    public IReadOnlyCollection<PermissionGroupInfo> GetGroups() => _groups.Values;

    public PermissionGroupInfo? GetGroup(string groupCode) => _groups.GetValueOrDefault(groupCode);

    public IReadOnlyList<string> GetPermissions(string groupCode) => GetGroup(groupCode)?.PermissionKeys ?? [];

    private static void DiscoverType(
        Type type,
        string? groupCode,
        Dictionary<string, PermissionDefinitionInfo> definitions,
        Dictionary<string, List<string>> groupMembers)
    {
        var effectiveGroupCode = type.GetCustomAttribute<PermissionGroupAttribute>()?.Code ?? groupCode;

        const BindingFlags fieldFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;

        foreach (var field in type.GetFields(fieldFlags))
        {
            if (!field.IsLiteral || field.FieldType != typeof(string))
                continue;

            var attribute = field.GetCustomAttribute<PermissionDefinitionAttribute>();
            if (attribute is null)
                continue;

            var key = (string)field.GetRawConstantValue()!;
            if (!definitions.TryAdd(key, new PermissionDefinitionInfo(key, attribute.Providers, effectiveGroupCode)))
                throw new InvalidOperationException(
                    $"Duplicate permission key \"{key}\" declared on {type.FullName}.{field.Name}.");

            if (effectiveGroupCode is not null)
            {
                if (!groupMembers.TryGetValue(effectiveGroupCode, out var members))
                    groupMembers[effectiveGroupCode] = members = [];
                members.Add(key);
            }
        }

        foreach (var nested in type.GetNestedTypes(BindingFlags.Public))
            DiscoverType(nested, effectiveGroupCode, definitions, groupMembers);
    }
}
