using NovaCore.BuildingBlock.Application.Abstractions.Events;

namespace NovaCore.User.Application.Features.Users.Events.OnAccountEffectivePermissionsChanged;

public sealed class OnAccountEffectivePermissionsChangedHandler(IUserWriteService userWriteService)
    : IInternalEventHandler<OnAccountEffectivePermissionsChangedEvent>
{
    public async Task Handle(OnAccountEffectivePermissionsChangedEvent @event, CancellationToken ct = default)
    {
        await userWriteService.RebuildAuthorizationSnapshotAsync(@event.UserId, @event.Permissions, ct);
    }
}
