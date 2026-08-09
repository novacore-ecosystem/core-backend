namespace NovaCore.BuildingBlock.Contract.Events.User;

/// <summary>
/// Published by Auth whenever an Account's effective permission set may have changed (direct
/// AccountRole grant, RolePermission grant on a Role the Account holds directly or via an
/// effective Position). Carries the final, already-merged permission array - User only stores it,
/// it never recomputes (see docs/services/auth-service.md, Phase 3).
/// </summary>
public sealed record AccountEffectivePermissionsChangedIntegrationEvent(
    Guid TenantId,
    Guid AccountId,
    string[] Permissions,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(AccountEffectivePermissionsChangedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
