using NovaCore.BuildingBlock.Application.Abstractions.Caching;
using NovaCore.BuildingBlock.Application.Abstractions.Idempotency;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Infrastructure.Caching;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NSubstitute;

using Shouldly;

using Xunit;

namespace NovaCore.BuildingBlock.Infrastructure.Tests.Caching;

public sealed class LayeredVersionedCacheTests
{
    private readonly IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());
    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();
    private readonly IDistributedLockProvider _lockProvider = Substitute.For<IDistributedLockProvider>();
    private readonly ICacheChangePublisher _publisher = Substitute.For<ICacheChangePublisher>();
    private readonly ILogger<LayeredVersionedCache<string>> _logger = Substitute.For<ILogger<LayeredVersionedCache<string>>>();

    private LayeredVersionedCache<string> CreateSut() => new(
        _memoryCache, _cacheService, _lockProvider, _publisher,
        Options.Create(new VersionedCacheOptions<string>
        {
            CacheName = "Test",
            ChannelName = "test.channel",
            LocalMemoryTtl = TimeSpan.FromMinutes(2),
            RedisTtl = TimeSpan.FromMinutes(30),
            LockExpiration = TimeSpan.FromSeconds(10),
            LockTimeout = TimeSpan.FromMilliseconds(50)
        }),
        _logger);

    private static VersionedCacheEntry<string> Entry(string value = "v1", long version = 1) => new(value, version, DateTimeOffset.UtcNow);

    [Fact]
    public async Task GetOrRefreshAsync_ReturnsL1_WithoutCallingRefreshFactory_WhenLocalMemoryHits()
    {
        var key = Guid.NewGuid().ToString();
        _memoryCache.Set(key, Entry("cached"), TimeSpan.FromMinutes(2));
        var sut = CreateSut();
        var refreshCalls = 0;

        var result = await sut.GetOrRefreshAsync(key, _ => { refreshCalls++; return Task.FromResult<VersionedCacheEntry<string>?>(Entry("fresh")); });

        result!.Value.ShouldBe("cached");
        refreshCalls.ShouldBe(0);
        await _cacheService.DidNotReceive().GetAsync<VersionedCacheEntry<string>>(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOrRefreshAsync_PopulatesL1AndSkipsRefreshFactory_WhenRedisHits()
    {
        var key = Guid.NewGuid().ToString();
        _cacheService.GetAsync<VersionedCacheEntry<string>>(key, Arg.Any<CancellationToken>())
            .Returns(Entry("from-redis"));
        var sut = CreateSut();
        var refreshCalls = 0;

        var result = await sut.GetOrRefreshAsync(key, _ => { refreshCalls++; return Task.FromResult<VersionedCacheEntry<string>?>(Entry("fresh")); });

        result!.Value.ShouldBe("from-redis");
        refreshCalls.ShouldBe(0);
        _memoryCache.TryGetValue(key, out VersionedCacheEntry<string>? cached).ShouldBeTrue();
        cached!.Value.ShouldBe("from-redis");
    }

    [Fact]
    public async Task GetOrRefreshAsync_CallsRefreshFactoryOnce_AndPopulatesBothLayers_OnDoubleMiss()
    {
        var key = Guid.NewGuid().ToString();
        _cacheService.GetAsync<VersionedCacheEntry<string>>(key, Arg.Any<CancellationToken>())
            .Returns((VersionedCacheEntry<string>?)null);
        var acquiredLock = Substitute.For<IDistributedLock>();
        _lockProvider.AcquireAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(acquiredLock);
        var sut = CreateSut();
        var refreshCalls = 0;

        var result = await sut.GetOrRefreshAsync(key, _ => { refreshCalls++; return Task.FromResult<VersionedCacheEntry<string>?>(Entry("from-source")); });

        result!.Value.ShouldBe("from-source");
        refreshCalls.ShouldBe(1);
        _memoryCache.TryGetValue(key, out VersionedCacheEntry<string>? cached).ShouldBeTrue();
        cached!.Value.ShouldBe("from-source");
        await _cacheService.Received(1).SetAsync(key, Arg.Is<VersionedCacheEntry<string>>(e => e!.Value == "from-source"), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOrRefreshAsync_DoesNotCache_WhenRefreshFactoryReturnsNull()
    {
        var key = Guid.NewGuid().ToString();
        _cacheService.GetAsync<VersionedCacheEntry<string>>(key, Arg.Any<CancellationToken>())
            .Returns((VersionedCacheEntry<string>?)null);
        var sut = CreateSut();

        var result = await sut.GetOrRefreshAsync(key, _ => Task.FromResult<VersionedCacheEntry<string>?>(null));

        result.ShouldBeNull();
        _memoryCache.TryGetValue(key, out VersionedCacheEntry<string>? _).ShouldBeFalse();
        await _cacheService.DidNotReceive().SetAsync(Arg.Any<string>(), Arg.Any<VersionedCacheEntry<string>>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOrRefreshAsync_InvokesRefreshFactoryExactlyOnce_ForConcurrentCallersOnTheSameKey()
    {
        var key = Guid.NewGuid().ToString();
        _cacheService.GetAsync<VersionedCacheEntry<string>>(key, Arg.Any<CancellationToken>())
            .Returns((VersionedCacheEntry<string>?)null);
        var sut = CreateSut();
        var refreshCalls = 0;

        Task<VersionedCacheEntry<string>?> RefreshFactory(CancellationToken ct)
        {
            Interlocked.Increment(ref refreshCalls);
            return Task.FromResult<VersionedCacheEntry<string>?>(Entry("from-source"));
        }

        var results = await Task.WhenAll(Enumerable.Range(0, 50).Select(_ => sut.GetOrRefreshAsync(key, RefreshFactory)));

        refreshCalls.ShouldBe(1);
        results.ShouldAllBe(r => r!.Value == "from-source");
    }

    [Fact]
    public async Task GetOrRefreshAsync_AcquiresLock_WithPermCacheRefreshPrefixedResource_OnDoubleMiss()
    {
        var key = Guid.NewGuid().ToString();
        _cacheService.GetAsync<VersionedCacheEntry<string>>(key, Arg.Any<CancellationToken>())
            .Returns((VersionedCacheEntry<string>?)null);
        var acquiredLock = Substitute.For<IDistributedLock>();
        _lockProvider.AcquireAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(acquiredLock);
        var sut = CreateSut();

        await sut.GetOrRefreshAsync(key, _ => Task.FromResult<VersionedCacheEntry<string>?>(Entry()));

        await _lockProvider.Received(1).AcquireAsync($"permcache-refresh:{key}", Arg.Any<TimeSpan>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
        await acquiredLock.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task GetOrRefreshAsync_FallsBackToRefreshFactory_WithoutHanging_WhenLockIsContended()
    {
        var key = Guid.NewGuid().ToString();
        _cacheService.GetAsync<VersionedCacheEntry<string>>(key, Arg.Any<CancellationToken>())
            .Returns((VersionedCacheEntry<string>?)null);
        _lockProvider.AcquireAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns((IDistributedLock?)null);
        var sut = CreateSut();
        var refreshCalls = 0;

        var result = await sut.GetOrRefreshAsync(key, _ => { refreshCalls++; return Task.FromResult<VersionedCacheEntry<string>?>(Entry("from-source")); });

        result!.Value.ShouldBe("from-source");
        refreshCalls.ShouldBe(1);
        await _cacheService.Received(2).GetAsync<VersionedCacheEntry<string>>(key, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOrRefreshAsync_TreatsRedisReadFailureAsMiss_AndStillCallsRefreshFactory()
    {
        var key = Guid.NewGuid().ToString();
        _cacheService.GetAsync<VersionedCacheEntry<string>>(key, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<VersionedCacheEntry<string>?>(new InvalidOperationException("redis down")));
        var sut = CreateSut();
        var refreshCalls = 0;

        var result = await sut.GetOrRefreshAsync(key, _ => { refreshCalls++; return Task.FromResult<VersionedCacheEntry<string>?>(Entry("from-source")); });

        result!.Value.ShouldBe("from-source");
        refreshCalls.ShouldBe(1);
    }

    [Fact]
    public async Task InvalidateAsync_RemovesL1AndL2_AndPublishesInvalidate()
    {
        var key = Guid.NewGuid().ToString();
        _memoryCache.Set(key, Entry(), TimeSpan.FromMinutes(2));
        var sut = CreateSut();

        await sut.InvalidateAsync(key, newVersion: 7);

        _memoryCache.TryGetValue(key, out VersionedCacheEntry<string>? _).ShouldBeFalse();
        await _cacheService.Received(1).RemoveAsync(key, Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            "test.channel",
            Arg.Is<CacheChangeMessage>(m => m!.Key == key && m.Version == 7 && m.Operation == CacheChangeOperation.Invalidate),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvalidateAsync_StillClearsL1AndL2_WhenPublishThrows()
    {
        var key = Guid.NewGuid().ToString();
        _memoryCache.Set(key, Entry(), TimeSpan.FromMinutes(2));
        _publisher.PublishAsync(Arg.Any<string>(), Arg.Any<CacheChangeMessage>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("redis down")));
        var sut = CreateSut();

        await sut.InvalidateAsync(key);

        _memoryCache.TryGetValue(key, out VersionedCacheEntry<string>? _).ShouldBeFalse();
        await _cacheService.Received(1).RemoveAsync(key, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void OnCacheChange_EvictsUnconditionally_ForInvalidate()
    {
        var key = Guid.NewGuid().ToString();
        _memoryCache.Set(key, Entry(version: 99), TimeSpan.FromMinutes(2));
        var sut = CreateSut();

        sut.OnCacheChange(new CacheChangeMessage(key, Version: 1, CacheChangeOperation.Invalidate, DateTimeOffset.UtcNow));

        _memoryCache.TryGetValue(key, out VersionedCacheEntry<string>? _).ShouldBeFalse();
    }

    [Fact]
    public void OnCacheChange_EvictsOnlyWhenNewer_ForRefresh()
    {
        var staleKey = Guid.NewGuid().ToString();
        var freshKey = Guid.NewGuid().ToString();
        _memoryCache.Set(staleKey, Entry(version: 1), TimeSpan.FromMinutes(2));
        _memoryCache.Set(freshKey, Entry(version: 5), TimeSpan.FromMinutes(2));
        var sut = CreateSut();

        sut.OnCacheChange(new CacheChangeMessage(staleKey, Version: 2, CacheChangeOperation.Refresh, DateTimeOffset.UtcNow));
        sut.OnCacheChange(new CacheChangeMessage(freshKey, Version: 3, CacheChangeOperation.Refresh, DateTimeOffset.UtcNow));

        _memoryCache.TryGetValue(staleKey, out VersionedCacheEntry<string>? _).ShouldBeFalse();
        _memoryCache.TryGetValue(freshKey, out VersionedCacheEntry<string>? stillCached).ShouldBeTrue();
        stillCached!.Version.ShouldBe(5);
    }

    [Fact]
    public void OnCacheChange_NeverTouchesUnrelatedKeys()
    {
        var targetKey = Guid.NewGuid().ToString();
        var otherKey = Guid.NewGuid().ToString();
        _memoryCache.Set(targetKey, Entry(), TimeSpan.FromMinutes(2));
        _memoryCache.Set(otherKey, Entry(), TimeSpan.FromMinutes(2));
        var sut = CreateSut();

        sut.OnCacheChange(new CacheChangeMessage(targetKey, Version: 1, CacheChangeOperation.Invalidate, DateTimeOffset.UtcNow));

        _memoryCache.TryGetValue(otherKey, out VersionedCacheEntry<string>? _).ShouldBeTrue();
    }
}
