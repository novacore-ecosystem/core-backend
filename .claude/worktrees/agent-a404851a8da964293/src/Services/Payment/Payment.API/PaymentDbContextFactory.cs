using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using NovaCore.Payment.Persistence.Engine;

namespace NovaCore.Payment.API;

/// <summary>
/// Lets `dotnet ef` build <see cref="PaymentDbContext"/> directly, without booting the full app
/// host (Kafka producers, Redis, APM, ...) that <c>Program.cs</c> wires up. Tooling-only - never
/// invoked at runtime.
/// </summary>
public sealed class PaymentDbContextFactory : IDesignTimeDbContextFactory<PaymentDbContext>
{
    public PaymentDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=localhost;Port=5432;Database=payment_db;User Id=postgres;Password=NovaCore@Postgres2026;";

        var optionsBuilder = new DbContextOptionsBuilder<PaymentDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        optionsBuilder.UseSnakeCaseNamingConvention();

        return new PaymentDbContext(optionsBuilder.Options);
    }
}
