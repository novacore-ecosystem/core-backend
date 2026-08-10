namespace NovaCore.User.Domain.Events;

/// <summary>
/// Payload shape for "a UserRole was created". No IDomainEvent base exists yet anywhere in this
/// codebase (Domain layers here don't raise events - see the Application-layer MediatR
/// notifications used elsewhere instead), so this is intentionally a plain, unwired record: it
/// documents the data an eventual Application-layer publisher will need, not a raised event.
/// </summary>
public sealed record RoleCreated(Guid RoleId, string Key);
