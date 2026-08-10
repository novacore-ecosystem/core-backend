namespace NovaCore.Auth.Application.Features.Auth.Events.OnUserCreated;

public sealed record OnUserCreatedEvent(
    string UserId,
    string Email,
    string UserName,
    string FirstName,
    string MiddleName,
    string LastName,
    string[] Roles,
    string TempPassword,
    string CorrelationId,
    DateTime OccurredAt) : IInternalEvent
{
    public OnUserCreatedEvent(
        string userId,
        string email,
        string userName,
        string firstName,
        string middleName,
        string lastName,
        string[] roles,
        string tempPassword,
        string correlationId)
        : this(userId, email, userName, firstName, middleName, lastName, roles, tempPassword, correlationId, DateTime.UtcNow)
    {
    }
}
