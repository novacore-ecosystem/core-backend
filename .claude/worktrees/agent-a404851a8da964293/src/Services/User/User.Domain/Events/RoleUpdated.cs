namespace NovaCore.User.Domain.Events;

/// <summary>Payload shape for "a UserRole's description or status changed". See RoleCreated for
/// why this is an unwired record rather than a raised domain event.</summary>
public sealed record RoleUpdated(Guid RoleId);
