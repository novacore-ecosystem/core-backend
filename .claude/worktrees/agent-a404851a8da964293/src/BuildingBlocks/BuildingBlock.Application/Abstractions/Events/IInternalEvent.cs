using MediatR;

namespace NovaCore.BuildingBlock.Application.Abstractions.Events;

/// <summary>
/// Represents an internal application event handled synchronously via MediatR.
/// Used to decouple application logic from infrastructure concerns.
/// Handlers should move business logic from Infrastructure to Application layer.
/// </summary>
public interface IInternalEvent : INotification
{
    /// <summary>
    /// Unique identifier for event correlation/tracing
    /// </summary>
    string CorrelationId { get; }

    /// <summary>
    /// Timestamp when event occurred
    /// </summary>
    DateTime OccurredAt { get; }
}
