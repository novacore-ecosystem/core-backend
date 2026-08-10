using NovaCore.BuildingBlock.Application.Abstractions.Outbox;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NovaCore.BuildingBlock.Infrastructure.Messaging;

/// <summary>
/// Owns the Inbox attempt lifecycle (begin -> invoke handler -> complete/fail) and its logging.
/// Shared by the live Kafka dispatch path (via the delegate registered in
/// MessagingInfrastructureExtensions) and InboxRetryHostedService, so there is exactly one place
/// that decides skip/proceed and interprets BeginAttemptAsync/FailAttemptAsync outcomes.
/// </summary>
public sealed class InboxAttemptExecutor(
    IOptions<InboxRetryOptions> options,
    ILogger<InboxAttemptExecutor> logger)
{
    private readonly InboxRetryOptions _options = options.Value;

    public async Task ExecuteAsync(
        IInboxStore inboxStore,
        Guid messageId,
        string consumerName,
        string topic,
        string payload,
        string headersJson,
        Func<Task> handlerAction,
        CancellationToken ct)
    {
        var decision = await inboxStore.BeginAttemptAsync(messageId, consumerName, topic, payload, headersJson, ct);

        switch (decision)
        {
            case InboxAttemptDecision.AlreadyProcessed:
                logger.LogInformation(
                    "Skipping already-processed message {MessageId} for consumer {ConsumerName}",
                    messageId, consumerName);
                return;
            case InboxAttemptDecision.DeadLettered:
                logger.LogWarning(
                    "Skipping dead-lettered message {MessageId} for consumer {ConsumerName} on topic {Topic} - not retried automatically",
                    messageId, consumerName, topic);
                return;
            case InboxAttemptDecision.NotDueYet:
                logger.LogDebug(
                    "Message {MessageId} for consumer {ConsumerName} is not due for retry yet",
                    messageId, consumerName);
                return;
        }

        logger.LogInformation(
            "Retry started: message {MessageId}, consumer {ConsumerName}, topic {Topic}",
            messageId, consumerName, topic);

        try
        {
            await handlerAction();
            await inboxStore.CompleteAttemptAsync(messageId, consumerName, ct);

            logger.LogInformation(
                "Retry succeeded: message {MessageId}, consumer {ConsumerName}, topic {Topic}",
                messageId, consumerName, topic);
        }
        catch (Exception ex)
        {
            var outcome = await inboxStore.FailAttemptAsync(messageId, consumerName, ex.Message, _options.ToPolicy(), ct);

            switch (outcome)
            {
                case InboxFailureOutcome.AlreadyCommitted:
                    logger.LogWarning(
                        ex,
                        "Consumer {ConsumerName} threw for message {MessageId} (topic {Topic}) after its own transaction had already committed - Inbox left Processed to avoid re-running business logic",
                        consumerName, messageId, topic);
                    break;
                case InboxFailureOutcome.DeadLettered:
                    logger.LogError(
                        ex,
                        "Retry failed and message moved to DeadLetter: message {MessageId}, consumer {ConsumerName}, topic {Topic}",
                        messageId, consumerName, topic);
                    break;
                default:
                    logger.LogWarning(
                        ex,
                        "Retry failed: message {MessageId}, consumer {ConsumerName}, topic {Topic} - will retry",
                        messageId, consumerName, topic);
                    break;
            }
        }
    }
}
