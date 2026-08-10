using NovaCore.BuildingBlock.Application.Abstractions.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.BuildingBlock.Infrastructure.Audit;

public static class AuditInfrastructureExtensions
{
    /// <summary>Registers the HTTP-context-aware <see cref="IAuditMetadataProvider"/> for <paramref name="serviceName"/>, overriding NovaCore.BuildingBlock.Persistence.Ef's default no-op provider.</summary>
    public static IServiceCollection AddHttpAuditMetadataProvider(this IServiceCollection services, string serviceName)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<IAuditMetadataProvider>(sp => new HttpAuditMetadataProvider(
            sp.GetRequiredService<ICurrentUserService>(),
            sp.GetRequiredService<IHttpContextAccessor>(),
            serviceName));

        return services;
    }
}
