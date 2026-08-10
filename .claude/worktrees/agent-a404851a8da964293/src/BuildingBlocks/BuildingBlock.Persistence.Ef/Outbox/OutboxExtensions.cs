using NovaCore.BuildingBlock.Persistence.Outbox;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.BuildingBlock.Persistence.Ef.Outbox;

public static class OutboxExtensions
{
    /// <summary>
    /// Apply Outbox table configuration to the model builder.
    /// Call from your DbContext's OnModelCreating after ApplyConfigurationsFromAssembly.
    /// </summary>
    public static void ApplyOutboxConfiguration(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxConfiguration());
    }

    /// <summary>
    /// Register the generic EF outbox store for the given DbContext type.
    /// The DbContext must implement IOutboxDbContext.
    /// </summary>
    public static IServiceCollection AddEfOutboxStore<TContext>(this IServiceCollection services)
        where TContext : Microsoft.EntityFrameworkCore.DbContext, IOutboxDbContext
    {
        services.AddScoped<IOutboxStore, EfOutboxStore<TContext>>();
        return services;
    }
}
