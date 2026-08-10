using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using Microsoft.Extensions.Configuration;

using NovaCore.User.Application.Abstractions.Persistence.Users;
using NovaCore.User.Application.Abstractions.Services;
using NovaCore.User.Application.Features.Users.DTOs;

namespace NovaCore.User.Infrastructure.Caching.Users;

/// <summary>
/// Owns the complete User Detail cache lifecycle for one specific question - "what does this
/// user's profile look like right now" - single (<see cref="GetByIdAsync"/>) and batch
/// (<see cref="GetByIdsAsync"/>): check Redis, on miss read through to persistence, refresh
/// Redis, return. Implements IUserProfileDetailCache - a distinct, cache-specific abstraction -
/// rather than decorating IUserReadService/IUserWriteService: this project deliberately rejects
/// the decorator pattern for cache invalidation/refresh (see docs/reference/caching.md), because
/// it hides cache synchronization behind an unrelated interface and lets a persistence call
/// silently trigger cache updates regardless of whether the surrounding business workflow (e.g. an
/// outer transaction) actually completed. Explicit callers (GetUserByIdHandler,
/// GetUserDetailHandler, GetUsersByIdsHandler for reads; UpdateUserHandler, OnUserDeletionHandler
/// for InvalidateAsync) decide when to call this, in plain sight in their own Handle methods.
/// GetAllAsync-equivalent bulk reads (search reindex) go straight to IUserReadService instead -
/// not worth caching, so Application depends on IUserReadService directly for that path.
/// </summary>
public sealed class UserProfileDetailCache(
    IUserReadService userReadService,
    ICacheService cacheService,
    IConfiguration configuration) : IUserProfileDetailCache
{
    private readonly TimeSpan _defaultTtl = TimeSpan.FromMinutes(
        configuration
            .GetSection("Caching:EntityTtl:UserProfiles:MinutesToExpire")
            .Get<int?>() ?? CacheKeyConstant.UserProfiles.DefaultTtlMinutes);

    public async Task<UserReadModel?> GetByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var key = CacheKeyConstant.UserProfiles.Detail(userId);
        var cached = await cacheService.GetAsync<UserReadModel>(key, ct);
        if (cached is not null)
            return cached;

        var user = await userReadService.GetByIdAsync(userId, ct);
        if (user is null)
            return null;

        await cacheService.SetAsync(key, user, _defaultTtl, ct);
        return user;
    }

    public async Task<IReadOnlyDictionary<Guid, UserReadModel>> GetByIdsAsync(IReadOnlyCollection<Guid> userIds, CancellationToken ct = default)
    {
        if (userIds.Count == 0)
            return new Dictionary<Guid, UserReadModel>();

        var idsByKey = userIds.ToDictionary(CacheKeyConstant.UserProfiles.Detail, id => id);
        var cachedByKey = await cacheService.GetManyAsync<UserReadModel>(idsByKey.Keys, ct);

        var result = new Dictionary<Guid, UserReadModel>();
        foreach (var (key, value) in cachedByKey)
        {
            if (value is not null)
                result[idsByKey[key]] = value;
        }

        var missingIds = userIds.Where(id => !result.ContainsKey(id)).ToList();
        if (missingIds.Count > 0)
        {
            var freshUsers = await userReadService.GetByIdsAsync(missingIds, ct);
            if (freshUsers.Count > 0)
            {
                var freshByKey = freshUsers.ToDictionary(u => CacheKeyConstant.UserProfiles.Detail(u.Id), u => u);
                await cacheService.SetManyAsync(freshByKey, _defaultTtl, ct);
            }

            foreach (var user in freshUsers)
                result[user.Id] = user;
        }

        return result;
    }

    public async Task InvalidateAsync(Guid userId, CancellationToken ct = default)
    {
        var key = CacheKeyConstant.UserProfiles.Detail(userId);
        await cacheService.RemoveAsync(key, ct);
    }
}
