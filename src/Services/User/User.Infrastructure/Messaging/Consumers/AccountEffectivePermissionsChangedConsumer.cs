using System.Text.Json;

using NovaCore.BuildingBlock.Application.Abstractions.Events;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Contract.Events.User;
using NovaCore.BuildingBlock.Messaging.Abstractions;

using NovaCore.User.Application.Abstractions.Persistence.Users;
using NovaCore.User.Application.Features.Users.Events.OnAccountEffectivePermissionsChanged;

namespace NovaCore.User.Infrastructure.Messaging.Consumers;

/// <summary>Consumes Auth's AccountEffectivePermissionsChangedIntegrationEvent to keep
/// UserAuthorizationSnapshot in sync - see docs/services/auth-service.md, Phase 3. One event batches
/// every Account a Role permission change affected; AccountId in each entry equals a User's Id (the
/// two rows are correlated by sharing an id, same as every other Auth-to-User Account/User
/// correlation - see User.Create's id override).</summary>
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
            "Received AccountEffectivePermissionsChangedIntegrationEvent for {AccountCount} account(s)",
            integrationEvent.Accounts.Count);

        var updates = integrationEvent.Accounts
            .Select(a => new AccountAuthorizationUpdate(a.AccountId, a.Permissions))
            .ToArray();

        var @event = new OnAccountEffectivePermissionsChangedEvent(updates);
        await eventDispatcher.PublishAsync(@event, ct);

        logger.Information(
            "Successfully processed AccountEffectivePermissionsChangedIntegrationEvent for {AccountCount} account(s)",
            integrationEvent.Accounts.Count);
    }
}
