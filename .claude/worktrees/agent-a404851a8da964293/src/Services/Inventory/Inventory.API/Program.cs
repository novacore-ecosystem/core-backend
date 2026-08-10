using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

using NovaCore.BuildingBlock.Infrastructure.Observability;
using NovaCore.BuildingBlock.Messaging.Kafka.Tracing;
using NovaCore.BuildingBlock.Observability.Logging;
using NovaCore.BuildingBlock.Observability.Tracing;

using Serilog;

using NovaCore.Inventory.API;
using NovaCore.Inventory.Application;
using NovaCore.Inventory.Infrastructure;
using NovaCore.Inventory.Persistence;
using NovaCore.Inventory.Persistence.Engine;
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) => config.ConfigureAppLogging(context.Configuration, "inventory-api"));

builder.WebHost.ConfigureKestrel(options =>
{
    var httpPort = int.Parse(builder.Configuration["ASPNETCORE_HTTP_PORT"] ?? "8080");
    options.ListenAnyIP(httpPort, listen =>
    {
        listen.Protocols = HttpProtocols.Http1;
    });

    var grpcPort = int.Parse(builder.Configuration["ASPNETCORE_GRPC_PORT"] ?? "5002");
    options.ListenAnyIP(grpcPort, listen =>
    {
        listen.Protocols = HttpProtocols.Http2;
    });
});

builder.Services
    .AddPersistence(builder.Configuration)
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPresentation(builder.Configuration)
    .AddOpenTelemetryObservability(builder.Configuration, "inventory-api", tracing => tracing
        .AddPersistenceTracing()
        .AddKafkaMessagingTracing()
        .AddInfrastructureTracing());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseRedisTracing();
app.UseApplication();

app.Run();
