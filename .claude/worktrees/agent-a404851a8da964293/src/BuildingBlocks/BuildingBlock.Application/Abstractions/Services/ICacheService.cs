namespace NovaCore.BuildingBlock.Application.Abstractions.Services;

/// <summary>
/// Distributed cache service abstraction. Supports single and batch operations.
/// See /docs/CACHE_SERVICE.md for detailed documentation.
/// </summary>
public interface ICacheService
{
    /// <summary>Gets a single value from cache</summary>
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);

    /// <summary>Gets multiple values from cache in one round trip</summary>
    Task<IDictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken ct = default);

    /// <summary>Sets a single value in cache with optional expiration</summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default);

    /// <summary>Sets multiple values in cache in one round trip</summary>
    Task SetManyAsync<T>(IDictionary<string, T> items, TimeSpan? expiration = null, CancellationToken ct = default);

    /// <summary>Removes a single key from cache</summary>
    Task RemoveAsync(string key, CancellationToken ct = default);

    /// <summary>Removes multiple keys from cache</summary>
    Task RemoveManyAsync(IEnumerable<string> keys, CancellationToken ct = default);

    /// <summary>Removes all keys matching a pattern (e.g., "user:*")</summary>
    Task RemoveByPatternAsync(string pattern, CancellationToken ct = default);

    /// <summary>Checks if a key exists in cache</summary>
    Task<bool> ExistsAsync(string key, CancellationToken ct = default);

    /// <summary>Adds a member to a Redis set (SADD). Returns true if the member was newly added.</summary>
    Task<bool> SetAddAsync(string key, string member, CancellationToken ct = default);

    /// <summary>Removes a member from a Redis set (SREM). Returns true if the member was present.</summary>
    Task<bool> SetRemoveAsync(string key, string member, CancellationToken ct = default);

    /// <summary>Gets all members of a Redis set (SMEMBERS)</summary>
    Task<IReadOnlyList<string>> SetMembersAsync(string key, CancellationToken ct = default);

    /// <summary>Gets the number of members in a Redis set (SCARD)</summary>
    Task<long> SetLengthAsync(string key, CancellationToken ct = default);

    /// <summary>Sets a single field in a Redis hash (HSET). Field-level write, does not affect other fields</summary>
    Task HashSetAsync<T>(string key, string field, T value, CancellationToken ct = default);

    /// <summary>Removes a single field from a Redis hash (HDEL)</summary>
    Task<bool> HashDeleteAsync(string key, string field, CancellationToken ct = default);

    /// <summary>Removes multiple fields from a Redis hash in one round trip (HDEL)</summary>
    Task HashDeleteManyAsync(string key, IEnumerable<string> fields, CancellationToken ct = default);

    /// <summary>Gets all fields and values of a Redis hash in one round trip (HGETALL)</summary>
    Task<IDictionary<string, T?>> HashGetAllAsync<T>(string key, CancellationToken ct = default);

    /// <summary>Gets the number of fields in a Redis hash (HLEN)</summary>
    Task<long> HashLengthAsync(string key, CancellationToken ct = default);
}
