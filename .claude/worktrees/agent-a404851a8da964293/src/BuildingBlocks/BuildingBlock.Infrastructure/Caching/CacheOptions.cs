namespace NovaCore.BuildingBlock.Infrastructure.Caching;

/// <summary>
/// Cache options
/// </summary>
public sealed class CacheOptions
{
    /// <summary>Configuration cache key</summary>
    public const string Section = "Cache";

    /// <summary>Redis connection string (e.g., "loalhost:6379")</summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>Default expiration time in minutes (default: 60)</summary>
    public int DefaultExpirationMinutes { get; set; } = 60;

    /// <summary>Enable compression for large values (default: false)</summary>
    public bool EnableCompression { get; set; } = false;

    /// <summary>Key prefix for namespacing (e.g., "auth:", "order:")</summary>
    public string KeyPrefix { get; set; } = string.Empty;

    /// <summary>Connection timeout in milliseconds (default: 5000)</summary>
    public int ConnectionTimeout { get; set; } = 5000;
}
