using NovaCore.Inventory.Domain.ValueObjects;
using NovaCore.Inventory.Persistence.Engine;

namespace NovaCore.Inventory.Persistence.Storage.Seeders;

public sealed class InventorySeeder(InventoryDbContext context)
{
    public async Task SeedAsync()
    {
        if (await context.Warehouses.AnyAsync())
            return;

        var platformAddress = Address.Create(
            country: "Global",
            stateOrProvince: "",
            city: "Platform",
            district: "",
            ward: "",
            street: "Logical Inventory Location",
            postalCode: "");

        var platformWarehouse = Warehouse.Create(
            code: "PLATFORM",
            name: "Platform Store Warehouse",
            type: WarehouseType.Virtual,
            address: platformAddress);

        platformWarehouse.AddZone(
            code: "DEFAULT",
            name: "Default Storage Zone",
            type: WarehouseZoneType.Storage,
            priority: 0,
            capacity: null,
            temperature: null,
            humidity: null,
            pickingStrategy: PickingStrategy.FIFO,
            allowMixedLot: false);

        context.Warehouses.Add(platformWarehouse);
        await context.SaveChangesAsync();
    }
}
