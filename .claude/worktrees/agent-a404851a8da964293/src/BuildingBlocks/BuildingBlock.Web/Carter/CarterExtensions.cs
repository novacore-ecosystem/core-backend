using Carter;

using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.BuildingBlock.Web.Carter;

public static class CarterExtensions
{
    public static IServiceCollection AddCarterModules(
        this IServiceCollection services,
        params Type[] assembliesToScan)
    {
        var assemblyCatelog = assembliesToScan.Length > 0
            ? new DependencyContextAssemblyCatalog(
                [.. assembliesToScan.Select(x => x.Assembly)])
            : null;
        services.AddCarter(assemblyCatelog);

        return services;
    }
}
