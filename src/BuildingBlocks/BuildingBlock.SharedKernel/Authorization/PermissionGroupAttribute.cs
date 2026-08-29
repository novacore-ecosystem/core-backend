namespace NovaCore.BuildingBlock.SharedKernel.Authorization;

/// <summary>
/// Decorates a Permissions.cs nested static class (e.g. `Shipment`), declaring it as a permission
/// group with the given stable Code - the same identity PermissionRegistry indexes by and
/// DbMigrator seeds PermissionGroup rows under. Deliberately carries nothing else (no display
/// name/description/sort order/translation) - that metadata belongs to the DB-backed
/// PermissionGroup, not this code-owned structural contract. A const not nested under any
/// [PermissionGroup]-attributed class (e.g. Permissions.Root/Permissions.User) is a valid,
/// deliberately ungrouped permission - PermissionRegistry does not require every permission to
/// belong to a group.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class PermissionGroupAttribute(string code) : Attribute
{
    public string Code { get; } = code;
}
