using NovaCore.BuildingBlock.Web.Swagger.Filters;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace NovaCore.BuildingBlock.Web.Swagger;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(
        this IServiceCollection services,
        BuildingBlockWebOptions options)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(swaggerOptions =>
        {
            swaggerOptions.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = options.ServiceTitle,
                Version = "v1",
                Description = options.ServiceDescription,
                Contact = new OpenApiContact
                {
                    Name = "NovaCore",
                    Url = new Uri(options.ContactUrl)
                }
            });

            swaggerOptions.AddServer(new OpenApiServer
            {
                Url = options.SwaggerRoutePrefix
            });

            if (options.IncludeJwtBearerSwaggerAuth)
                AddJwtBearerToSwagger(swaggerOptions);

            swaggerOptions.OperationFilter<HeadersOperationFilter>();
        });

        return services;
    }

    private static void AddJwtBearerToSwagger(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options)
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            Description = "Enter JWT token: Bearer {token}"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    }

    public static WebApplication UseSwaggerDocumentation(this WebApplication app, string swaggerUiTitle)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("./swagger/v1/swagger.json", $"{swaggerUiTitle} v1");
            options.RoutePrefix = string.Empty;
        });

        return app;
    }
}
