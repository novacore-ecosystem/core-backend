using NovaCore.BuildingBlock.Application.Abstractions.Events;

using MediatR;

namespace NovaCore.BuildingBlock.Infrastructure.Events;

/// <summary>
/// MediatR-based implementation of IApplicationEventDispatcher.
/// Handles synchronous application event dispatch to move business logic from Infrastructure to Application.
/// </summary>
public sealed class InternalEventDispatcher(IMediator mediator) : IInternalEventDispatcher
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Publishes an application event to all registered MediatR handlers.
    /// </summary>
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : IInternalEvent
    {
        await _mediator.Publish(@event, ct);
    }
}
