using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.BuildingBlock.Persistence.Ef.DbContext;

public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Provider-agnostic EF options shared by every service DbContext: naming convention, error
    /// detail level, and any registered SaveChanges interceptors. Provider selection (UseNpgsql,
    /// connection string, retry policy) stays in AddPersistenceDbContext - that part is
    /// Npgsql-specific, this part isn't.
    /// </summary>
    public static DbContextOptionsBuilder UsePersistenceDefaults(
        this DbContextOptionsBuilder builder,
        IServiceProvider serviceProvider)
    {
        builder.UseSnakeCaseNamingConvention();

        builder.EnableDetailedErrors();

#if DEBUG
        builder.EnableSensitiveDataLogging();
#endif

        var interceptors = serviceProvider.GetServices<ISaveChangesInterceptor>();

        if (interceptors.Any())
        {
            builder.AddInterceptors(interceptors);
        }

        return builder;
    }

    /// <summary>
    /// Suppresses EF's design-time "pending model changes" warning. Every service's migrations
    /// are already correct; this only silences a false positive some provider conventions
    /// (e.g. EFCore.NamingConventions) can trigger against the model snapshot.
    /// </summary>
    public static DbContextOptionsBuilder SuppressPendingModelChangesWarning(
        this DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));

        return optionsBuilder;
    }
}
