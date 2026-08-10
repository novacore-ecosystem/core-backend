using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using NovaCore.Inventory.Persistence.Engine;

namespace NovaCore.Inventory.API;

/// <summary>
/// Lets `dotnet ef` build <see cref="InventoryDbContext"/> directly, without booting the full app
/// host (Kafka producers, Hangfire, ...) that <c>Program.cs</c> wires up. Tooling-only - never
/// invoked at runtime. Reads the same env var Program.cs uses, falling back to a local default for
/// convenience when running against the docker-compose Postgres via its host-mapped port.
/// </summary>
public sealed class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=localhost;Port=5432;Database=inventory_db;User Id=postgres;Password=NovaCore@Postgres2026;";

        var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        optionsBuilder.UseSnakeCaseNamingConvention();

        return new InventoryDbContext(optionsBuilder.Options);
    }
}
