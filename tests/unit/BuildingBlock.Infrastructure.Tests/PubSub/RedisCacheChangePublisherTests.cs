using System.Text.Json;

using NovaCore.BuildingBlock.Application.Abstractions.Caching;
using NovaCore.BuildingBlock.Infrastructure.PubSub;
using NovaCore.BuildingBlock.SharedKernel.Serialization;

using NSubstitute;

using Shouldly;

using StackExchange.Redis;

using Xunit;

namespace NovaCore.BuildingBlock.Infrastructure.Tests.PubSub;

public sealed class RedisCacheChangePublisherTests
{
    private readonly ISubscriber _subscriber = Substitute.For<ISubscriber>();
    private readonly IConnectionMultiplexer _connectionMultiplexer = Substitute.For<IConnectionMultiplexer>();

    public RedisCacheChangePublisherTests()
    {
        _connectionMultiplexer.GetSubscriber().Returns(_subscriber);
    }

    [Fact]
    public async Task PublishAsync_PublishesToTheGivenChannel_WithJsonSerializedMessage()
    {
        RedisChannel capturedChannel = default;
        RedisValue capturedPayload = default;
        _subscriber.PublishAsync(Arg.Do<RedisChannel>(c => capturedChannel = c), Arg.Do<RedisValue>(v => capturedPayload = v))
            .Returns(1L);

        var sut = new RedisCacheChangePublisher(_connectionMultiplexer);
        var message = new CacheChangeMessage("perm:acc:1", 7, CacheChangeOperation.Invalidate, DateTimeOffset.UtcNow);

        await sut.PublishAsync("authorization.permission.changed", message);

        capturedChannel.ToString().ShouldBe("authorization.permission.changed");
        var deserialized = JsonSerializer.Deserialize<CacheChangeMessage>(capturedPayload.ToString(), JsonSerializerConfiguration.Default);
        deserialized.ShouldBe(message);
    }
}
