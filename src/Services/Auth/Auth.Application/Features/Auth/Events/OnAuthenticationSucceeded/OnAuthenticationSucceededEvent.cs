namespace NovaCore.Auth.Application.Features.Auth.Events.OnAuthenticationSucceeded;

public sealed record OnAuthenticationSucceededEvent(
    Guid AccountId,
    Guid? AppId,
    Guid TenantId,
    string AuthenticationType,
    Guid JwtId,
    string? IpAddress,
    string CorrelationId) : IInternalEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
