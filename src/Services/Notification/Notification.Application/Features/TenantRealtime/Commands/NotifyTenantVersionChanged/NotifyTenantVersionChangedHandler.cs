using NovaCore.Notification.Application.Abstractions.Services;

namespace NovaCore.Notification.Application.Features.TenantRealtime.Commands.NotifyTenantVersionChanged;

public sealed class NotifyTenantVersionChangedHandler(IRealtimeNotifier realtimeNotifier)
    : ICommandHandler<NotifyTenantVersionChangedCommand>
{
    public async Task Handle(NotifyTenantVersionChangedCommand request, CancellationToken ct = default)
    {
        await realtimeNotifier.PushTenantBootstrapVersionChangedAsync(request.TenantId, request.Version, ct);
    }
}
