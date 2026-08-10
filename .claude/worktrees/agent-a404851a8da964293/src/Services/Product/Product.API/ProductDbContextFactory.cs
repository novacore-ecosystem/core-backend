using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using NovaCore.Product.Persistence.Engine;

namespace NovaCore.Product.API;

/// <summary>
/// Lets `dotnet ef` build <see cref="ProductDbContext"/> directly, without booting the full app host
/// (Kafka producers, Elasticsearch client, ...) that <c>Program.cs</c> wires up. Tooling-only - never
/// invoked at runtime. Reads the same env var Program.cs uses, falling back to a local default for
/// convenience when running against the docker-compose Postgres via its host-mapped port.
/// </summary>
public sealed class ProductDbContextFactory : IDesignTimeDbContextFactory<ProductDbContext>
{
    public ProductDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=localhost;Port=5432;Database=product_db;User Id=postgres;Password=NovaCore@Postgres2026;";

        var optionsBuilder = new DbContextOptionsBuilder<ProductDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        optionsBuilder.UseSnakeCaseNamingConvention();

        return new ProductDbContext(optionsBuilder.Options);
    }
}
