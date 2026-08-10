using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using OpenTelemetry.Trace;

using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Extensions;
using NovaCore.BuildingBlock.Persistence.Audit;
using NovaCore.BuildingBlock.Persistence.Ef.DependencyInjection;
using NovaCore.BuildingBlock.Persistence.Ef.Inbox;
using NovaCore.BuildingBlock.Persistence.Ef.Outbox;
using NovaCore.BuildingBlock.Persistence.Repository;
using NovaCore.BuildingBlock.Search.DependencyInjection;
using NovaCore.User.Application.Abstractions.Persistence.Users;
using NovaCore.User.Application.Abstractions.Search;
using NovaCore.User.Persistence.Contexts.Users.Read;
using NovaCore.User.Persistence.Contexts.Users.Search.Indexers;
using NovaCore.User.Persistence.Contexts.Users.Search.Repositories;
using NovaCore.User.Persistence.Contexts.Users.Write;
using NovaCore.User.Persistence.Engine;
using NovaCore.User.Persistence.Engine.UnitOfWork;
using NovaCore.User.Persistence.Reliability.Inbox;
using NovaCore.User.Persistence.Reliability.Outbox;

namespace NovaCore.User.Persistence;

public static class DependencyInjection
{
    public static TracerProviderBuilder AddPersistenceTracing(this TracerProviderBuilder builder)
    {
        return builder.AddNpgsql();
    }

    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddDatabaseContext(configuration)
            .AddApplicationServices()
            .AddUserPersistence()
            .AddUnitOfWork()
            .AddOutboxAndInbox()
            .AddAuditHierarchy()
            .AddUserSearchServices(configuration);

        return services;
    }

    // Mirrors NovaCore.Product.Persistence's AddProductSearchServices - a business-capability-named
    // method (not AddElasticsearchPersistence) called from AddPersistence, never a separate
    // Program.cs step. See docs/reference/search.md.
    private static IServiceCollection AddUserSearchServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddElasticsearchClient(configuration);
        services.AddScoped<IUserSearchIndexer, UserSearchIndexer>();
        services.AddScoped<IUserSearchRepository, UserSearchRepository>();

        return services;
    }

    private static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddPersistenceDbContext<UserDbContext>(connectionString);

        return services;
    }

    // User, UserRole and UserTag are the independent aggregates from the identity-model redesign -
    // UserProfile/UserAvatar/UserSetting/UserSecuritySetting/UserPrivacySetting/
    // UserNotificationSetting/UserPreference/UserActivitySummary/UserAddress/UserContact/
    // UserPaymentMethod/UserVerification/UserRoleAssignment are all owned children with real
    // business content (contact/security/privacy settings, KYC verification, role-grant history),
    // so each is registered via BelongsTo. UserRoleTranslation/UserTagTranslation are registered
    // the same way - admin-facing display copy is real content. UserTagMapping (pure mapping) and
    // UserPermissionSnapshot (denormalized permission cache) are intentionally not IAuditable and
    // stay unregistered.
    private static IServiceCollection AddAuditHierarchy(this IServiceCollection services)
    {
        services.ConfigureAuditHierarchy(builder =>
        {
            builder.Entity<UserEntity>().IsRoot(x => x.Id);
            builder.Entity<UserProfile>()
                .BelongsTo<UserEntity>(x => x.UserId);
            builder.Entity<UserAvatar>()
                .BelongsTo<UserEntity>(x => x.UserId);
            builder.Entity<UserSetting>()
                .BelongsTo<UserEntity>(x => x.UserId);
            builder.Entity<UserSecuritySetting>()
                .BelongsTo<UserEntity>(x => x.UserId);
            builder.Entity<UserPrivacySetting>()
                .BelongsTo<UserEntity>(x => x.UserId);
            builder.Entity<UserNotificationSetting>()
                .BelongsTo<UserEntity>(x => x.UserId);
            builder.Entity<UserPreference>()
                .BelongsTo<UserEntity>(x => x.UserId);
            builder.Entity<UserActivitySummary>()
                .BelongsTo<UserEntity>(x => x.UserId);
            builder.Entity<UserAddress>()
                .BelongsTo<UserEntity>(x => x.UserId);
            builder.Entity<UserContact>()
                .BelongsTo<UserEntity>(x => x.UserId);
            builder.Entity<UserPaymentMethod>()
                .BelongsTo<UserEntity>(x => x.UserId);
            builder.Entity<UserVerification>()
                .BelongsTo<UserEntity>(x => x.UserId);
            builder.Entity<UserRoleAssignment>()
                .BelongsTo<UserEntity>(x => x.UserId);

            builder.Entity<UserRole>().IsRoot(x => x.Id);
            builder.Entity<UserRoleTranslation>()
                .BelongsTo<UserRole>(x => x.Id);

            builder.Entity<UserTag>().IsRoot(x => x.Id);
            builder.Entity<UserTagTranslation>()
                .BelongsTo<UserTag>(x => x.Id);
        });

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScopedByInterfaceAndConcrete<IAppService>(typeof(UserDbContext));
        return services;
    }

    // UserRepo implements the generic IRepository<T> - AsImplementedInterfaces() registers it
    // against both IRepository<User> and the specific IUserRepository in one scan call.
    private static IServiceCollection AddUserPersistence(this IServiceCollection services)
    {
        services.AddScopedByInterface(typeof(IRepository<>), typeof(UserDbContext));

        services.AddScoped<IUserReadService, UserReadService>();
        services.AddScoped<IUserWriteService, UserWriteService>();
        return services;
    }

    private static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }

    private static IServiceCollection AddOutboxAndInbox(this IServiceCollection services)
    {
        services
            .AddEfOutboxStore<UserDbContext>()
            .AddEfInboxStore<UserDbContext>()
            .AddEfDeadLetterQueryService<UserDbContext>();

        services.AddScoped<IOutboxStore, OutboxStore>();
        services.AddScoped<IInboxStore, InboxStore>();

        return services;
    }
}
