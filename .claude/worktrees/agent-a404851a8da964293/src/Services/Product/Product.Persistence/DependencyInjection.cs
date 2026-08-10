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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using OpenTelemetry.Trace;

using NovaCore.Product.Application.Abstractions.Persistence.ProductCategories;
using NovaCore.Product.Application.Abstractions.Persistence.Products;
using NovaCore.Product.Application.Abstractions.Persistence.ProductTags;
using NovaCore.Product.Application.Abstractions.Search;
using NovaCore.Product.Persistence.Contexts.ProductCategories.Read;
using NovaCore.Product.Persistence.Contexts.ProductCategories.Write;
using NovaCore.Product.Persistence.Contexts.Products.Read;
using NovaCore.Product.Persistence.Contexts.Products.Search.Indexers;
using NovaCore.Product.Persistence.Contexts.Products.Search.Repositories;
using NovaCore.Product.Persistence.Contexts.Products.Write;
using NovaCore.Product.Persistence.Contexts.ProductTags.Read;
using NovaCore.Product.Persistence.Contexts.ProductTags.Write;
using NovaCore.Product.Persistence.Engine;
using NovaCore.Product.Persistence.Engine.UnitOfWork;
using NovaCore.Product.Persistence.Reliability.Inbox;
using NovaCore.Product.Persistence.Reliability.Outbox;

namespace NovaCore.Product.Persistence;

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
            .AddRepositories()
            .AddUnitOfWork()
            .AddOutbox()
            .AddInbox()
            .AddAuditHierarchy()
            .AddProductSearchServices(configuration);

        return services;
    }

    // Product, ProductCategory, ProductTag, ProductBrand, ProductCollection, ProductOptionDefinition,
    // SpecificationGroup, SpecificationDefinition and WarrantyPolicy are all independent aggregates -
    // Product references their ids but doesn't own them, so each is its own root. Variant and every
    // Translation entity hold real business content (pricing/SKU, admin-facing display copy) an
    // administrator would expect to see change history for, so they're registered via BelongsTo
    // even though they're owned children, not roots. Pure mapping entities (ProductCategoryMapping/
    // ProductTagMapping/ProductCollectionMapping/ProductOption/ProductOptionValue/VariantOptionValue)
    // aren't IAuditable and stay unregistered - they carry no content of their own beyond the
    // relationship + display order.
    private static IServiceCollection AddAuditHierarchy(this IServiceCollection services)
    {
        services.ConfigureAuditHierarchy(builder =>
        {
            builder.Entity<ProductEntity>().IsRoot(x => x.Id);
            builder.Entity<ProductVariant>()
                .BelongsTo<ProductEntity>(x => x.ProductId);
            builder.Entity<ProductTranslation>()
                .BelongsTo<ProductEntity>(x => x.Id);

            builder.Entity<ProductCategory>().IsRoot(x => x.Id);
            builder.Entity<ProductCategoryTranslation>()
                .BelongsTo<ProductCategory>(x => x.Id);

            builder.Entity<ProductTag>().IsRoot(x => x.Id);
            builder.Entity<ProductTagTranslation>()
                .BelongsTo<ProductTag>(x => x.Id);

            builder.Entity<ProductBrand>().IsRoot(x => x.Id);
            builder.Entity<ProductBrandTranslation>()
                .BelongsTo<ProductBrand>(x => x.Id);

            builder.Entity<ProductCollection>().IsRoot(x => x.Id);
            builder.Entity<ProductCollectionTranslation>()
                .BelongsTo<ProductCollection>(x => x.Id);

            builder.Entity<ProductOptionDefinition>().IsRoot(x => x.Id);
            builder.Entity<ProductOptionDefinitionTranslation>()
                .BelongsTo<ProductOptionDefinition>(x => x.ProductOptionDefinitionId);
            builder.Entity<ProductOptionValueDefinition>()
                .BelongsTo<ProductOptionDefinition>(x => x.ProductOptionDefinitionId);
            builder.Entity<ProductOptionValueDefinitionTranslation>()
                .BelongsTo<ProductOptionValueDefinition>(x => x.ProductOptionValueDefinitionId);

            builder.Entity<SpecificationGroup>().IsRoot(x => x.Id);
            builder.Entity<SpecificationGroupTranslation>()
                .BelongsTo<SpecificationGroup>(x => x.SpecificationGroupId);

            builder.Entity<SpecificationDefinition>().IsRoot(x => x.Id);
            builder.Entity<SpecificationDefinitionTranslation>()
                .BelongsTo<SpecificationDefinition>(x => x.SpecificationDefinitionId);

            builder.Entity<WarrantyPolicy>().IsRoot(x => x.Id);
        });

        return services;
    }

    private static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddPersistenceDbContext<ProductDbContext>(connectionString);

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScopedByInterfaceAndConcrete<IAppService>(typeof(ProductDbContext));
        return services;
    }

    // ProductRepo/ProductCategoryRepo/ProductTagRepo all implement the generic IRepository<T> -
    // Scrutor's AsImplementedInterfaces() registers each concrete class against every interface
    // it implements (including the empty per-aggregate marker interfaces), so this one scan call
    // covers both. Read/Write services are registered explicitly since they're one-per-aggregate.
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScopedByInterface(typeof(IRepository<>), typeof(ProductDbContext));

        services.AddScoped<IProductReadService, ProductReadService>();
        services.AddScoped<IProductWriteService, ProductWriteService>();

        services.AddScoped<IProductCategoryReadService, ProductCategoryReadService>();
        services.AddScoped<IProductCategoryWriteService, ProductCategoryWriteService>();

        services.AddScoped<IProductTagReadService, ProductTagReadService>();
        services.AddScoped<IProductTagWriteService, ProductTagWriteService>();

        return services;
    }

    private static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }

    private static IServiceCollection AddOutbox(this IServiceCollection services)
    {
        services.AddEfOutboxStore<ProductDbContext>();
        services.AddScoped<IOutboxStore, OutboxStore>();

        return services;
    }

    // Product now self-consumes its own integration events for Search sync (see
    // docs/reference/search.md), so it needs an Inbox for dedup - previously it only published.
    private static IServiceCollection AddInbox(this IServiceCollection services)
    {
        services.AddEfInboxStore<ProductDbContext>();
        services.AddEfDeadLetterQueryService<ProductDbContext>();
        services.AddScoped<IInboxStore, InboxStore>();

        return services;
    }

    private static IServiceCollection AddProductSearchServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddElasticsearchClient(configuration);
        services.AddScoped<IProductSearchIndexer, ProductSearchIndexer>();
        services.AddScoped<IProductSearchRepository, ProductSearchRepository>();

        return services;
    }
}
