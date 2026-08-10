using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.Auth.Persistence.Storage.Seeders;

public static class SeedingExtensions
{
    public static async Task SeedDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }
}
