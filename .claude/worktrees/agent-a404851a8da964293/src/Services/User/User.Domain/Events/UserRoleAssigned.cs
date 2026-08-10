namespace NovaCore.User.Domain.Events;

/// <summary>Payload shape for "a UserRoleAssignment was created for a User". See RoleCreated for
/// why this is an unwired record rather than a raised domain event.</summary>
public sealed record UserRoleAssigned(Guid UserId, Guid RoleId, Guid AssignmentId);
