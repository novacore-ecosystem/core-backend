namespace NovaCore.Auth.Application.Features.Auth.Events.OnUserRegistered;

public sealed record OnUserRegisteredEvent(
    Guid UserId,
    string Email,
    string UserName,
    string FirstName,
    string MiddleName,
    string LastName,
    string PhoneNumber,
    string CorrelationId) : IInternalEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
