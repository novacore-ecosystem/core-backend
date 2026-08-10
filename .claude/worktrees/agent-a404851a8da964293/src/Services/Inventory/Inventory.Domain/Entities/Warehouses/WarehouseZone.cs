using NovaCore.Inventory.Domain.Metadata;

namespace NovaCore.Inventory.Domain.Entities.Warehouses;

public sealed class WarehouseZone : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Guid WarehouseId { get; private set; }
    public Warehouse Warehouse { get; private set; } = default!;
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public WarehouseZoneType Type { get; private set; }
    public WarehouseZoneStatus Status { get; private set; }
    public int Priority { get; private set; }
    public WarehouseCapacity? Capacity { get; private set; }
    public TemperatureRange? Temperature { get; private set; }
    public HumidityRange? Humidity { get; private set; }
    public PickingStrategy PickingStrategy { get; private set; }
    public bool AllowMixedLot { get; private set; }
    public WarehouseZoneMetadata Metadata { get; private set; } = default!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private WarehouseZone() { }

    /// <summary>Only Warehouse may construct/mutate its Zones - see Warehouse.AddZone.</summary>
    internal static WarehouseZone Create(
        Guid warehouseId,
        string code,
        string name,
        WarehouseZoneType type,
        int priority,
        WarehouseCapacity? capacity,
        TemperatureRange? temperature,
        HumidityRange? humidity,
        PickingStrategy pickingStrategy,
        bool allowMixedLot)
    {
        if (code.IsNullOrWhiteSpace())
            throw ExceptionFactory.RequiredField("Zone code is required.");

        if (name.IsNullOrWhiteSpace())
            throw ExceptionFactory.RequiredField("Zone name is required.");

        return new WarehouseZone
        {
            Id = Guid.CreateVersion7(),
            WarehouseId = warehouseId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Type = type,
            Status = WarehouseZoneStatus.Active,
            Priority = priority,
            Capacity = capacity,
            Temperature = temperature,
            Humidity = humidity,
            PickingStrategy = pickingStrategy,
            AllowMixedLot = allowMixedLot,
            Metadata = WarehouseZoneMetadata.Create<WarehouseZoneMetadata>(),
        };
    }

    public void UpdateDetails(string name, string description, int priority)
    {
        if (name.IsNullOrWhiteSpace())
            throw ExceptionFactory.RequiredField("Zone name is required.");

        Name = name.Trim();
        Description = description;
        Priority = priority;
    }

    public void UpdateEnvironment(TemperatureRange? temperature, HumidityRange? humidity)
    {
        Temperature = temperature;
        Humidity = humidity;
    }

    public void Activate()
    {
        Status = WarehouseZoneStatus.Active;
    }

    public void Deactivate()
    {
        Status = WarehouseZoneStatus.Inactive;
    }

    public void SetMaintenance()
    {
        Status = WarehouseZoneStatus.Maintenance;
    }
}
