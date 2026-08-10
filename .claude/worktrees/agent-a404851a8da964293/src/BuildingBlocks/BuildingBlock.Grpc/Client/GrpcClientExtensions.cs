using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.BuildingBlock.Grpc.Client;

public static class GrpcClientExtensions
{
    private const int MaxMessageSize = 10 * 1024 * 1024;

    public static IHttpClientBuilder AddGrpcClient<TClient>(
        this IServiceCollection services,
        Uri address)
        where TClient : class
    {
        return services
            .AddGrpcClient<TClient>(options => options.Address = address)
            .ConfigureChannel(ConfigureDefaultChannel);
    }

    public static IHttpClientBuilder AddGrpcClient<TClient>(
        this IServiceCollection services,
        Uri address,
        Action<GrpcChannelOptions> configure)
        where TClient : class
    {
        return services
            .AddGrpcClient<TClient>(options => options.Address = address)
            .ConfigureChannel(options =>
            {
                ConfigureDefaultChannel(options);
                configure(options);
            });
    }

    public static IServiceCollection AddGrpcClientMesh(
        this IServiceCollection services,
        Dictionary<string, Uri> serviceAddresses)
    {
        ArgumentNullException.ThrowIfNull(serviceAddresses);

        foreach (var (serviceName, address) in serviceAddresses)
        {
            services.Configure<GrpcClientOptions>(serviceName, options =>
            {
                options.ServiceName = serviceName;
                options.Address = address;
            });
        }

        return services;
    }

    private static void ConfigureDefaultChannel(GrpcChannelOptions options)
    {
        options.HttpHandler = new SocketsHttpHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip
        };
        options.MaxSendMessageSize = MaxMessageSize;
        options.MaxReceiveMessageSize = MaxMessageSize;
    }
}

/// <summary>
/// Configuration options for gRPC clients.
/// </summary>
public class GrpcClientOptions
{
    public string? ServiceName { get; set; }
    public Uri? Address { get; set; }
    public int? MaxRetries { get; set; } = 3;
    public TimeSpan? Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
