using Microsoft.AspNetCore.Server.Kestrel.Core;

using NovaCore.BuildingBlock.Infrastructure.Observability;
using NovaCore.BuildingBlock.Messaging.Kafka.Tracing;
using NovaCore.BuildingBlock.Observability.Logging;
using NovaCore.BuildingBlock.Observability.Tracing;
using NovaCore.BuildingBlock.Web.Extensions;

using Serilog;

using NovaCore.Payment.API;
using NovaCore.Payment.Application;
using NovaCore.Payment.Infrastructure;
using NovaCore.Payment.Persistence;

var builder = WebApplication.CreateBuilder(args);

await builder.Configuration.AddVaultSecretsAsync();

builder.Host.UseSerilog((context, config) => config.ConfigureAppLogging(context.Configuration, "payment-api"));

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
    .AddOpenTelemetryObservability(builder.Configuration, "payment-api", tracing => tracing
        .AddPersistenceTracing()
        .AddKafkaMessagingTracing());

var app = builder.Build();

// Migrations and seeding are Payment.DbMigrator's responsibility, run before this service starts.
app.UseRedisTracing();
app.UseApplication();

app.Run();
