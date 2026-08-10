using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

using NovaCore.BuildingBlock.Infrastructure.Observability;
using NovaCore.BuildingBlock.Messaging.Kafka.Tracing;
using NovaCore.BuildingBlock.Observability.Logging;
using NovaCore.BuildingBlock.Observability.Tracing;

using Serilog;

using NovaCore.Product.API;
using NovaCore.Product.Application;
using NovaCore.Product.Application.Abstractions.Search;
using NovaCore.Product.Infrastructure;
using NovaCore.Product.Persistence;
using NovaCore.Product.Persistence.Engine;
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) => config.ConfigureAppLogging(context.Configuration, "product-api"));

builder.WebHost.ConfigureKestrel(options =>
{
    var httpPort = int.Parse(builder.Configuration["ASPNETCORE_HTTP_PORT"] ?? "8080");
    options.ListenAnyIP(httpPort, listen =>
    {
        listen.Protocols = HttpProtocols.Http1;
    });
});

builder.Services
    .AddPersistence(builder.Configuration)
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPresentation(builder.Configuration)
    .AddOpenTelemetryObservability(builder.Configuration, "product-api", tracing => tracing
        .AddPersistenceTracing()
        .AddKafkaMessagingTracing()
        .AddInfrastructureTracing());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    await dbContext.Database.MigrateAsync();

    var searchIndexer = scope.ServiceProvider.GetRequiredService<IProductSearchIndexer>();
    try
    {
        await searchIndexer.EnsureIndexAsync();
    }
    catch (Exception ex)
    {
        // Elasticsearch is a read-model dependency, not a hard requirement to serve traffic -
        // don't let a transient ES outage/misconfiguration take down the whole API on boot.
        app.Logger.LogError(ex, "Failed to ensure the product search index exists. Search endpoints will be degraded until Elasticsearch connectivity is restored.");
    }
}

app.UseRedisTracing();
app.UseApplication();

app.Run();
