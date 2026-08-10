using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using Yarp.ReverseProxy.Configuration;

using NovaCore.YarpApiGateway.Caching;
using NovaCore.YarpApiGateway.Configuration;
using NovaCore.YarpApiGateway.Services;

namespace NovaCore.YarpApiGateway;

public static class DependencyInjection
{
    public static IServiceCollection AddGatewayServices(this IServiceCollection services, IConfiguration configuration)
    {
        var gatewayOptions = configuration.GetSection("Gateway").Get<GatewayOptions>()
            ?? throw new InvalidOperationException("Gateway configuration is missing");

        services.AddSingleton(gatewayOptions);
        services.AddSingleton<ISwaggerAggregator, SwaggerAggregator>();
        services.AddHttpClient();
        services.AddHealthChecks();
        services.AddRefreshTokenCache(gatewayOptions.Redis.ConnectionString);

        AddAuthentication(services, gatewayOptions);
        AddReverseProxy(services, gatewayOptions);

        return services;
    }

    /// <summary>
    /// Basic JWT integrity validation only: signature, expiry, issuer/audience, and token format.
    /// No role/permission resolution here - every downstream service performs its own complete
    /// authorization; the gateway's job is only to reject obviously invalid or forged tokens early.
    /// </summary>
    private static void AddAuthentication(IServiceCollection services, GatewayOptions gatewayOptions)
    {
        var jwtSettings = gatewayOptions.Jwt;
        var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

        services.AddAuthentication(authOptions =>
        {
            authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(jwtOptions =>
        {
            jwtOptions.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            };

            jwtOptions.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (context.Request.Cookies.TryGetValue("AccessToken", out var token))
                        context.Token = token;

                    return Task.CompletedTask;
                }
            };
        });
    }

    private static void AddReverseProxy(IServiceCollection services, GatewayOptions options)
    {
        services.AddReverseProxy().LoadFromMemory(
            BuildRoutes(options),
            BuildClusters(options)
        );
    }

    private const string HubPathSegment = "/hubs";

    private static List<RouteConfig> BuildRoutes(GatewayOptions options)
    {
        var routes = new List<RouteConfig>();

        foreach (var service in options.Services)
        {
            routes.Add(new RouteConfig
            {
                RouteId = service.Key,
                ClusterId = service.Key,
                Match = new RouteMatch
                {
                    Path = $"{service.Value.Path}{{**catch-all}}"
                },
                // SignalR hubs must receive the untouched request path (e.g. /hubs/global) since the
                // downstream negotiate/connect endpoints are mapped at that exact path; stripping the
                // prefix like a normal REST route would 404 the handshake.
                Transforms = IsSignalRHub(service.Value.Path)
                    ? null
                    : [
                        new Dictionary<string, string>
                        {
                            { "PathPattern", "{**catch-all}" }
                        }
                    ]
            });
        }

        return routes;
    }

    private static bool IsSignalRHub(string path) =>
        path.Contains(HubPathSegment, StringComparison.OrdinalIgnoreCase);

    private static List<ClusterConfig> BuildClusters(GatewayOptions options)
    {
        var clusters = new List<ClusterConfig>();

        foreach (var service in options.Services)
        {
            clusters.Add(new ClusterConfig
            {
                ClusterId = service.Key,
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    [service.Key] = new DestinationConfig
                    {
                        Address = service.Value.Url
                    }
                }
            });
        }

        return clusters;
    }
}
