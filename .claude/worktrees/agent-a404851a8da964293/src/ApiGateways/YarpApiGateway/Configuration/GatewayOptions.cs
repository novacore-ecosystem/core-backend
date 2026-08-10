namespace NovaCore.YarpApiGateway.Configuration;

public sealed class GatewayOptions
{
    public required Dictionary<string, ServiceConfig> Services { get; init; }
    public required JwtOptions Jwt { get; init; }
    public required RedisOptions Redis { get; init; }
}

public sealed class ServiceConfig
{
    public required string Url { get; init; }
    public required string Name { get; init; }
    public required string Path { get; init; }
    public string? SwaggerUrl { get; init; }
    public bool RequireAuth { get; init; } = true;
}

public sealed class JwtOptions
{
    public required string SecretKey { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
}

public sealed class RedisOptions
{
    public required string ConnectionString { get; init; }
}
