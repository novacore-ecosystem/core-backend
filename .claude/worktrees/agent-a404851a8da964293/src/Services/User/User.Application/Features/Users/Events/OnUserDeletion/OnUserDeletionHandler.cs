using NovaCore.BuildingBlock.Application.Abstractions.Events;

using NovaCore.User.Application.Abstractions.Services;
using NovaCore.User.Application.Features.Users.Events.OnUserSearchRemovalRequired;

namespace NovaCore.User.Application.Features.Users.Events.OnUserDeletion;

public sealed class OnUserDeletionHandler(
    IUserWriteService userWriteService,
    IInternalEventDispatcher eventDispatcher,
    IUserProfileDetailCache userProfileCache) : IInternalEventHandler<OnUserDeletionEvent>
{
    public async Task Handle(OnUserDeletionEvent @event, CancellationToken ct = default)
    {
        // This is the REAL deletion path - the separate DeleteUserCommand/DeleteUserHandler have
        // no callers anywhere in the repo.
        await userWriteService.DeleteWithNoTrackingAsync(@event.Id, ct);

        // Search removal trigger - dispatched inline rather than via Outbox/Kafka
        // self-consumption, since this handler already runs off an inbound Kafka message with
        // Inbox-dedup guarantees; no second hop needed. See
        // docs/tasks/2026-07-28/Task8_projection-builder-and-sync-events.md.
        await eventDispatcher.PublishAsync(new OnUserSearchRemovalRequiredEvent(@event.Id), ct);

        // User Detail cache invalidation - explicit, after the delete has actually happened.
        await userProfileCache.InvalidateAsync(@event.Id, ct);
    }
}