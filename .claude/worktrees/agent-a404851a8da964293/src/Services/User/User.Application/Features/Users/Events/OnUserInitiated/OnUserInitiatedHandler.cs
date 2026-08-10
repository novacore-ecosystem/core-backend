using NovaCore.BuildingBlock.Application.Abstractions.Events;

using NovaCore.User.Application.Features.Users.Events.OnUserSearchSyncRequired;

namespace NovaCore.User.Application.Features.Users.Events.OnUserInitiated;

public sealed class OnUserInitiatedHandler(
    IUserWriteService userWriteService,
    IInternalEventDispatcher eventDispatcher) : IInternalEventHandler<OnUserInitiatedEvent>
{
    public async Task Handle(OnUserInitiatedEvent @event, CancellationToken ct = default)
    {
        // Mirrors the Account Auth already created (self-registration path) into a local User -
        // role assignment is Auth's own concern (AccountRole), not replicated here.
        var user = await userWriteService.SyncFromAccountInitiationAsync(
            new SyncUserRequest(
                @event.AccountId,
                @event.UserName,
                @event.Email,
                @event.PhoneNumber,
                @event.FirstName,
                @event.MiddleName,
                @event.LastName),
            ct);

        // Search sync trigger - dispatched inline rather than via Outbox/Kafka self-consumption,
        // since this handler already runs in-process off Auth's gRPC call; no cross-service hop
        // to decouple from. See docs/tasks/2026-07-28/Task8_projection-builder-and-sync-events.md.
        await eventDispatcher.PublishAsync(new OnUserSearchSyncRequiredEvent(user.Id, @event.CorrelationId), ct);
    }
}
