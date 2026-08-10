using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Application.Abstractions.Persistence.RefreshTokens;
using NovaCore.Auth.Application.Abstractions.Persistence.Scopes;
using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;
using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Domain.Entities.Invitations;
using NovaCore.Auth.Domain.Entities.Permissions;
using NovaCore.Auth.Domain.Entities.Positions;
using NovaCore.Auth.Domain.Entities.Roles;
using NovaCore.Auth.Domain.Entities.Scopes;
using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.Auth.Persistence.Contexts.Accounts.Read;
using NovaCore.Auth.Persistence.Contexts.Accounts.Repositories;
using NovaCore.Auth.Persistence.Contexts.Accounts.Write;
using NovaCore.Auth.Persistence.Contexts.RefreshTokens.Read;
using NovaCore.Auth.Persistence.Contexts.RefreshTokens.Write;
using NovaCore.Auth.Persistence.Contexts.Scopes.Read;
using NovaCore.Auth.Persistence.Contexts.Scopes.Write;
using NovaCore.Auth.Persistence.Contexts.Tenants.Read;
using NovaCore.Auth.Persistence.Contexts.Tenants.Write;
using NovaCore.Auth.Persistence.Engine;
using NovaCore.Auth.Persistence.Engine.UnitOfWork;
using NovaCore.Auth.Persistence.Reliability.Inbox;
using NovaCore.Auth.Persistence.Reliability.Outbox;
using NovaCore.Auth.Persistence.Storage.Seeders;
using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Extensions;
using NovaCore.BuildingBlock.Persistence.Audit;
using NovaCore.BuildingBlock.Persistence.Ef.DependencyInjection;
using NovaCore.BuildingBlock.Persistence.Ef.Inbox;
using NovaCore.BuildingBlock.Persistence.Ef.Outbox;
using NovaCore.BuildingBlock.Persistence.Repository;

using Npgsql;

using OpenTelemetry.Trace;

