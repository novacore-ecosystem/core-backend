# Microservice Development Template

**Based on:** Auth Service pattern (production-ready reference implementation)  
**Last Updated:** 2026-07-09  
**Applies to:** All new microservices (User, Product, Order, etc.)

---

## 📋 Quick Start Checklist

Use this checklist when creating a new service:

- [ ] **Project Structure** - Create 4-layer projects
- [ ] **Domain Layer** - Define entities with factory methods
- [ ] **Persistence Layer** - Configure DbContext, configs, seeders
- [ ] **Application Layer** - Add CQRS handlers, validators, mappers
- [ ] **Infrastructure Layer** - Implement services, security, caching
- [ ] **API Layer** - Setup endpoints, middleware, health checks
- [ ] **Configuration** - Update docker-compose.yml, environment variables
- [ ] **Documentation** - Create SERVICE_NAME.md guide

---

## 🏗️ Project Structure

### Create 4-Layer Projects

```
src/Services/[ServiceName]/
├── [ServiceName].Domain/           # Entities, enums, value objects
├── [ServiceName].Persistence/      # DbContext, configs, seeders
├── [ServiceName].Application/      # CQRS, validators, mappers
├── [ServiceName].Infrastructure/   # Services, security, caching
└── [ServiceName].API/              # Endpoints, middleware, Program.cs
```

### Create .csproj Files

**Domain Project:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  
  <ItemGroup>
    <ProjectReference Include="../../BuildingBlocks/BuildingBlock.Domain/BuildingBlock.Domain.csproj" />
  </ItemGroup>
</Project>
```

**Persistence, Application, Infrastructure, API:** Similar structure with appropriate project references.

---

## 🎯 Layer-by-Layer Implementation

### 1. Domain Layer

#### Define Entities

Use factory methods for creation, immutable value objects, audit timestamps:

```csharp
namespace User.Domain.Entities;

using BuildingBlock.Domain.Abstractions;

public sealed class User : IEntity
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string UserName { get; private set; }
    public string PhoneNumber { get; private set; }
    public UserStatus Status { get; private set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    private User() { }

    public static User Create(
        string email,
        string userName,
        string phoneNumber,
        UserStatus status = UserStatus.Active)
    {
        return new User
        {
            Id = Guid.CreateVersion7(),
            Email = email,
            UserName = userName,
            PhoneNumber = phoneNumber,
            Status = status,
        };
    }

    public void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Status = UserStatus.Inactive;
        Touch();
    }
}
```

#### Define Enums

```csharp
namespace User.Domain.Enums;

public enum UserStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3,
}
```

### 2. Persistence Layer

#### Create DbContext

```csharp
namespace User.Persistence;

using Microsoft.EntityFrameworkCore;
using User.Domain.Entities;

public sealed class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply entity configs
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserDbContext).Assembly);
    }
}
```

#### Create Entity Configurations

```csharp
namespace User.Persistence.Config;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using User.Domain.Entities;

public sealed class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.UserName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(x => x.Status)
            .HasConversion<int>();

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.UserName).IsUnique();
    }
}
```

#### Create Seeders (Optional)

```csharp
namespace User.Persistence.Seeders;

using Microsoft.EntityFrameworkCore;
using User.Domain.Entities;
using User.Domain.Enums;

public sealed class UserSeeder(UserDbContext context)
{
    public async Task SeedAsync()
    {
        if (await context.Users.AnyAsync())
            return;

        var users = new[]
        {
            User.Create("admin@example.com", "admin", "1234567890"),
            User.Create("user@example.com", "user", "0987654321"),
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();
    }
}
```

#### DependencyInjection.cs

```csharp
namespace User.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found");

        services.AddDbContext<UserDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }
}
```

### 3. Application Layer

#### Create CQRS Handlers

**Command:**
```csharp
namespace User.Application.Features.Users.Commands.CreateUser;

using MediatR;

public sealed record CreateUserCommand(
    string Email,
    string UserName,
    string PhoneNumber) : IRequest<CreateUserResponse>;

public sealed record CreateUserResponse(Guid UserId);
```

**Handler:**
```csharp
namespace User.Application.Features.Users.Commands.CreateUser;

using MediatR;
using User.Domain.Entities;
using User.Persistence;

