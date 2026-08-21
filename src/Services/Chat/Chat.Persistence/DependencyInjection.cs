using NovaCore.BuildingBlock.Persistence.Ef.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.Chat.Persistence;

/// <summary>
/// Registers only ChatDbContext this pass - Repository/ReadService/WriteService/UnitOfWork/
/// Outbox/Inbox wiring depends on Application-owned interfaces (see
/// docs/conventions/persistence-coding-conventions.md), and Chat.Application is an empty shell
/// until the next phase.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddPersistenceDbContext<ChatDbContext>(connectionString);

        return services;
    }
}
