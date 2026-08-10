using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using NovaCore.Order.Persistence.Engine;

namespace NovaCore.Order.API;

/// <summary>
/// Lets `dotnet ef` build <see cref="OrderDbContext"/> directly, without booting the full app host
/// (Kafka producers, Redis Cart, APM, ...) that <c>Program.cs</c> wires up. Tooling-only - never
/// invoked at runtime. Reads the same env var Program.cs uses, falling back to a local default for
/// convenience when running against the docker-compose Postgres via its host-mapped port.
/// </summary>
public sealed class OrderDbContextFactory : IDesignTimeDbContextFactory<OrderDbContext>
{
    public OrderDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=localhost;Port=5432;Database=order_db;User Id=postgres;Password=NovaCore@Postgres2026;";

        var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        optionsBuilder.UseSnakeCaseNamingConvention();

        return new OrderDbContext(optionsBuilder.Options);
    }
}
