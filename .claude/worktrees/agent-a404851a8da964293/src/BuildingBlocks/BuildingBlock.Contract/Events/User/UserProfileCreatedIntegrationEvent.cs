namespace NovaCore.BuildingBlock.Contract.Events.User;

public sealed record UserProfileCreatedIntegrationEvent(
    Guid TenantId,
    Guid UserId,
    string Email,
    string UserName,
    string FirstName,
    string MiddleName,
    string LastName,
    string? CorrelationId = null,
    string[]? Roles = null,
    string TempPassword = "") : IIntegrationEvent
{
    // Get or init data
    public string[] Roles { get; } = Roles ?? [];

    // Set base values as default
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(UserProfileCreatedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
