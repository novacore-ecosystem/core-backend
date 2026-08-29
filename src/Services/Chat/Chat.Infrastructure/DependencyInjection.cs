using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NovaCore.Chat.Infrastructure.Configurations;
using NovaCore.Chat.Infrastructure.Security.Jwt;
using NovaCore.Chat.Infrastructure.SignalR;
using NovaCore.Chat.Infrastructure.SignalR.Facade;

namespace NovaCore.Chat.Infrastructure;

/// <summary>
/// Deliberately minimal for this phase - no Redis cache, background jobs, gRPC clients, or Kafka
/// messaging wiring (unlike Product/Order's AddInfrastructure), since nothing in Chat needs them
/// yet. AddSignalR() itself is registered by Chat.API (see Notification's own split - the Hub type
/// is Infrastructure's, but the ASP.NET Core SignalR server registration/mapping is Presentation's).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddChatConfigurations(configuration)
            .AddScoped<IGuestTokenGenerator, GuestTokenGenerator>();

        services.AddScoped<ConversationHubFacade>();
        services.AddScoped<IChatRealtimeNotifier, SignalRChatRealtimeNotifier>();

        return services;
    }
}
