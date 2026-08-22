using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Application.Extensions;
using NovaCore.BuildingBlock.Persistence.Audit;
using NovaCore.BuildingBlock.Persistence.Ef.DependencyInjection;
using NovaCore.BuildingBlock.Persistence.Ef.Inbox;
using NovaCore.BuildingBlock.Persistence.Ef.Outbox;
using NovaCore.BuildingBlock.Persistence.Repository;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using OpenTelemetry.Trace;

using NovaCore.Content.Application.Abstractions.Persistence.ContentTypes;
using NovaCore.Content.Application.Abstractions.Persistence.Contents;
using NovaCore.Content.Application.Abstractions.Persistence.Taxonomies;
using NovaCore.Content.Application.Abstractions.Persistence.Workflows;

using NovaCore.Content.Persistence.Contexts.ContentTaxonomies.Read;
using NovaCore.Content.Persistence.Contexts.ContentTaxonomies.Write;
using NovaCore.Content.Persistence.Contexts.ContentTypes.Read;
using NovaCore.Content.Persistence.Contexts.ContentTypes.Write;
using NovaCore.Content.Persistence.Contexts.ContentWorkflowDefinitions.Read;
using NovaCore.Content.Persistence.Contexts.ContentWorkflowDefinitions.Write;
using NovaCore.Content.Persistence.Contexts.Contents.Read;
using NovaCore.Content.Persistence.Contexts.Contents.Write;
using NovaCore.Content.Persistence.Engine;
using NovaCore.Content.Persistence.Engine.UnitOfWork;
using NovaCore.Content.Persistence.Reliability.Inbox;
using NovaCore.Content.Persistence.Reliability.Outbox;

namespace NovaCore.Content.Persistence;

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
            .AddRepositories()
            .AddUnitOfWork()
            .AddOutbox()
            .AddInbox()
            .AddAuditHierarchy();

        return services;
    }

    // Only the 4 aggregate roots and their IAuditable owned children are registered - pure
    // mapping entities (ContentTaxonomyAssignment) aren't IAuditable and stay unregistered,
    // same reasoning Product/Chat.Persistence document for their own mapping entities.
    private static IServiceCollection AddAuditHierarchy(this IServiceCollection services)
    {
        services.ConfigureAuditHierarchy(builder =>
        {
            builder.Entity<ContentEntity>().IsRoot(x => x.Id);
            builder.Entity<ContentVersion>().BelongsTo<ContentEntity>(x => x.ContentId);
            builder.Entity<ContentPublication>().BelongsTo<ContentEntity>(x => x.ContentId);
            builder.Entity<ContentWorkflowInstance>().BelongsTo<ContentEntity>(x => x.ContentId);
            builder.Entity<ContentRelationship>().BelongsTo<ContentEntity>(x => x.SourceContentId);
            builder.Entity<ContentLocalization>().BelongsTo<ContentEntity>(x => x.ContentId);
            builder.Entity<ContentAudience>().BelongsTo<ContentEntity>(x => x.ContentId);
            builder.Entity<ContentContributor>().BelongsTo<ContentEntity>(x => x.ContentId);

            builder.Entity<ContentType>().IsRoot(x => x.Id);
            builder.Entity<ContentFieldDefinition>().BelongsTo<ContentType>(x => x.ContentTypeId);

            builder.Entity<ContentWorkflowDefinition>().IsRoot(x => x.Id);
            builder.Entity<ContentWorkflowState>().BelongsTo<ContentWorkflowDefinition>(x => x.WorkflowDefinitionId);
            builder.Entity<ContentWorkflowTransition>().BelongsTo<ContentWorkflowDefinition>(x => x.WorkflowDefinitionId);

            builder.Entity<ContentTaxonomy>().IsRoot(x => x.Id);
        });

        return services;
    }

    private static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddPersistenceDbContext<ContentDbContext>(connectionString);

        return services;
    }

    // Every <Aggregate>Repo implements the generic IRepository<T>/IRepository<T,TId> - Scrutor's
    // AsImplementedInterfaces() registers each concrete class against every interface it
    // implements (including the empty per-aggregate marker interfaces), so this one scan call
    // covers all 4 aggregate roots. Read/Write services are registered explicitly since they're
    // one-per-aggregate.
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScopedByInterface(typeof(IRepository<>), typeof(ContentDbContext));

        services.AddScoped<IContentReadService, ContentReadService>();
        services.AddScoped<IContentWriteService, ContentWriteService>();

        services.AddScoped<IContentTypeReadService, ContentTypeReadService>();
        services.AddScoped<IContentTypeWriteService, ContentTypeWriteService>();

        services.AddScoped<IContentWorkflowDefinitionReadService, ContentWorkflowDefinitionReadService>();
        services.AddScoped<IContentWorkflowDefinitionWriteService, ContentWorkflowDefinitionWriteService>();

        services.AddScoped<IContentTaxonomyReadService, ContentTaxonomyReadService>();
        services.AddScoped<IContentTaxonomyWriteService, ContentTaxonomyWriteService>();

        return services;
    }

    private static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }

    private static IServiceCollection AddOutbox(this IServiceCollection services)
    {
        services.AddEfOutboxStore<ContentDbContext>();
        services.AddScoped<IOutboxStore, OutboxStore>();

        return services;
    }

    private static IServiceCollection AddInbox(this IServiceCollection services)
    {
        services.AddEfInboxStore<ContentDbContext>();
        services.AddEfDeadLetterQueryService<ContentDbContext>();
        services.AddScoped<IInboxStore, InboxStore>();

        return services;
    }
}
