using NovaCore.BuildingBlock.Application.Abstractions.Events;

using NovaCore.User.Application.Abstractions.Persistence.Users;

namespace NovaCore.User.Application.Features.Users.Events.OnAccountEffectivePermissionsChanged;

public record OnAccountEffectivePermissionsChangedEvent(IReadOnlyList<AccountAuthorizationUpdate> Updates) : IInternalEvent
{
    public string CorrelationId { get; } = string.Empty;
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
