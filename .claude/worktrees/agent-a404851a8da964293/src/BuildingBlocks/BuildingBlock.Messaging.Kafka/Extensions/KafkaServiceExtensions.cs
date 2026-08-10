using NovaCore.BuildingBlock.Messaging.Abstractions;
using NovaCore.BuildingBlock.Messaging.Kafka.BackgroundServices;
using NovaCore.BuildingBlock.Messaging.Kafka.Configuration;
using NovaCore.BuildingBlock.Messaging.Kafka.Consumers;
using NovaCore.BuildingBlock.Messaging.Kafka.Publishers;
using NovaCore.BuildingBlock.Messaging.Services;

using Confluent.Kafka;

using KafkaFlow;
using KafkaFlow.Producers;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NovaCore.BuildingBlock.Messaging.Kafka.Extensions;

public static class KafkaServiceExtensions
{
    public static IServiceCollection AddKafkaMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        var kafkaOptions = configuration.GetSection(KafkaOptions.Section).Get<KafkaOptions>()
            ?? new KafkaOptions();

        services.AddSingleton(kafkaOptions);

        // Register a placeholder delegate so DiscoverConsumerTopics (below) can construct
        // IntegrationEventConsumerRegistry before Inbox infrastructure exists. Callers must
        // invoke AddInboxOutboxInfrastructure AFTER this method so its real delegate
        // registration is last and wins when resolved as a single instance.
        services.AddScoped<Func<InboxDispatchContext, Func<Task>, CancellationToken, Task>>(sp =>
            (context, handlerAction, ct) => handlerAction());

        // Register ConsumerRegistry with the delegate for Inbox dedup/retry tracking.
        // The delegate is provided by AddInboxOutboxInfrastructure; it enables Inbox
        // dedup/retry to work generically without service-specific knowledge.
        services.AddScoped(sp =>
        {
            var executeWithInboxAsync = sp.GetRequiredService<Func<InboxDispatchContext, Func<Task>, CancellationToken, Task>>();
            var consumers = sp.GetRequiredService<IEnumerable<IIntegrationEventConsumer>>();
            var logger = sp.GetRequiredService<ILogger<IntegrationEventConsumerRegistry>>();
            return new IntegrationEventConsumerRegistry(consumers, executeWithInboxAsync, logger);
        });

        var topics = DiscoverConsumerTopics(services);
        var brokers = kafkaOptions.BootstrapServers
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var groupId = string.IsNullOrWhiteSpace(kafkaOptions.GroupId) || kafkaOptions.GroupId == "default-consumer-group"
            ? $"{serviceName}-consumer-group"
            : kafkaOptions.GroupId;

        services.AddKafka(kafka => kafka
            .AddCluster(cluster =>
            {
                cluster.WithBrokers(brokers);

                cluster.AddProducer(KafkaFlowDefaults.ProducerName, producer => producer
                    .WithProducerConfig(BuildProducerConfig(kafkaOptions)));

                if (topics.Count > 0)
                {
                    cluster.AddConsumer(consumer => consumer
                        .Topics(topics)
                        .WithGroupId(groupId)
                        .WithConsumerConfig(BuildConsumerConfig(kafkaOptions, groupId))
                        .WithWorkersCount(kafkaOptions.ConsumerWorkersCount)
                        .WithBufferSize(kafkaOptions.ConsumerBufferSize)
                        .AddMiddlewares(middlewares => middlewares
                            .AddTypedHandlers(handlers => handlers
                                .AddHandler<IntegrationEventDispatchHandler>()
                                .WithHandlerLifetime(InstanceLifetime.Scoped))));
                }
            }));

        services.AddScoped<IEventPublisher>(sp =>
            new KafkaFlowEventPublisher(sp.GetRequiredService<IProducerAccessor>()));

        services.AddScoped<IOutboxPublisher>(sp =>
            new KafkaOutboxPublisher(sp.GetRequiredService<IProducerAccessor>()));

        services.AddScoped<IEventDispatcher, EventDispatcher>();
        services.AddSingleton<IHostedService, KafkaFlowBusHostedService>();

        return services;
    }

    private static IReadOnlyCollection<string> DiscoverConsumerTopics(IServiceCollection services)
    {
        using var tempProvider = services.BuildServiceProvider();
        using var scope = tempProvider.CreateScope();
        return scope.ServiceProvider
            .GetRequiredService<IntegrationEventConsumerRegistry>()
            .Topics;
    }

    private static ProducerConfig BuildProducerConfig(KafkaOptions options)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = options.BootstrapServers,
            RequestTimeoutMs = options.RequestTimeoutMs,
            MessageTimeoutMs = options.RequestTimeoutMs,
            Acks = Confluent.Kafka.Acks.All,
        };

        ApplySecurityConfig(config, options);
        return config;
    }

    private static ConsumerConfig BuildConsumerConfig(KafkaOptions options, string groupId)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = options.BootstrapServers,
            GroupId = groupId,
            EnableAutoCommit = options.EnableAutoCommit,
            AutoCommitIntervalMs = options.AutoCommitIntervalMs,
            AutoOffsetReset = ParseAutoOffsetReset(options.AutoOffsetReset),
            SessionTimeoutMs = options.SessionTimeoutMs,
            MaxPollIntervalMs = options.RequestTimeoutMs,
        };

        ApplySecurityConfig(config, options);
        return config;
    }

    private static void ApplySecurityConfig(ClientConfig config, KafkaOptions options)
    {
        if (!options.EnableSecurityProtocol)
            return;

        config.SecurityProtocol = SecurityProtocol.SaslSsl;

        if (!string.IsNullOrEmpty(options.SaslMechanism))
            config.SaslMechanism = ParseSaslMechanism(options.SaslMechanism);

        if (!string.IsNullOrEmpty(options.SaslUsername))
            config.SaslUsername = options.SaslUsername;

        if (!string.IsNullOrEmpty(options.SaslPassword))
            config.SaslPassword = options.SaslPassword;
    }

    private static Confluent.Kafka.AutoOffsetReset ParseAutoOffsetReset(string value) =>
        value.ToLowerInvariant() switch
        {
            "latest" => Confluent.Kafka.AutoOffsetReset.Latest,
            _ => Confluent.Kafka.AutoOffsetReset.Earliest
        };

    private static SaslMechanism ParseSaslMechanism(string value) =>
        value.ToUpperInvariant() switch
        {
            "SCRAM-SHA-256" => SaslMechanism.ScramSha256,
            "SCRAM-SHA-512" => SaslMechanism.ScramSha512,
            _ => SaslMechanism.Plain
        };
}
