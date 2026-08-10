namespace NovaCore.BuildingBlock.SharedKernel.Constants;

/// <summary>
/// Centralized cache key patterns and configurations for all cached entities.
/// Follows the pattern: {service}:{entity}:{operation}:{id}
/// </summary>
public static class CacheKeyConstant
{
    /// <summary>User roles cache patterns and configuration</summary>
    public static class Roles
    {
        private const string Prefix = "auth:roles";

        /// <summary>Get roles cache key</summary>
        public const string RoleList = "auth:roles:list";

        /// <summary>Get user roles cache key. Pattern: auth:roles:user:{userId}</summary>
        public static string UserRoles(Guid userId) => $"{Prefix}:user:{userId}";

        /// <summary>Cache key pattern for all user roles. Used for pattern-based invalidation</summary>
        public const string UserRolesPattern = "auth:roles:user:*";

        /// <summary>Default TTL for role cache in minutes. Override in appsettings if needed</summary>
        public const int DefaultTtlMinutes = 30;
    }

    /// <summary>
    /// Dead scaffold - never wired to any code (kept, not deleted, to avoid churn unrelated to
    /// this change). The "auth:users" prefix was seeded for Auth's own account concept, NOT
    /// User service's UserProfile aggregate - see <see cref="UserProfiles"/> for the real,
    /// User-owned key group.
    /// </summary>
    public static class Users
    {
        private const string Prefix = "auth:users";

        /// <summary>Get user profile cache key. Pattern: auth:users:profile:{userId}</summary>
        public static string Profile(Guid userId) => $"{Prefix}:profile:{userId}";

        /// <summary>Get user by email cache key. Pattern: auth:users:email:{email}</summary>
        public static string Email(string email) => $"{Prefix}:email:{email}";

        /// <summary>Cache key pattern for all user cache entries. Used for pattern-based invalidation</summary>
        public const string AllPattern = "auth:users:*";

        /// <summary>Default TTL for user cache in minutes</summary>
        public const int DefaultTtlMinutes = 60;
    }

    /// <summary>
    /// User service's own UserProfile detail cache (read-through: cache -&gt; DB, invalidated on
    /// Create/Update/Delete). Deliberately a different, correctly-namespaced key group from the
    /// dead <see cref="Users"/> scaffold above - see docs/tasks/2026-07-28/Task11_user-detail-cache-scaffold.md.
    /// User and Auth share one physical Redis instance, so a distinct prefix matters, not just style.
    /// </summary>
    public static class UserProfiles
    {
        private const string Prefix = "user:users";

        /// <summary>Get user profile detail cache key. Pattern: user:users:detail:{userId}</summary>
        public static string Detail(Guid userId) => $"{Prefix}:detail:{userId}";

        /// <summary>Cache key pattern for all user profile detail entries. Used for pattern-based invalidation</summary>
        public const string DetailPattern = "user:users:detail:*";

        /// <summary>Default TTL in minutes - short, per the read-through cache's design (short-lived, refreshed on read, invalidated on write)</summary>
        public const int DefaultTtlMinutes = 10;
    }

    /// <summary>
    /// Refresh token cache key formats. Must stay in sync with
    /// NovaCore.Auth.Infrastructure.Caching.RefreshTokenCacheService, which owns the write side of these keys.
    /// </summary>
    public static class RefreshTokens
    {
        private const string TokenKeyPrefix = "refresh_token:";
        private const string UserTokensKeyPrefix = "user_refresh_tokens:";

        /// <summary>Full cached token entry, keyed by the raw refresh token string. Pattern: refresh_token:{token}</summary>
        public static string ByTokenString(string token) => $"{TokenKeyPrefix}{token}";

        /// <summary>Lightweight per-user index (Redis hash) of tokens pending sync. Pattern: user_refresh_tokens:{userId}</summary>
        public static string UserTokens(Guid userId) => $"{UserTokensKeyPrefix}{userId}";

        /// <summary>Redis set of user IDs that currently own at least one active refresh token</summary>
        public const string ActiveUsers = "active_users";
    }

    /// <summary>Product cache patterns and configuration (for future extension)</summary>
    public static class Products
    {
        private const string Prefix = "product:products";

        /// <summary>Get product cache key. Pattern: product:products:detail:{productId}</summary>
        public static string Detail(Guid productId) => $"{Prefix}:detail:{productId}";

        /// <summary>Cache key pattern for all product details. Used for pattern-based invalidation</summary>
        public const string DetailPattern = "product:products:detail:*";

        /// <summary>Default TTL for product cache in minutes</summary>
        public const int DefaultTtlMinutes = 120;
    }

    /// <summary>Product category cache patterns and configuration (for future extension)</summary>
    public static class Categories
    {
        private const string Prefix = "product:categories";

        /// <summary>Get category cache key. Pattern: product:categories:detail:{categoryId}</summary>
        public static string Detail(Guid categoryId) => $"{Prefix}:detail:{categoryId}";

        /// <summary>Cache key pattern for all category details. Used for pattern-based invalidation</summary>
        public const string DetailPattern = "product:categories:detail:*";

        /// <summary>Default TTL for category cache in minutes</summary>
        public const int DefaultTtlMinutes = 240;
    }

    /// <summary>
    /// Shopping cart storage, keyed per user. Order Service owns both the read and write side -
    /// this is state, not a read-through cache of some other store, so there's no invalidation
    /// pattern here (TTL slides forward on every mutation instead).
    /// </summary>
    public static class Cart
    {
        private const string Prefix = "order:cart";

        /// <summary>Get the cart key for a user. Pattern: order:cart:{userId}</summary>
        public static string Key(Guid userId) => $"{Prefix}:{userId}";

        /// <summary>Default TTL for an idle cart in minutes (30 days)</summary>
        public const int DefaultTtlMinutes = 43200;
    }

    /// <summary>
    /// Notification channel runtime-config cache patterns. Keyed by channel type name (string,
    /// not the enum itself - SharedKernel can't depend on NovaCore.Notification.Domain).
    /// </summary>
    public static class NotificationChannels
    {
        private const string Prefix = "notification:channels";

        /// <summary>Get channel cache key. Pattern: notification:channels:type:{channelType}</summary>
        public static string ByType(string channelType) => $"{Prefix}:type:{channelType}";

        /// <summary>Cache key pattern for all channel entries. Used for pattern-based invalidation</summary>
        public const string AllPattern = "notification:channels:type:*";

        /// <summary>Default TTL for channel cache in minutes - short, since a manual admin change should take effect quickly</summary>
        public const int DefaultTtlMinutes = 5;
    }
}
