using Microsoft.AspNetCore.Server.Kestrel.Core;

using NovaCore.BuildingBlock.Infrastructure.Observability;
using NovaCore.BuildingBlock.Messaging.Kafka.Tracing;
using NovaCore.BuildingBlock.Observability.Logging;
using NovaCore.BuildingBlock.Observability.Tracing;
using NovaCore.BuildingBlock.Web.Extensions;

using Serilog;

using NovaCore.Inventory.API;
using NovaCore.Inventory.Application;
using NovaCore.Inventory.Infrastructure;
using NovaCore.Inventory.Persistence;
var builder = WebApplication.CreateBuilder(args);

await builder.Configuration.AddVaultSecretsAsync();

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

// Migrations and seeding are Inventory.DbMigrator's responsibility, run before this service starts.
app.UseRedisTracing();
app.UseApplication();

app.Run();
