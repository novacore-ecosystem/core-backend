using System.Text.Json;

using NovaCore.BuildingBlock.Application.Abstractions.Caching;
using NovaCore.BuildingBlock.Infrastructure.PubSub;
using NovaCore.BuildingBlock.SharedKernel.Serialization;

using Microsoft.Extensions.Logging;

using NSubstitute;

using Shouldly;

using StackExchange.Redis;

using Xunit;

namespace NovaCore.BuildingBlock.Infrastructure.Tests.PubSub;

/// <summary>
/// StackExchange.Redis's <c>ChannelMessageQueue</c> (the result of <c>ISubscriber.SubscribeAsync</c>)
/// is a sealed type with no public constructor, so the subscribe-and-dispatch wiring in
/// <c>ExecuteAsync</c> cannot be exercised end-to-end without a real Redis connection. These tests
/// instead exercise <see cref="RedisCacheChangeSubscriberHostedService.GroupListenersByChannel"/> and
/// <see cref="RedisCacheChangeSubscriberHostedService.Dispatch"/> directly - both made <c>internal</c>
/// specifically so the reliability logic (channel routing, per-listener failure isolation,
/// malformed-payload handling) is unit-testable in isolation from the transport.
/// </summary>
public sealed class RedisCacheChangeSubscriberHostedServiceTests
{
    private readonly ILogger<RedisCacheChangeSubscriberHostedService> _logger = Substitute.For<ILogger<RedisCacheChangeSubscriberHostedService>>();

    private RedisCacheChangeSubscriberHostedService CreateSut(params ICacheChangeListener[] listeners)
        => new(Substitute.For<IConnectionMultiplexer>(), listeners, _logger);

    private static RedisValue Payload(CacheChangeMessage message)
        => JsonSerializer.Serialize(message, JsonSerializerConfiguration.Default);

    [Fact]
    public void GroupListenersByChannel_GroupsMultipleListenersUnderTheSameChannel()
    {
        var listenerA = Substitute.For<ICacheChangeListener>();
        listenerA.Channel.Returns("channel-a");
        var listenerB = Substitute.For<ICacheChangeListener>();
        listenerB.Channel.Returns("channel-a");
        var listenerC = Substitute.For<ICacheChangeListener>();
        listenerC.Channel.Returns("channel-b");

        var grouped = RedisCacheChangeSubscriberHostedService.GroupListenersByChannel([listenerA, listenerB, listenerC]);

        grouped.Keys.ShouldBe(["channel-a", "channel-b"], ignoreOrder: true);
        grouped["channel-a"].Count.ShouldBe(2);
        grouped["channel-b"].Count.ShouldBe(1);
    }

    [Fact]
    public void Dispatch_DeliversMessage_OnlyToListenersOnThatChannel()
    {
        var listenerA = Substitute.For<ICacheChangeListener>();
        listenerA.Channel.Returns("channel-a");
        var listenerB = Substitute.For<ICacheChangeListener>();
        listenerB.Channel.Returns("channel-b");
        var listenersByChannel = RedisCacheChangeSubscriberHostedService.GroupListenersByChannel([listenerA, listenerB]);
        var sut = CreateSut(listenerA, listenerB);
        var message = new CacheChangeMessage("some-key", 1, CacheChangeOperation.Invalidate, DateTimeOffset.UtcNow);

        sut.Dispatch("channel-a", Payload(message), listenersByChannel);

        listenerA.Received(1).OnCacheChange(Arg.Is<CacheChangeMessage>(m => m!.Key == "some-key"));
        listenerB.DidNotReceive().OnCacheChange(Arg.Any<CacheChangeMessage>());
    }

    [Fact]
    public void Dispatch_ContinuesToOtherListeners_WhenOneListenerThrows()
    {
        var throwingListener = Substitute.For<ICacheChangeListener>();
        throwingListener.Channel.Returns("channel-a");
        throwingListener.When(l => l.OnCacheChange(Arg.Any<CacheChangeMessage>())).Do(_ => throw new InvalidOperationException("boom"));
        var healthyListener = Substitute.For<ICacheChangeListener>();
        healthyListener.Channel.Returns("channel-a");
        var listenersByChannel = RedisCacheChangeSubscriberHostedService.GroupListenersByChannel([throwingListener, healthyListener]);
        var sut = CreateSut(throwingListener, healthyListener);
        var message = new CacheChangeMessage("some-key", 1, CacheChangeOperation.Invalidate, DateTimeOffset.UtcNow);

        Should.NotThrow(() => sut.Dispatch("channel-a", Payload(message), listenersByChannel));

        healthyListener.Received(1).OnCacheChange(Arg.Any<CacheChangeMessage>());
    }

    [Fact]
    public void Dispatch_IsNoOp_ForAChannelWithNoRegisteredListeners()
    {
        var listenersByChannel = RedisCacheChangeSubscriberHostedService.GroupListenersByChannel([]);
        var sut = CreateSut();
        var message = new CacheChangeMessage("some-key", 1, CacheChangeOperation.Invalidate, DateTimeOffset.UtcNow);

        Should.NotThrow(() => sut.Dispatch("unknown-channel", Payload(message), listenersByChannel));
    }

    [Fact]
    public void Dispatch_SwallowsMalformedPayload_WithoutThrowing()
    {
        var listener = Substitute.For<ICacheChangeListener>();
        listener.Channel.Returns("channel-a");
        var listenersByChannel = RedisCacheChangeSubscriberHostedService.GroupListenersByChannel([listener]);
        var sut = CreateSut(listener);

        Should.NotThrow(() => sut.Dispatch("channel-a", "not-valid-json", listenersByChannel));

        listener.DidNotReceive().OnCacheChange(Arg.Any<CacheChangeMessage>());
    }
}
