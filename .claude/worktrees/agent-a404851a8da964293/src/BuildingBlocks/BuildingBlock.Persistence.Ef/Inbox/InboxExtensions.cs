using NovaCore.BuildingBlock.Application.Abstractions.DeadLetters;
using NovaCore.BuildingBlock.Persistence.Inbox;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.BuildingBlock.Persistence.Ef.Inbox;

public static class InboxExtensions
{
    /// <summary>
    /// Apply Inbox table configuration to the model builder.
    /// Call from your DbContext's OnModelCreating after ApplyConfigurationsFromAssembly.
    /// </summary>
    public static void ApplyInboxConfiguration(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new InboxConfiguration());
        modelBuilder.ApplyConfiguration(new InboxRetryHistoryConfiguration());
    }

    /// <summary>
    /// Register the generic EF inbox store for the given DbContext type.
    /// The DbContext must implement IInboxDbContext.
    /// </summary>
    public static IServiceCollection AddEfInboxStore<TContext>(this IServiceCollection services)
        where TContext : Microsoft.EntityFrameworkCore.DbContext, IInboxDbContext
    {
        services.AddScoped<IInboxStore, EfInboxStore<TContext>>();
        return services;
    }

    /// <summary>
    /// Register the generic EF dead-letter query service for the given DbContext type.
    /// </summary>
    public static IServiceCollection AddEfDeadLetterQueryService<TContext>(this IServiceCollection services)
        where TContext : Microsoft.EntityFrameworkCore.DbContext, IInboxDbContext
    {
        services.AddScoped<IDeadLetterQueryService, EfDeadLetterQueryService<TContext>>();
        return services;
    }
}
