using System.Diagnostics;
using System.Text.Json;

using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Infrastructure.Observability;
using NovaCore.BuildingBlock.Messaging.Abstractions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NovaCore.BuildingBlock.Infrastructure.Messaging;

/// <summary>
/// Polls the Inbox for Retrying rows whose NextRetryAt has arrived and replays them against the
/// originally-targeted consumer, using the payload/headers captured at first sight (no need for
/// the original Kafka message to still be available). Ignores Processed and DeadLetter rows -
/// BeginAttemptAsync (via InboxAttemptExecutor) re-checks status/NextRetryAt per row regardless,
/// so a row that races with a live Kafka redelivery is still handled safely.
/// </summary>
public sealed class InboxRetryHostedService(
    IServiceProvider serviceProvider,
    IOptions<InboxRetryOptions> options,
    ILogger<InboxRetryHostedService> logger)
    : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly InboxRetryOptions _options = options.Value;
    private readonly ILogger<InboxRetryHostedService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Inbox retry hosted service starting");

        using var timer = new PeriodicTimer(_options.PollingInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ProcessDueRetriesAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Inbox retry hosted service stopping");
        }
    }

    private async Task ProcessDueRetriesAsync(CancellationToken ct)
    {
        using var activity = InfrastructureActivitySource.Instance.StartActivity(
            "InboxRetry.Poll", ActivityKind.Internal);

        IReadOnlyList<InboxMessageSnapshot> due;

        try
        {
            using var scope = _serviceProvider.CreateAsyncScope();
            var inboxStore = scope.ServiceProvider.GetRequiredService<IInboxStore>();
            due = await inboxStore.GetDueForRetryAsync(_options.BatchSize, ct);
            activity?.SetTag("inbox.retries.count", due.Count);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _logger.LogError(ex, "Error fetching due Inbox retries");
            return;
        }

        if (due.Count == 0)
            return;

        _logger.LogDebug("Processing {Count} due Inbox retries", due.Count);

        foreach (var row in due)
        {
            try
            {
                await RetryOneAsync(row, ct);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.AddException(ex);
                _logger.LogError(
                    ex,
                    "Unexpected error retrying message {MessageId} for consumer {ConsumerName}",
                    row.MessageId, row.ConsumerName);
            }
        }
    }

    private async Task RetryOneAsync(InboxMessageSnapshot row, CancellationToken ct)
    {
        using var activity = InfrastructureActivitySource.Instance.StartActivity(
            "InboxRetry.RetryMessage", ActivityKind.Internal);
        activity?.SetTag("messaging.message.id", row.MessageId);
        activity?.SetTag("messaging.consumer.name", row.ConsumerName);

        // Fresh scope per message, same as the live Kafka dispatch path - keeps each retry's
        // business persistence isolated to its own DbContext/change tracker.
        using var scope = _serviceProvider.CreateAsyncScope();

        var consumers = scope.ServiceProvider.GetRequiredService<IEnumerable<IIntegrationEventConsumer>>();
        var consumer = consumers.FirstOrDefault(c => c.GetType().Name == row.ConsumerName);

        if (consumer is null)
        {
            _logger.LogWarning(
                "No registered consumer named {ConsumerName} for due retry {MessageId} - skipping until the next poll",
                row.ConsumerName, row.MessageId);
            return;
        }

        var inboxStore = scope.ServiceProvider.GetRequiredService<IInboxStore>();
        var executor = scope.ServiceProvider.GetRequiredService<InboxAttemptExecutor>();
        var headers = DeserializeHeaders(row.HeadersJson);

        await executor.ExecuteAsync(
            inboxStore,
            row.MessageId,
            row.ConsumerName,
            row.Topic,
            row.Payload,
            row.HeadersJson,
            () => consumer.HandleAsync(row.Payload, headers, ct),
            ct);
    }

    private static IReadOnlyDictionary<string, string> DeserializeHeaders(string headersJson)
    {
        if (string.IsNullOrEmpty(headersJson))
            return new Dictionary<string, string>();

        return JsonSerializer.Deserialize<Dictionary<string, string>>(headersJson)
            ?? new Dictionary<string, string>();
    }
}
