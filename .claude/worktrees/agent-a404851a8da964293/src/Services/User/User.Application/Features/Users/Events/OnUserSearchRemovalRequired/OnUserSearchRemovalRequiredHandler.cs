using NovaCore.BuildingBlock.Application.Abstractions.Events;

using NovaCore.User.Application.Abstractions.Search;

namespace NovaCore.User.Application.Features.Users.Events.OnUserSearchRemovalRequired;

public sealed class OnUserSearchRemovalRequiredHandler(IUserSearchIndexer searchIndexer)
    : IInternalEventHandler<OnUserSearchRemovalRequiredEvent>
{
    public Task Handle(OnUserSearchRemovalRequiredEvent @event, CancellationToken ct = default)
        => searchIndexer.DeleteAsync(@event.UserId, ct);
}
