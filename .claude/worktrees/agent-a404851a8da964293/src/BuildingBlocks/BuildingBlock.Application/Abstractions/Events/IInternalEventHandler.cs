using MediatR;

namespace NovaCore.BuildingBlock.Application.Abstractions.Events;

/// <summary>
/// Handles application events synchronously via MediatR.
/// Implement this to move business logic from Infrastructure to Application layer.
/// </summary>
/// <typeparam name="TEvent">The application event type</typeparam>
public interface IInternalEventHandler<in TEvent> : INotificationHandler<TEvent>
    where TEvent : IInternalEvent
{
}
