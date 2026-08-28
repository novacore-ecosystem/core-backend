using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

using NovaCore.BuildingBlock.Infrastructure.Observability;
using NovaCore.BuildingBlock.Messaging.Kafka.Tracing;
using NovaCore.BuildingBlock.Observability.Logging;
using NovaCore.BuildingBlock.Observability.Tracing;
using NovaCore.BuildingBlock.Web.Extensions;

using Serilog;

using NovaCore.Content.API;
using NovaCore.Content.Application;
using NovaCore.Content.Infrastructure;
using NovaCore.Content.Persistence;
using NovaCore.Content.Persistence.Engine;

var builder = WebApplication.CreateBuilder(args);

await builder.Configuration.AddVaultSecretsAsync();

builder.Host.UseSerilog((context, config) => config.ConfigureAppLogging(context.Configuration, "content-api"));

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
    .AddOpenTelemetryObservability(builder.Configuration, "content-api", tracing => tracing
        .AddPersistenceTracing()
        .AddKafkaMessagingTracing());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ContentDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseApplication();

app.Run();
