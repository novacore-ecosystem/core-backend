using NovaCore.Auth.API;
using NovaCore.Auth.Application;
using NovaCore.Auth.Infrastructure;
using NovaCore.Auth.Persistence;
using NovaCore.Auth.Persistence.Engine;

using NovaCore.BuildingBlock.Infrastructure.Observability;
using NovaCore.BuildingBlock.Messaging.Kafka.Tracing;
using NovaCore.BuildingBlock.Observability.Logging;
using NovaCore.BuildingBlock.Observability.Tracing;

using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

using Serilog;
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) => config.ConfigureAppLogging(context.Configuration, "auth-api"));

// Expose REST and gRPC on separate ports
builder.WebHost.ConfigureKestrel(options =>
{

    // Public REST API
    var httpPort = int.Parse(builder.Configuration["ASPNETCORE_HTTP_PORT"] ?? "8080");
    options.ListenAnyIP(httpPort, listen =>
    {
        listen.Protocols = HttpProtocols.Http1;
    });

    // Internal gRPC
    var grpcPort = int.Parse(builder.Configuration["ASPNETCORE_GRPC_PORT"] ?? "5002");
    options.ListenAnyIP(grpcPort, listen =>
    {
        listen.Protocols = HttpProtocols.Http2;
    });
});

// Add services
builder.Services
    .AddPersistence(builder.Configuration)
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPresentation(builder.Configuration)
    .AddOpenTelemetryObservability(builder.Configuration, "auth-api", tracing => tracing
        .AddPersistenceTracing()
        .AddKafkaMessagingTracing()
        .AddInfrastructureTracing());

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseRedisTracing();

// Configure middleware pipeline
app.UseApplication();

app.Run();
