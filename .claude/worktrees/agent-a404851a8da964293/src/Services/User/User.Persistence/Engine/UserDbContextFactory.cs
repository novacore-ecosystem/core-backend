using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NovaCore.User.Persistence.Engine;

/// <summary>
/// Design-time factory used only by `dotnet ef migrations` tooling. EF's design-time host
/// activation doesn't resolve the full application DI container, so DbContextBase used to need an
/// explicit application service provider supplying the Tenant Convention's DI dependencies. The
/// Entity Convention (see docs/reference/tenant-convention.md) is now pure reflection plus
/// NovaCore.BuildingBlock.SharedKernel.Context.RequestContext (a static ambient accessor, not a
/// DI-resolved service), so no application service provider is required here at all. Runtime
/// resolution (User.API via AddPersistence) is unaffected.
/// </summary>
public sealed class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        optionsBuilder.UseNpgsql("Server=localhost;Port=5432;Database=user_db;User Id=postgres;Password=postgres;");
        optionsBuilder.UseSnakeCaseNamingConvention();

        return new UserDbContext(optionsBuilder.Options);
    }
}
