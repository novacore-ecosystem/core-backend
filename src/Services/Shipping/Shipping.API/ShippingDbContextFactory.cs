using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using NovaCore.Shipping.Persistence.Engine;

namespace NovaCore.Shipping.API;

/// <summary>
/// Lets `dotnet ef` build <see cref="ShippingDbContext"/> directly, without booting the full app
/// host (Kafka producers, Redis, APM, ...) that <c>Program.cs</c> wires up. Tooling-only - never
/// invoked at runtime.
/// </summary>
public sealed class ShippingDbContextFactory : IDesignTimeDbContextFactory<ShippingDbContext>
{
    public ShippingDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=localhost;Port=5432;Database=shipping_db;User Id=postgres;Password=NovaCore@Postgres2026;";

        var optionsBuilder = new DbContextOptionsBuilder<ShippingDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        optionsBuilder.UseSnakeCaseNamingConvention();

        return new ShippingDbContext(optionsBuilder.Options);
    }
}
