using NovaCore.BuildingBlock.Application.Abstractions.Events;

namespace NovaCore.User.Application.Features.Users.Events.OnAccountEffectivePermissionsChanged;

public record OnAccountEffectivePermissionsChangedEvent(Guid UserId, IReadOnlyList<string> Permissions) : IInternalEvent
{
    public string CorrelationId { get; } = string.Empty;
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
