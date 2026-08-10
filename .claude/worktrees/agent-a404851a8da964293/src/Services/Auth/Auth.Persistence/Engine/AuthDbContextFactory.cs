using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NovaCore.Auth.Persistence.Engine;

/// <summary>
/// Design-time factory used only by `dotnet ef migrations` tooling. Building AuthDbContext
/// through the full application host fails at design time - the Identity 10.0
/// AddEntityFrameworkStores&lt;AuthDbContext&gt; registration resolves a UserStore generic
/// signature the host's DI container can't satisfy for design-time activation - so migrations
/// are generated against a bare DbContextOptions instead. Mirrors AddPersistenceDbContext's
/// options (snake_case naming convention); no application service provider is needed since the
/// Entity Convention (see docs/reference/tenant-convention.md) is pure reflection plus
/// NovaCore.BuildingBlock.SharedKernel.Context.RequestContext, a static ambient accessor rather
/// than a DI-resolved service. Runtime resolution (Auth.API via AddPersistence) is unaffected.
/// </summary>
public sealed class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
        optionsBuilder.UseNpgsql("Server=localhost;Port=5432;Database=auth_db;User Id=postgres;Password=postgres;");
        optionsBuilder.UseSnakeCaseNamingConvention();

        return new AuthDbContext(optionsBuilder.Options);
    }
}
