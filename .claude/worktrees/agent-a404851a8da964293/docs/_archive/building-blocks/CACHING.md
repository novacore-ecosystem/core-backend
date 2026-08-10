# Redis Cache Service

`ICacheService` is a distributed caching abstraction backed by Redis (`RedisCacheService`). It supports single and batch operations, JSON serialization, configurable TTL, and pattern-based key removal.

For the higher-level role-caching decorator pattern built on top of this, see [../guides/ROLE_CACHING.md](../guides/ROLE_CACHING.md).

## Architecture

```
Service Layer (Auth.API, Order.API, Product.API, ...)
    │ injects ICacheService
    ▼
BuildingBlock.Application.Abstractions.Services.ICacheService
    │ implemented by
    ▼
BuildingBlock.Infrastructure.Caching.RedisCacheService
    │ uses StackExchange.Redis.IConnectionMultiplexer (singleton, connection pooled)
    ▼
Redis Server
```

## Quick Start

### 1. Configuration

```json
{
  "Cache": {
    "ConnectionString": "redis:6379",
    "DefaultExpirationMinutes": 60,
    "KeyPrefix": "servicename:",
    "ConnectionTimeout": 5000
  }
}
```

### 2. Register

```csharp
builder.Services.AddRedisCache(builder.Configuration);
```

Other registration options:
```csharp
// Explicit options
services.AddRedisCache(new CacheOptions { ConnectionString = "redis:6379" });

// Simple connection string
services.AddRedisCache("redis:6379");
```

### 3. Use

```csharp
public class UserService(ICacheService cacheService)
{
    public async Task<User?> GetUserAsync(int userId)
    {
        var key = $"user:{userId}";
        var user = await cacheService.GetAsync<User>(key);
        if (user != null)
            return user;

        user = await _database.GetUserAsync(userId);
        if (user != null)
            await cacheService.SetAsync(key, user, TimeSpan.FromHours(1));

        return user;
    }
}
```

## Interface Methods

### Single Operations
| Method | Notes |
|---|---|
| `GetAsync<T>(key)` | Returns deserialized value or null |
| `SetAsync<T>(key, value, expiration?)` | Throws `ArgumentException`/`ArgumentNullException` on bad input |
| `RemoveAsync(key)` | Silently succeeds if key doesn't exist |
| `ExistsAsync(key)` | Returns bool |

### Batch Operations (use these for multiple keys — 6-100x faster than looping)
| Method | Notes |
|---|---|
| `GetManyAsync<T>(keys)` | ~8ms for 100 items vs ~100ms with individual calls |
| `SetManyAsync<T>(items, expiration?)` | All items share the same expiration |
| `RemoveManyAsync(keys)` | Single round trip |

### Pattern-Based
| Method | Notes |
|---|---|
| `RemoveByPatternAsync(pattern)` | e.g. `"user:*"`, `"order:123:*"`, `"*"` |

## Common Patterns

### Cache-Aside
```csharp
var cached = await cacheService.GetAsync<User>(cacheKey);
if (cached != null) return cached;

var user = await database.GetUserAsync(userId);
await cacheService.SetAsync(cacheKey, user, TimeSpan.FromHours(1));
return user;
```

### Batch Cache-Aside
```csharp
var cacheKeys = userIds.Select(id => $"user:{id}").ToList();
var cached = await cacheService.GetManyAsync<User>(cacheKeys);

var missingIds = userIds.Where(id => cached[$"user:{id}"] == null).ToList();
if (missingIds.Any())
{
    var dbUsers = await database.GetByIdsAsync(missingIds);
    await cacheService.SetManyAsync(dbUsers.ToDictionary(u => $"user:{u.Id}"), TimeSpan.FromHours(1));
}
```

### Invalidation
```csharp
public async Task UpdateUserAsync(User user)
{
    await database.UpdateAsync(user);
    await cacheService.RemoveAsync($"user:{user.Id}");
    await cacheService.RemoveByPatternAsync("user:*:*"); // related derived data
}
```

## Configuration Reference

| Option | Default | Notes |
|---|---|---|
| `ConnectionString` | *required* | `hostname:port`, e.g. `redis:6379` |
| `DefaultExpirationMinutes` | 60 | Reference only — each entry can set its own TTL |
| `KeyPrefix` | empty | Namespace per service to avoid collisions on a shared Redis instance |
| `ConnectionTimeout` | 5000ms | Connection and command execution timeout |

### Multi-Service Key Prefixes

All services share one Redis instance; `KeyPrefix` keeps their keys from colliding:

```json
// Auth:    { "Cache": { "KeyPrefix": "auth:" } }
// Order:   { "Cache": { "KeyPrefix": "order:" } }
// Product: { "Cache": { "KeyPrefix": "product:" } }
```

### Key Naming Convention

`service:entity:id[:attribute]` — e.g. `auth:user:123`, `order:order:456:items`, `inventory:stock:789:location`.

### TTL Guidance

| Data type | Suggested TTL |
|---|---|
| Session data | 15-30 min |
| User/role data | 30-60 min |
| Reference/catalog data | 2-4 hours |
| Stable config/hierarchy data | 4+ hours |

## Performance Characteristics

- Single Get (hit): 1-5ms
- Single Get (miss, includes DB call): ~50ms
- Batch Get (100 items): ~8ms vs ~100ms with individual calls

## Testing

Mock `ICacheService` — do not spin up a real Redis for unit tests:

```csharp
var cacheMock = new Mock<ICacheService>();
cacheMock.Setup(c => c.GetAsync<User>("user:1", default)).ReturnsAsync(user);

var service = new UserService(cacheMock.Object);
var result = await service.GetUserAsync(1);
```

## Troubleshooting

| Issue | Fix |
|---|---|
| Connection timeout | `docker-compose ps redis`, then `redis-cli ping` |
| Serialization error | Ensure cached objects are JSON-serializable |
| Key not found unexpectedly | Check `KeyPrefix` configuration matches between write/read |
| Memory full | `redis-cli info memory`; consider `redis-cli CONFIG SET maxmemory-policy allkeys-lru` |
| Stale data | Add/verify invalidation on the corresponding write path |

## Best Practices

**DO**: use batch operations for multiple keys, set TTLs deliberately, invalidate on writes, namespace keys hierarchically, mock in unit tests.

**DON'T**: store sensitive data unencrypted, use very long TTLs "just in case", cache without an invalidation plan, share keys across services without a prefix.

## See Also

- `src/BuildingBlocks/BuildingBlock.Infrastructure/Caching/` — implementation
- `src/BuildingBlocks/BuildingBlock.Application/Abstractions/Services/ICacheService.cs` — interface
- [ROLE_CACHING.md](../guides/ROLE_CACHING.md) — decorator pattern built on this building block
