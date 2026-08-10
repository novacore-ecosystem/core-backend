using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NovaCore.Inventory.Persistence.Engine;

/// <summary>
/// Design-time factory used only by `dotnet ef migrations` tooling. EF's design-time host
/// activation doesn't resolve the full application DI container, so DbContextBase used to need an
/// explicit application service provider supplying the Tenant Convention's DI dependencies. The
/// Entity Convention (see docs/reference/tenant-convention.md) is now pure reflection plus
/// NovaCore.BuildingBlock.SharedKernel.Context.RequestContext (a static ambient accessor, not a
/// DI-resolved service), so no application service provider is required here at all. Runtime
/// resolution (Inventory.API via AddPersistence) is unaffected.
/// </summary>
public sealed class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();
        optionsBuilder.UseNpgsql("Server=localhost;Port=5432;Database=inventory_db;User Id=postgres;Password=postgres;");
        optionsBuilder.UseSnakeCaseNamingConvention();

        return new InventoryDbContext(optionsBuilder.Options);
    }
}
