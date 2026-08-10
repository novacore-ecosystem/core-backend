using NovaCore.BuildingBlock.Application;
using FluentValidation;

using NovaCore.Inventory.Application.Abstractions.Persistence;
using NovaCore.Inventory.Application.Abstractions.Services;
using NovaCore.Inventory.Application.Services;

using Mapster;

using MapsterMapper;

using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.Inventory.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services
            .AddMediatR()
            .AddApplicationBehaviors()
            .AddMapster()
            .AddFluentValidation()
            .AddInfrastructure()
            .AddBusinessServices();

        return services;
    }

    private static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<OptimisticConcurrencyRetry>();
        return services;
    }

    private static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<IInventoryAdjustmentService, InventoryAdjustmentService>();
        services.AddScoped<IReceivingService, ReceivingService>();
        services.AddScoped<ITransferService, TransferService>();
        services.AddScoped<ICycleCountService, CycleCountService>();
        services.AddScoped<IInventoryDocumentService, InventoryDocumentService>();
        services.AddScoped<IStockDeductionService, StockDeductionService>();
        services.AddScoped<IInventoryTransactionService, InventoryTransactionService>();
        services.AddScoped<IStockAvailabilityService, StockAvailabilityService>();

        return services;
    }

    private static IServiceCollection AddMediatR(this IServiceCollection services)
    {
        services.AddMediatR(config =>
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }

    private static IServiceCollection AddMapster(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(typeof(DependencyInjection).Assembly);

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }

    private static IServiceCollection AddFluentValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