public sealed class CreateUserCommandHandler(UserDbContext context)
    : IRequestHandler<CreateUserCommand, CreateUserResponse>
{
    public async Task<CreateUserResponse> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = User.Create(
            request.Email.Trim(),
            request.UserName.Trim(),
            request.PhoneNumber.Trim());

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateUserResponse(user.Id);
    }
}
```

**Validator:**
```csharp
namespace User.Application.Features.Users.Commands.CreateUser;

using FluentValidation;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.UserName)
            .NotEmpty()
            .Length(3, 50);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(@"^\d{10,}$");
    }
}
```

#### DependencyInjection.cs

```csharp
namespace User.Application;

using BuildingBlock.Application;
using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services
            .AddMediatR()
            .AddApplicationBehaviors()
            .AddMapster()
            .AddFluentValidation();

        return services;
    }

    private static IServiceCollection AddMediatR(this IServiceCollection services)
    {
        services.AddMediatR(config =>
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }

    private static IServiceCollection AddMapster(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(typeof(DependencyInjection).Assembly);

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }

    private static IServiceCollection AddFluentValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
```

### 4. Infrastructure Layer

#### Create Domain Services

```csharp
namespace User.Infrastructure.Services;

using User.Application.Abstractions.Services;
using User.Persistence;

public sealed class UserService(UserDbContext context) : IUserService
{
    public async Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await context.Users.AnyAsync(
            u => u.Email == email,
            cancellationToken);
    }
}
```

#### DependencyInjection.cs

```csharp
namespace User.Infrastructure;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using User.Application.Abstractions.Services;
using User.Infrastructure.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
```

### 5. API Layer

#### Create Carter Endpoints

```csharp
namespace User.API.Endpoints;

using MediatR;
using User.Application.Features.Users.Commands.CreateUser;

public sealed record CreateUserRequest(
    string Email,
    string UserName,
    string PhoneNumber);

public sealed class CreateUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/users", Handle)
            .AllowAnonymous()
            .WithName("CreateUser")
            .WithOpenApi()
            .Produces<ApiResponse<CreateUserResponse>>(StatusCodes.Status201Created);
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateUserRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new CreateUserCommand(
            request.Email.Trim(),
            request.UserName.Trim(),
            request.PhoneNumber.Trim());

        var response = await sender.Send(command, ct);
        
        return Results.Created($"/users/{response.UserId}", 
            ApiResponse<CreateUserResponse>.Ok(response));
    }
}
```

#### Program.cs

```csharp
using User.API;
using User.Application;
using User.Infrastructure;
using User.Persistence;

using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Serilog;

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
    options.ListenAnyIP(5101, listen =>
    {
        listen.Protocols = HttpProtocols.Http1;
    });

    options.ListenAnyIP(5003, listen =>
    {
        listen.Protocols = HttpProtocols.Http2;
    });
});

builder.Services
    .AddPersistence(builder.Configuration)
    .AddInfrastructure(builder.Configuration)
    .AddApplication()
    .AddPresentation(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseApplication();

app.Run();
```

#### DependencyInjection.cs (Presentation)

```csharp
namespace User.API;

using Microsoft.OpenApi.Models;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddSwaggerDocumentation()
            .AddCorsPolicy()
            .AddCarterModules()
            .AddHealthCheckServices()
            .AddAuthorization();

        return services;
    }

    private static IServiceCollection AddSwaggerDocumentation(
        this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "NovaCore User Service",
                Version = "v1",
                Description = "User Management Service API",
                Contact = new OpenApiContact
                {
                    Name = "NovaCore",
                    Url = new Uri("http://localhost:5101")
                }
            });

            options.AddServer(new OpenApiServer { Url = "/api/users" });
        });

        return services;
    }

    private static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        });

        return services;
    }

    private static IServiceCollection AddCarterModules(this IServiceCollection services)
    {
        services.AddCarter();
        return services;
    }

    private static IServiceCollection AddHealthCheckServices(this IServiceCollection services)
    {
        services.AddHealthChecks();
        return services;
    }
}
```

#### ApplicationPipeline.cs

```csharp
namespace User.API;

namespace User.Persistence.Seeders;

