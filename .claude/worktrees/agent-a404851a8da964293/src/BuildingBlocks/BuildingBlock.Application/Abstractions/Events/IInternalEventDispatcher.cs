namespace NovaCore.BuildingBlock.Application.Abstractions.Events;

/// <summary>
/// Dispatches application events synchronously to registered MediatR handlers.
/// Used for internal orchestration logic within a single service.
/// </summary>
public interface IInternalEventDispatcher
{
    /// <summary>
    /// Publishes an application event to all registered MediatR handlers.
    /// Handlers execute synchronously in order.
    /// </summary>
    /// <typeparam name="TEvent">The application event type</typeparam>
    /// <param name="event">The event to publish</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : IInternalEvent;
}
