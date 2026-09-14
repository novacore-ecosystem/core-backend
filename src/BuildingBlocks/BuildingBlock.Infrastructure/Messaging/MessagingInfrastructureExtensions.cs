using System.Text.Json;

using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Messaging.Abstractions;
using NovaCore.BuildingBlock.Messaging.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NovaCore.BuildingBlock.Infrastructure.Messaging;

/// <summary>
/// Extensions for registering Inbox/Outbox background infrastructure.
/// Registers the OutboxRelayHostedService/InboxRetryHostedService and wires up the Inbox
/// dedup/retry delegate consumed by IntegrationEventConsumerRegistry.
/// </summary>
public static class MessagingInfrastructureExtensions
{
    /// <summary>
    /// Registers the Outbox Relay + Inbox Retry hosted services and Inbox dedup/retry support.
    /// Must be called AFTER AddKafkaMessaging: the .NET DI container resolves a single-instance
    /// service to its LAST registration, so this has to be added after AddKafkaMessaging's
    /// placeholder delegate for the real Inbox implementation to actually win at runtime.
    /// </summary>
    public static IServiceCollection AddInboxOutboxInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .Configure<OutboxRelayOptions>(configuration.GetSection(OutboxRelayOptions.Section));
        services
            .Configure<InboxRetryOptions>(configuration.GetSection(InboxRetryOptions.Section));

        services.AddScoped<InboxAttemptExecutor>();
        services.AddScoped<OutboxDispatcher>();

        // Replace the placeholder delegate with the real Inbox implementation. Must be
        // registered after AddKafkaMessaging so this registration is last and wins when
        // resolved as a single instance (DiscoverConsumerTopics only reads registry.Topics,
        // it never invokes the delegate, so the placeholder being present during that
        // eager-discovery step inside AddKafkaMessaging is harmless).
        services.AddScoped(BuildInboxExecutionDelegate);

        // Consumed by EfUnitOfWork as its optional notifyOutboxCommittedAsync constructor
        // parameter - scoped to the same DbContext-backed scope as the unit of work itself, so
        // marking a row processed reuses the connection/tracked entity from the transaction that
        // just committed it instead of opening a second one.
        services.AddScoped(BuildOutboxCommitNotificationDelegate);

        // Register Outbox relay + Inbox retry background services
        services.AddSingleton<IHostedService, OutboxRelayHostedService>();
        services.AddSingleton<IHostedService, InboxRetryHostedService>();

        return services;
    }

    /// <summary>
    /// Builds the executeWithInboxAsync delegate for IntegrationEventConsumerRegistry. This
    /// enables generic Inbox dedup/retry tracking without the Messaging project needing to
    /// depend on Application or Persistence.
    /// </summary>
    private static Func<InboxDispatchContext, Func<Task>, CancellationToken, Task> BuildInboxExecutionDelegate(
        IServiceProvider provider)
    {
        return async (dispatchContext, handlerAction, ct) =>
        {
            var inboxStore = provider.GetRequiredService<IInboxStore>();
            var executor = provider.GetRequiredService<InboxAttemptExecutor>();
            var headersJson = JsonSerializer.Serialize(dispatchContext.Headers);

            await executor.ExecuteAsync(
                inboxStore,
                dispatchContext.MessageId,
                dispatchContext.ConsumerName,
                dispatchContext.Topic,
                dispatchContext.Payload,
                headersJson,
                handlerAction,
                ct);
        };
    }

    /// <summary>
    /// Builds the notifyOutboxCommittedAsync delegate consumed by EfUnitOfWork. Re-reads the
    /// just-committed rows by id and publishes each through the shared OutboxDispatcher, so the
    /// immediate path publishes with the same message-id and the same success/failure semantics
    /// as OutboxRelayHostedService's background retry.
    /// </summary>
    private static Func<IReadOnlyList<Guid>, CancellationToken, Task> BuildOutboxCommitNotificationDelegate(
        IServiceProvider provider)
    {
        return async (committedOutboxIds, ct) =>
        {
            var outboxStore = provider.GetRequiredService<IOutboxStore>();
            var dispatcher = provider.GetRequiredService<OutboxDispatcher>();

            var messages = await outboxStore.GetByIdsAsync(committedOutboxIds, ct);
            foreach (var message in messages)
            {
                await dispatcher.PublishAndMarkAsync(message, ct);
            }
        };
    }
}