public static class ApplicationPipeline
{
    public static WebApplication UseApplication(this WebApplication app)
    {
        app.SeedDatabase();
        app.UseExceptionHandling();
        app.UseSwaggerDocumentation();
        app.UseCorsPolicy();
        app.UseAuthenticationAuthorization();
        app.MapEndpoints();

        return app;
    }

    private static void SeedDatabase(this WebApplication app)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            var seeder = new UserSeeder(context);
            seeder.SeedAsync().Wait();
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Database seeding failed");
            if (app.Environment.IsDevelopment())
                throw;
        }
    }

    private static WebApplication UseExceptionHandling(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
            app.UseDeveloperExceptionPage();
        else
            app.UseExceptionHandler("/error");

        return app;
    }

    private static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("./swagger/v1/swagger.json", "User Service v1");
            options.RoutePrefix = string.Empty;
        });

        return app;
    }

    private static WebApplication UseCorsPolicy(this WebApplication app)
    {
        app.UseCors("AllowAll");
        return app;
    }

    private static WebApplication UseAuthenticationAuthorization(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    private static WebApplication MapEndpoints(this WebApplication app)
    {
        app.MapCarter();
        app.MapHealthChecks("/health");
        return app;
    }
}
```

---

## 🔌 Configuration

### Environment Variables

```env
# Service Name [ServiceName]
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5101
ASPNETCORE_HTTP_PORT=5101
ASPNETCORE_HTTPS_PORT=5102

# Database
ConnectionStrings__DefaultConnection=Server=postgres;Port=5432;Database=[service_name];User Id=postgres;Password=postgres;

# Logging
Logging__Seq__Url=http://seq:5341
```

### Docker Compose Entry

```yaml
[service-name]-api:
  build:
    context: .
    dockerfile: src/Services/[ServiceName]/Dockerfile
  ports:
    - "5101:5101"  # REST API
    - "5003:5003"  # gRPC
  environment:
    ASPNETCORE_ENVIRONMENT: ${ASPNETCORE_ENVIRONMENT}
    ConnectionStrings__DefaultConnection: ${[SERVICE_NAME]_DB_CONNECTION}
    Logging__Seq__Url: ${SEQ_URL}
  depends_on:
    - postgres
    - seq
  networks:
    - novacore
```

---

## 📚 Service Documentation Template

Create `docs/services/[SERVICE_NAME].md`:

```markdown
# [Service Name] Service - Complete Guide

## Overview

Service purpose and responsibilities.

## Configuration

Configuration options and environment variables.

## API Endpoints

List of available endpoints with examples.

## Authentication & Authorization

Security requirements and JWT setup.

## Database Schema

Entity relationships and key tables.

## Running the Service

Local development and Docker commands.

## Troubleshooting

Common issues and solutions.
```

---

## ✅ Pre-Deployment Checklist

- [ ] All layers created with proper DI registration
- [ ] DbContext created and migrations run
- [ ] CQRS handlers with validators implemented
- [ ] Carter endpoints defined
- [ ] Swagger documentation complete
- [ ] Health checks working
- [ ] Environment variables configured
- [ ] Docker build successful
- [ ] Docker compose networking configured
- [ ] Service documentation written
- [ ] CORS policies configured
- [ ] Exception handling in place

---

## 🔗 References

- Exception Handling: [../building-blocks/EXCEPTIONS.md](../building-blocks/EXCEPTIONS.md)
- Development quality checklist: [DEVELOPMENT_CRITERIA.md](DEVELOPMENT_CRITERIA.md)
- Wiring a new service into docker-compose/gateway: [NEW_SERVICE_WORKFLOW.md](NEW_SERVICE_WORKFLOW.md)
- Auth Service: reference implementation

---

## Debugging Tips

### Verify DI Registration
```csharp
var sp = services.BuildServiceProvider();
var service = sp.GetRequiredService<IYourService>();
```

### Migrations
```bash
dotnet ef migrations list
dotnet ef migrations script       # script to SQL
dotnet ef database update PreviousMigration   # revert
```

### Database
```bash
psql -h localhost -U postgres -d your_db
\dt                                # list tables
SELECT * FROM "YourEntities";
```

## GlobalUsings Templates

**Domain:**
```csharp
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
```

**API:**
```csharp
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
global using BuildingBlock.Application.Abstractions;
global using Carter;
global using MediatR;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Routing;
```
**Template Owner:** Development Team