namespace NovaCore.Auth.Persistence;

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
            .AddIdentityService()
            .AddApplicationServices()
            .AddRepositories()
            .AddUnitOfWork()
            .AddOutboxAndInbox()
            .AddSeeding()
            .AddAuditHierarchy();

        return services;
    }

    // Account, Role, Position, PermissionGroup, PermissionDefinition, Invitation, Tenant and
    // Scope are independent aggregates. AccountPosition (personnel/position grant history),
    // ExternalIdentity (OAuth link configuration) and MfaMethod (MFA configuration) are all
    // genuine business-critical children an administrator would expect change history for, so
    // they're registered via BelongsTo despite being owned by Account rather than roots
    // themselves. Every *Translation entity is registered the same way - admin-facing display
    // copy is real content, not structural scaffolding. TenantLocale (bootstrap resource
    // content) is registered the same way for the same reason. AccountRole/RolePermission/
    // PositionRole (pure mapping, no content beyond the relationship) and RefreshToken/Session/
    // LoginHistory/PasswordHistory/MfaBackupCode/Device/AccountPermission (generated artifacts,
    // high-churn tracking records, or a denormalized cache) are intentionally not IAuditable and
    // stay unregistered.
    private static IServiceCollection AddAuditHierarchy(this IServiceCollection services)
    {
        services.ConfigureAuditHierarchy(builder =>
        {
            builder.Entity<Account>().IsRoot(x => x.Id);
            builder.Entity<AccountPosition>()
                .BelongsTo<Account>(x => x.AccountId);
            builder.Entity<ExternalIdentity>()
                .BelongsTo<Account>(x => x.AccountId);
            builder.Entity<MfaMethod>()
                .BelongsTo<Account>(x => x.AccountId);

            builder.Entity<Role>().IsRoot(x => x.Id);
            builder.Entity<RoleTranslation>()
                .BelongsTo<Role>(x => x.Id);

            builder.Entity<Position>().IsRoot(x => x.Id);
            builder.Entity<PositionTranslation>()
                .BelongsTo<Position>(x => x.Id);

            builder.Entity<PermissionGroup>().IsRoot(x => x.Id);
            builder.Entity<PermissionGroupTranslation>()
                .BelongsTo<PermissionGroup>(x => x.Id);

            builder.Entity<PermissionDefinition>().IsRoot(x => x.Id);
            builder.Entity<PermissionDefinitionTranslation>()
                .BelongsTo<PermissionDefinition>(x => x.Id);

            builder.Entity<Invitation>().IsRoot(x => x.Id);

            builder.Entity<Tenant>().IsRoot(x => x.Id);
            builder.Entity<TenantLocale>()
                .BelongsTo<Tenant>(x => x.TenantId);

            builder.Entity<Scope>().IsRoot(x => x.Id);
            builder.Entity<ScopeTranslation>()
                .BelongsTo<Scope>(x => x.Id);
        });

        return services;
    }

    private static IServiceCollection AddDatabaseContext(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentException("ConnectionString was not configured.");

        services.AddPersistenceDbContext<AuthDbContext>(connectionString);

        return services;
    }

    private static IServiceCollection AddIdentityService(this IServiceCollection services)
    {
        services.AddIdentityCore<Account>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddRoles<Role>()
        .AddEntityFrameworkStores<AuthDbContext>();

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScopedByInterface<IAppService>(typeof(AuthDbContext));
        return services;
    }

    // RefreshTokenRepo implements the generic IRepository<T> (restored - it genuinely needs the
    // tracked-load-and-mutate UpdateAsync), so it's Scrutor-scanned; AccountRepo doesn't (its only
    // real operation, DeleteIfExistAsync, is a bulk ExecuteDeleteAsync that doesn't fit the
    // generic shape - Account mutation itself goes through ASP.NET Identity's UserManager, not
    // this repository at all), so it stays manually registered.
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScopedByInterface(typeof(IRepository<>), typeof(AuthDbContext));

        services.AddScoped<IAccountRepository, AccountRepo>();
        services.AddScoped<IAccountReadService, AccountReadService>();
        services.AddScoped<IAccountWriteService, AccountWriteService>();

        services.AddScoped<IRefreshTokenReadService, RefreshTokenReadService>();
        services.AddScoped<IRefreshTokenWriteService, RefreshTokenWriteService>();

        // TenantRepo/ScopeRepo implement the generic IRepository<T> in full (via
        // AuthBaseRepository), so - like RefreshTokenRepo - they're Scrutor-scanned; only their
        // one-per-aggregate Read/Write services are registered explicitly.
        services.AddScoped<ITenantReadService, TenantReadService>();
        services.AddScoped<ITenantWriteService, TenantWriteService>();

        services.AddScoped<IScopeReadService, ScopeReadService>();
        services.AddScoped<IScopeWriteService, ScopeWriteService>();

        return services;
    }

    private static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, AuthUnitOfWork>();
        return services;
    }

    private static IServiceCollection AddSeeding(this IServiceCollection services)
    {
        services.AddScoped<DatabaseSeeder>();
        return services;
    }

    // Manually registered (not Scrutor-scanned) because the adapter needs a literal
    // serviceName - must match the string passed to AddKafkaMessaging so relayed
    // messages land on the topic other services actually listen on.
    private static IServiceCollection AddOutboxAndInbox(this IServiceCollection services)
    {
        services
            .AddEfOutboxStore<AuthDbContext>()
            .AddEfInboxStore<AuthDbContext>()
            .AddEfDeadLetterQueryService<AuthDbContext>();

        services.AddScoped<IOutboxStore>(sp => new OutboxStore(
            sp.GetRequiredService<NovaCore.BuildingBlock.Persistence.Outbox.IOutboxStore>(),
            "auth-service"));
        services.AddScoped<IInboxStore>(sp => new InboxStore(
            sp.GetRequiredService<NovaCore.BuildingBlock.Persistence.Inbox.IInboxStore>()));

        return services;
    }
}
