namespace NovaCore.BuildingBlock.Contract.Events.User;

/// <summary>
/// Published by Auth whenever a Role permission change may have altered one or more Accounts'
/// effective permission set (direct AccountRole grant, RolePermission grant on a Role the Account
/// holds directly or via an effective Position). One event per Role permission update, batching
/// every affected Account rather than one message per Account. Each entry carries its own Account's
/// final, already-merged permission array - different Accounts affected by the same Role change can
/// still end up with different effective sets (extra Roles/Positions), so no single shared array is
/// assumed. User only stores what it's given, it never recomputes (see
/// docs/services/auth-service.md, Phase 3).
/// </summary>
public sealed record AccountEffectivePermissionsChangedIntegrationEvent(
    Guid TenantId,
    IReadOnlyList<AccountEffectivePermissions> Accounts,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(AccountEffectivePermissionsChangedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}

public sealed record AccountEffectivePermissions(Guid AccountId, string[] Permissions);
