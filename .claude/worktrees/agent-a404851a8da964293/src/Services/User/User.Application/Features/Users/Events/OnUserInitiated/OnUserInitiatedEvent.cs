using NovaCore.BuildingBlock.Application.Abstractions.Events;

namespace NovaCore.User.Application.Features.Users.Events.OnUserInitiated;

public sealed record OnUserInitiatedEvent(
    Guid AccountId,
    string Email,
    string UserName,
    string PhoneNumber,
    string FirstName,
    string MiddleName,
    string LastName,
    string CorrelationId = "") : IInternalEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
