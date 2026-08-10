namespace NovaCore.User.Domain.Events;

/// <summary>Payload shape for "a UserRoleAssignment was revoked or expired for a User". See
/// RoleCreated for why this is an unwired record rather than a raised domain event.</summary>
public sealed record UserRoleRemoved(Guid UserId, Guid RoleId, Guid AssignmentId);
