using Microsoft.AspNetCore.Server.Kestrel.Core;

using NovaCore.BuildingBlock.Infrastructure.Observability;
using NovaCore.BuildingBlock.Messaging.Kafka.Tracing;
using NovaCore.BuildingBlock.Observability.Logging;
using NovaCore.BuildingBlock.Observability.Tracing;
using NovaCore.BuildingBlock.Persistence.Mongo.DependencyInjection;

using Serilog;

using NovaCore.Notification.API;
using NovaCore.Notification.Application;
using NovaCore.Notification.Infrastructure;
using NovaCore.Notification.Persistence;
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) => config.ConfigureAppLogging(context.Configuration, "notification-api"));

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
    .AddOpenTelemetryObservability(builder.Configuration, "notification-api", tracing => tracing
        .AddMongoTracing()
        .AddKafkaMessagingTracing()
        .AddInfrastructureTracing());

var app = builder.Build();

app.UseRedisTracing();

// No migration step here - Mongo is schemaless. The "notifications" collection and its indexes
// are created once by scripts/mongodb/init-mongo.js when the mongo container first initializes;
// Outbox/Inbox collection indexes are created by NotificationMongoContext's constructor instead
// (see NovaCore.BuildingBlock.Persistence.Mongo's Outbox/Inbox EnsureXIndexes() extensions).

app.UseApplication();

app.Run();
