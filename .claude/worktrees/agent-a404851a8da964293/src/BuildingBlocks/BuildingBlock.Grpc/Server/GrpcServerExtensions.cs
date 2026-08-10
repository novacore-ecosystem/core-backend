using NovaCore.BuildingBlock.Grpc.Interceptors;

using Grpc.AspNetCore.Server;
using Grpc.Health.V1;
using Grpc.HealthCheck;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.BuildingBlock.Grpc.Server;

/// <summary>
/// Extension methods for registering gRPC server services.
/// </summary>
public static class GrpcServerExtensions
{
    /// <summary>
    /// Add gRPC services with standard interceptors and health checks.
    /// </summary>
    public static IServiceCollection AddGrpcServer(this IServiceCollection services)
    {
        services.AddGrpc(options =>
        {
            options.Interceptors.Add<LoggingInterceptor>();
            options.Interceptors.Add<ErrorHandlingInterceptor>();
            options.MaxReceiveMessageSize = 10 * 1024 * 1024; // 10 MB
            options.MaxSendMessageSize = 10 * 1024 * 1024;
        });

        // Health check service
        services.AddSingleton<HealthServiceImpl>();

        return services;
    }

    /// <summary>
    /// Configure gRPC server with custom options.
    /// </summary>
    public static IServiceCollection AddGrpcServer(
        this IServiceCollection services,
        Action<GrpcServiceOptions> configureOptions)
    {
        services.AddGrpc(options =>
        {
            options.Interceptors.Add<LoggingInterceptor>();
            options.Interceptors.Add<ErrorHandlingInterceptor>();
            configureOptions(options);
        });

        services.AddSingleton<HealthServiceImpl>();
        return services;
    }

    /// <summary>
    /// Map gRPC services and health check endpoint.
    /// Call this in Program.cs after app.UseRouting().
    /// </summary>
    public static WebApplication MapGrpcServices(this WebApplication app)
    {
        // Health check service for load balancers and monitoring
        var healthService = app.Services.GetRequiredService<HealthServiceImpl>();
        healthService.SetStatus("", HealthCheckResponse.Types.ServingStatus.Serving);

        return app;
    }

}
