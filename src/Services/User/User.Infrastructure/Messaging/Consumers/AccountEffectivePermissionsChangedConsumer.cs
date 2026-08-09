using System.Text.Json;

using NovaCore.BuildingBlock.Application.Abstractions.Events;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Contract.Events.User;
using NovaCore.BuildingBlock.Messaging.Abstractions;

using NovaCore.User.Application.Features.Users.Events.OnAccountEffectivePermissionsChanged;

namespace NovaCore.User.Infrastructure.Messaging.Consumers;

/// <summary>Consumes Auth's AccountEffectivePermissionsChangedIntegrationEvent to keep
/// UserAuthorizationSnapshot in sync - see docs/services/auth-service.md, Phase 3. AccountId in
/// the event equals this User's Id (the two rows are correlated by sharing an id, same as every
/// other Auth-to-User Account/User correlation - see User.Create's id override).</summary>
public sealed class AccountEffectivePermissionsChangedConsumer(
    IInternalEventDispatcher eventDispatcher,
    IAppLogger<AccountEffectivePermissionsChangedConsumer> logger)
    : IIntegrationEventConsumer
{
    public IEnumerable<string> Topics => [
        nameof(AccountEffectivePermissionsChangedIntegrationEvent).ToLowerInvariant(),
    ];

    public async Task HandleAsync(
        string message,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken ct = default)
    {
        var integrationEvent = JsonSerializer.Deserialize<AccountEffectivePermissionsChangedIntegrationEvent>(message)
            ?? throw new InvalidOperationException("Failed to deserialize AccountEffectivePermissionsChangedIntegrationEvent");

        logger.Information(
            "Received AccountEffectivePermissionsChangedIntegrationEvent for AccountId: {AccountId}",
            integrationEvent.AccountId);

        var @event = new OnAccountEffectivePermissionsChangedEvent(integrationEvent.AccountId, integrationEvent.Permissions);
        await eventDispatcher.PublishAsync(@event, ct);

        logger.Information(
            "Successfully processed AccountEffectivePermissionsChangedIntegrationEvent for AccountId: {AccountId}",
            integrationEvent.AccountId);
    }
}
