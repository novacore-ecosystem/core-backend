namespace NovaCore.User.Application.Abstractions.Persistence.Users;

/// <summary>One Account's resulting effective permission set, as carried by
/// AccountEffectivePermissionsChangedIntegrationEvent - UserId equals the correlated Auth AccountId.</summary>
public sealed record AccountAuthorizationUpdate(Guid UserId, IReadOnlyList<string> Permissions);
