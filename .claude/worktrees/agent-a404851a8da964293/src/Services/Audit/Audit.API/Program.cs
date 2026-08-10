using Microsoft.AspNetCore.Server.Kestrel.Core;

using Serilog;

using NovaCore.Audit.API;
using NovaCore.Audit.Application;
using NovaCore.Audit.Infrastructure;
using NovaCore.Audit.Persistence;
var builder = WebApplication.CreateBuilder(args);

var seqUrl = builder.Configuration["Logging:Seq:Url"] ?? "http://seq:5341";
builder.Host.UseSerilog((context, config) =>
{
    config
        .MinimumLevel.Information()
        .WriteTo.Console()
        .WriteTo.Seq(seqUrl);
});

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
    .AddPresentation(builder.Configuration);

var app = builder.Build();

// No migration step here - Mongo is schemaless. The "logs" collection and its indexes are
// created once by scripts/mongodb/init-mongo.js when the mongo container first initializes;
// Outbox/Inbox collection indexes are created by AuditMongoContext's constructor instead
// (see NovaCore.BuildingBlock.Persistence.Mongo's Outbox/Inbox EnsureXIndexes() extensions).

app.UseApplication();

app.Run();
