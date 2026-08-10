using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

using NovaCore.BuildingBlock.Infrastructure.Observability;
using NovaCore.BuildingBlock.Messaging.Kafka.Tracing;
using NovaCore.BuildingBlock.Observability.Logging;
using NovaCore.BuildingBlock.Observability.Tracing;

using Serilog;

using NovaCore.Payment.API;
using NovaCore.Payment.Application;
using NovaCore.Payment.Infrastructure;
using NovaCore.Payment.Persistence;
using NovaCore.Payment.Persistence.Engine;

var builder = WebApplication.CreateBuilder(args);

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

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseRedisTracing();
app.UseApplication();

app.Run();
