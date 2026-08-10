using System.Text.Json;

using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.SharedKernel.Serialization;

using StackExchange.Redis;

namespace NovaCore.BuildingBlock.Infrastructure.Caching;

internal sealed class RedisCacheService(IConnectionMultiplexer connectionMultiplexer) : ICacheService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var db = _connectionMultiplexer.GetDatabase();
        var value = await db.StringGetAsync(key);
        if (!value.HasValue)
            return default;
        return JsonSerializer.Deserialize<T>(value.ToString(), JsonSerializerConfiguration.Default);
    }

    public async Task<IDictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken ct = default)
    {
        var keysList = keys.ToList();
        if (keysList.Count == 0)
            return new Dictionary<string, T?>();

        var redisKeys = keysList.Select(k => (RedisKey)k).ToArray();
        var db = _connectionMultiplexer.GetDatabase();
        var values = await db.StringGetAsync(redisKeys);

        var result = new Dictionary<string, T?>(keysList.Count);
        for (int i = 0; i < keysList.Count; i++)
        {
            if (values[i].HasValue)
            {
                var deserialized = JsonSerializer.Deserialize<T>(values[i].ToString(), JsonSerializerConfiguration.Default);
                result[keysList[i]] = deserialized;
            }
            else
            {
                result[keysList[i]] = default;
            }
        }
        return result;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);
        var db = _connectionMultiplexer.GetDatabase();
        var serialized = JsonSerializer.Serialize(value, JsonSerializerConfiguration.Default);
        if (expiration.HasValue)
            await db.StringSetAsync(key, serialized, expiration.Value);
        else
            await db.StringSetAsync(key, serialized);
    }

    public async Task SetManyAsync<T>(IDictionary<string, T> items, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        if (items == null || !items.Any())
            return;

        var db = _connectionMultiplexer.GetDatabase();
        var keyValuePairs = new List<KeyValuePair<RedisKey, RedisValue>>(items.Count);

        foreach (var item in items)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(item.Key);
            ArgumentNullException.ThrowIfNull(item.Value);
            var serialized = JsonSerializer.Serialize(item.Value, JsonSerializerConfiguration.Default);
            keyValuePairs.Add(new(item.Key, serialized));
        }

        await db.StringSetAsync(keyValuePairs.ToArray());

        if (expiration.HasValue)
        {
            foreach (var item in items)
                await db.KeyExpireAsync(item.Key, expiration.Value);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var db = _connectionMultiplexer.GetDatabase();
        await db.KeyDeleteAsync(key);
    }

    public async Task RemoveManyAsync(IEnumerable<string> keys, CancellationToken ct = default)
    {
        var keysList = keys.ToList();
        if (keysList.Count == 0) return;

        var redisKeys = keysList.Select(k => (RedisKey)k).ToArray();
        var db = _connectionMultiplexer.GetDatabase();
        await db.KeyDeleteAsync(redisKeys);
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);
        var endpoints = _connectionMultiplexer.GetEndPoints();
        var keysToDelete = new List<RedisKey>();

        foreach (var endpoint in endpoints)
        {
            var server = _connectionMultiplexer.GetServer(endpoint);
            keysToDelete.AddRange(server.Keys(pattern: pattern));
        }

        if (keysToDelete.Count != 0)
        {
            var db = _connectionMultiplexer.GetDatabase();
            await db.KeyDeleteAsync([.. keysToDelete]);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var db = _connectionMultiplexer.GetDatabase();
        return await db.KeyExistsAsync(key);
    }

    public async Task<bool> SetAddAsync(string key, string member, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(member);
        var db = _connectionMultiplexer.GetDatabase();
        return await db.SetAddAsync(key, member);
    }

    public async Task<bool> SetRemoveAsync(string key, string member, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(member);
        var db = _connectionMultiplexer.GetDatabase();
        return await db.SetRemoveAsync(key, member);
    }

    public async Task<IReadOnlyList<string>> SetMembersAsync(string key, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var db = _connectionMultiplexer.GetDatabase();
        var members = await db.SetMembersAsync(key);
        return members.Select(m => m.ToString()).ToList();
    }

    public async Task<long> SetLengthAsync(string key, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var db = _connectionMultiplexer.GetDatabase();
        return await db.SetLengthAsync(key);
    }

    public async Task HashSetAsync<T>(string key, string field, T value, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(field);
        ArgumentNullException.ThrowIfNull(value);
        var db = _connectionMultiplexer.GetDatabase();
        var serialized = JsonSerializer.Serialize(value, JsonSerializerConfiguration.Default);
        await db.HashSetAsync(key, field, serialized);
    }

    public async Task<bool> HashDeleteAsync(string key, string field, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(field);
        var db = _connectionMultiplexer.GetDatabase();
        return await db.HashDeleteAsync(key, field);
    }

    public async Task HashDeleteManyAsync(string key, IEnumerable<string> fields, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var fieldsArray = fields.Select(f => (RedisValue)f).ToArray();
        if (fieldsArray.Length == 0)
            return;

        var db = _connectionMultiplexer.GetDatabase();
        await db.HashDeleteAsync(key, fieldsArray);
    }

    public async Task<IDictionary<string, T?>> HashGetAllAsync<T>(string key, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var db = _connectionMultiplexer.GetDatabase();
        var entries = await db.HashGetAllAsync(key);

        var result = new Dictionary<string, T?>(entries.Length);
        foreach (var entry in entries)
        {
            if (entry.Value.HasValue)
                result[entry.Name.ToString()] = JsonSerializer.Deserialize<T>(entry.Value.ToString(), JsonSerializerConfiguration.Default);
        }
        return result;
    }

    public async Task<long> HashLengthAsync(string key, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var db = _connectionMultiplexer.GetDatabase();
        return await db.HashLengthAsync(key);
    }
}
