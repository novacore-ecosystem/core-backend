namespace NovaCore.User.Domain.Events;

/// <summary>
/// Payload shape for "a UserRole's PermissionCollection changed" - carries the role's full
/// resulting permission set so a subscriber can recompute every affected User's
/// UserPermissionSnapshot without a separate read. See RoleCreated for why this is an unwired
/// record rather than a raised domain event.
/// </summary>
public sealed record RolePermissionChanged(Guid RoleId, IReadOnlyCollection<string> Permissions);
