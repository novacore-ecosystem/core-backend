namespace NovaCore.BuildingBlock.Contract.Events.User;

/// <summary>
/// The two ways an Account can end up holding a fresh access token - kept as plain string
/// constants (not an enum) so the value survives JSON round-tripping across services without an
/// enum-serialization convention decision.
/// </summary>
public static class AuthenticationType
{
    public const string Login = nameof(Login);
    public const string RefreshToken = nameof(RefreshToken);
}

/// <summary>
/// Raised by Login and RefreshToken once credentials/tokens have been validated and a new access
/// token has been issued - identifiers only, no Account/Device/Session entities. Consumed
/// in-service (AuthenticationSucceededConsumer) to record Session/Device/LoginHistory without
/// making Login/RefreshToken wait on those writes.
/// </summary>
public sealed record AuthenticationSucceededIntegrationEvent(
    string AccountId,
    string? AppId,
    string TenantId,
    string AuthenticationType,
    string JwtId,
    string? IpAddress,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; } = nameof(AuthenticationSucceededIntegrationEvent);
    public DateTime PublishedAt { get; } = DateTime.UtcNow;
}
