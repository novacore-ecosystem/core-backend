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

        // Replace the placeholder delegate with the real Inbox implementation. Must be
        // registered after AddKafkaMessaging so this registration is last and wins when
        // resolved as a single instance (DiscoverConsumerTopics only reads registry.Topics,
        // it never invokes the delegate, so the placeholder being present during that
        // eager-discovery step inside AddKafkaMessaging is harmless).
        services.AddScoped(BuildInboxExecutionDelegate);

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
}
