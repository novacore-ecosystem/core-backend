namespace NovaCore.Notification.Application.Features.TenantRealtime.Commands.NotifyTenantVersionChanged;

/// <summary>Dispatched by NotificationTriggerConsumer on TenantVersionChangedIntegrationEvent -
/// pushes IGlobalHubBase.BootstrapVersionChanged to every connection in the tenant's group.
/// Backend foundation only - see docs/services/auth-service.md, "Notification Hub Version Check".
/// Does not refresh a Redis version cache on this side: Notification has no Redis dependency
/// today (unlike Auth, see Auth.Infrastructure.Caching.TenantVersionCache), and adding one only
/// for this would make an optional piece of infrastructure a hard startup dependency for the
/// whole service - deferred to whenever a real Hub-connection version-check consumer needs it.</summary>
public sealed record NotifyTenantVersionChangedCommand(
    Guid TenantId,
    int Version) : ICommand;
