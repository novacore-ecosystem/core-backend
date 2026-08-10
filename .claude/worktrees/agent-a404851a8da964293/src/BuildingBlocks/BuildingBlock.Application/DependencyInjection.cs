using NovaCore.BuildingBlock.Application.Behaviors;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.BuildingBlock.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationBehaviors(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services;
    }
}
