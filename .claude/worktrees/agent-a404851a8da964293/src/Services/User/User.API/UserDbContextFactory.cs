using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using NovaCore.User.Persistence.Engine;

namespace NovaCore.User.API;

/// <summary>
/// Lets `dotnet ef` build <see cref="UserDbContext"/> directly, without booting the full app host
/// (Kafka producers, gRPC clients, Redis, ...) that <c>Program.cs</c> wires up. Tooling-only - never
/// invoked at runtime. Reads the same env var Program.cs uses, falling back to a local default for
/// convenience when running against the docker-compose Postgres via its host-mapped port.
/// </summary>
public sealed class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=localhost;Port=5432;Database=user_db;User Id=postgres;Password=NovaCore@Postgres2026;";

        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        optionsBuilder.UseSnakeCaseNamingConvention();

        return new UserDbContext(optionsBuilder.Options);
    }
}
