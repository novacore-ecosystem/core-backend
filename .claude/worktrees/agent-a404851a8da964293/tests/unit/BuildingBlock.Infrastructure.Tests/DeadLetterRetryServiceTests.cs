using NovaCore.BuildingBlock.Application.Abstractions.Idempotency;
using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Infrastructure.DeadLetters;
using NovaCore.BuildingBlock.Messaging.Abstractions;

using Microsoft.Extensions.Logging;

using NSubstitute;

using Shouldly;

using Xunit;

namespace NovaCore.BuildingBlock.Infrastructure.Tests;

public sealed class DeadLetterRetryServiceTests
{
    private readonly IInboxStore _inboxStore = Substitute.For<IInboxStore>();
    private readonly IOutboxPublisher _outboxPublisher = Substitute.For<IOutboxPublisher>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly ILogger<DeadLetterRetryService> _logger = Substitute.For<ILogger<DeadLetterRetryService>>();

    private static InboxMessageSnapshot Snapshot(Guid messageId) => new(
        messageId, "SomeConsumer", "some-topic", "{}",
        "{\"event-type\":\"SomethingHappened\",\"correlation-id\":\"corr-1\"}",
        InboxMessageStatus.Retrying, 0, DateTime.UtcNow, null, null, DateTime.UtcNow, null);

    [Fact]
    public async Task RetryAsync_Requeues_AndRepublishes_WhenNoDistributedLockRegistered()
    {
        // Services with no Redis (Audit, Inventory, Notification) resolve IDistributedLockProvider
        // as null - the retry must still succeed, relying solely on the DB-level atomic requeue.
        var inboxMessageId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        _inboxStore.RequeueDeadLetterAsync(inboxMessageId, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new InboxRequeueResult(InboxRequeueOutcome.Requeued, Snapshot(messageId), 1));

        var sut = new DeadLetterRetryService(_inboxStore, _outboxPublisher, null, _currentUser, _logger);

        var result = await sut.RetryAsync(inboxMessageId);

        result.Outcome.ShouldBe(DeadLetterRetryOutcome.Succeeded);
        await _outboxPublisher.Received(1).PublishOutboxMessageAsync(
            messageId, "some-topic", "{}", "SomethingHappened", "corr-1", null, "System", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RetryAsync_ReturnsConflict_WhenDistributedLockNotAcquired()
    {
        var inboxMessageId = Guid.NewGuid();
        var lockProvider = Substitute.For<IDistributedLockProvider>();
        lockProvider.AcquireAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns((IDistributedLock?)null);

        var sut = new DeadLetterRetryService(_inboxStore, _outboxPublisher, lockProvider, _currentUser, _logger);

        var result = await sut.RetryAsync(inboxMessageId);

        result.Outcome.ShouldBe(DeadLetterRetryOutcome.Conflict);
        await _inboxStore.DidNotReceive().RequeueDeadLetterAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RetryAsync_ReturnsNotFound_WhenRowDoesNotExist()
    {
        var inboxMessageId = Guid.NewGuid();
        _inboxStore.RequeueDeadLetterAsync(inboxMessageId, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new InboxRequeueResult(InboxRequeueOutcome.NotFound, null, 0));

        var sut = new DeadLetterRetryService(_inboxStore, _outboxPublisher, null, _currentUser, _logger);

        var result = await sut.RetryAsync(inboxMessageId);

        result.Outcome.ShouldBe(DeadLetterRetryOutcome.NotFound);
        await _outboxPublisher.DidNotReceive().PublishOutboxMessageAsync(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RetryAsync_ReturnsNotDeadLetter_WhenRowIsNotCurrentlyDeadLettered()
    {
        var inboxMessageId = Guid.NewGuid();
        _inboxStore.RequeueDeadLetterAsync(inboxMessageId, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new InboxRequeueResult(InboxRequeueOutcome.NotDeadLetter, null, 0));

        var sut = new DeadLetterRetryService(_inboxStore, _outboxPublisher, null, _currentUser, _logger);

        var result = await sut.RetryAsync(inboxMessageId);

        result.Outcome.ShouldBe(DeadLetterRetryOutcome.NotDeadLetter);
    }

    [Fact]
    public async Task RetryAsync_RevertsToDeadLetter_WhenPublishThrows()
    {
        var inboxMessageId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        _inboxStore.RequeueDeadLetterAsync(inboxMessageId, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new InboxRequeueResult(InboxRequeueOutcome.Requeued, Snapshot(messageId), 1));
        _outboxPublisher.PublishOutboxMessageAsync(
                Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("Kafka is down")));

        var sut = new DeadLetterRetryService(_inboxStore, _outboxPublisher, null, _currentUser, _logger);

        var result = await sut.RetryAsync(inboxMessageId);

        result.Outcome.ShouldBe(DeadLetterRetryOutcome.PublishFailed);
        await _inboxStore.Received(1).RevertFailedRequeueAsync(inboxMessageId, Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RetryAsync_ReleasesLock_EvenWhenPublishThrows()
    {
        var inboxMessageId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var lockProvider = Substitute.For<IDistributedLockProvider>();
        var acquiredLock = Substitute.For<IDistributedLock>();
        lockProvider.AcquireAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(acquiredLock);
        _inboxStore.RequeueDeadLetterAsync(inboxMessageId, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new InboxRequeueResult(InboxRequeueOutcome.Requeued, Snapshot(messageId), 1));
        _outboxPublisher.PublishOutboxMessageAsync(
                Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("Kafka is down")));

        var sut = new DeadLetterRetryService(_inboxStore, _outboxPublisher, lockProvider, _currentUser, _logger);

        await sut.RetryAsync(inboxMessageId);

        await acquiredLock.Received(1).DisposeAsync();
    }
}
