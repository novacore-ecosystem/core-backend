using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

using NovaCore.BuildingBlock.Infrastructure.Observability;
using NovaCore.BuildingBlock.Messaging.Kafka.Tracing;
using NovaCore.BuildingBlock.Observability.Logging;
using NovaCore.BuildingBlock.Observability.Tracing;
using NovaCore.BuildingBlock.Web.Extensions;

using Serilog;

using NovaCore.Promotion.API;
using NovaCore.Promotion.Application;
using NovaCore.Promotion.Application.Abstractions.Search;
using NovaCore.Promotion.Infrastructure;
using NovaCore.Promotion.Persistence;
using NovaCore.Promotion.Persistence.Engine;

var builder = WebApplication.CreateBuilder(args);

await builder.Configuration.AddVaultSecretsAsync();

builder.Host.UseSerilog((context, config) => config.ConfigureAppLogging(context.Configuration, "promotion-api"));

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
    .AddOpenTelemetryObservability(builder.Configuration, "promotion-api", tracing => tracing
        .AddPersistenceTracing()
        .AddKafkaMessagingTracing());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PromotionDbContext>();
    await dbContext.Database.MigrateAsync();

    var couponSearchIndexer = scope.ServiceProvider.GetRequiredService<ICouponSearchIndexer>();
    try
    {
        await couponSearchIndexer.EnsureIndexAsync();
    }
    catch (Exception ex)
    {
        // Elasticsearch is a read-model dependency, not a hard requirement to serve traffic -
        // don't let a transient ES outage/misconfiguration take down the whole API on boot.
        app.Logger.LogError(ex, "Failed to ensure the coupon search index exists. Search endpoints will be degraded until Elasticsearch connectivity is restored.");
    }
}

app.UseRedisTracing();
app.UseApplication();

app.Run();
