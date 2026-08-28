using Microsoft.AspNetCore.Server.Kestrel.Core;

using NovaCore.BuildingBlock.Infrastructure.Observability;
using NovaCore.BuildingBlock.Observability.Logging;
using NovaCore.BuildingBlock.Observability.Tracing;
using NovaCore.BuildingBlock.Web.Extensions;

using Serilog;

using NovaCore.Chat.API;
using NovaCore.Chat.Application;
using NovaCore.Chat.Infrastructure;
using NovaCore.Chat.Persistence;

var builder = WebApplication.CreateBuilder(args);

await builder.Configuration.AddVaultSecretsAsync();

builder.Host.UseSerilog((context, config) => config.ConfigureAppLogging(context.Configuration, "chat-api"));

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
    .AddInfrastructure()
    .AddPresentation(builder.Configuration)
    .AddOpenTelemetryObservability(builder.Configuration, "chat-api", tracing => tracing
        .AddPersistenceTracing()
        .AddInfrastructureTracing());

var app = builder.Build();

// Migrations and seeding are Chat.DbMigrator's responsibility, run before this service starts.
app.UseApplication();

app.Run();
