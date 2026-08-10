using NovaCore.Inventory.Domain.Metadata;

namespace NovaCore.Inventory.Domain.Entities.Warehouses;

public sealed class Warehouse : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public WarehouseType Type { get; private set; }
    public WarehouseStatus Status { get; private set; }
    public int Priority { get; private set; }
    public string TimeZone { get; private set; } = string.Empty;
    public Address Address { get; private set; } = default!;
    public GeoLocation? Location { get; private set; }
    public ContactInfo? Contact { get; private set; }
    public WarehouseCapacity? Capacity { get; private set; }
    public bool SupportsReceiving { get; private set; } = true;
    public bool SupportsShipping { get; private set; } = true;
    public bool SupportsReservation { get; private set; } = true;
    public bool SupportsTransfer { get; private set; } = true;
    public bool SupportsPicking { get; private set; } = true;
    public bool SupportsReturns { get; private set; } = true;
    public bool AllowNegativeStock { get; private set; }
    public WarehouseMetadata Metadata { get; private set; } = default!;

    public ICollection<WarehouseZone> Zones { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private Warehouse() { }

    public static Warehouse Create(
        string code,
        string name,
        WarehouseType type,
        Address address,
        int priority = 0,
        string timeZone = "UTC")
    {
        if (code.IsNullOrWhiteSpace())
            throw ExceptionFactory.RequiredField("Warehouse code is required.");

        if (name.IsNullOrWhiteSpace())
            throw ExceptionFactory.RequiredField("Warehouse name is required.");

        return new Warehouse
        {
            Id = Guid.CreateVersion7(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Type = type,
            Address = address,
            Priority = priority,
            TimeZone = timeZone,
            Status = WarehouseStatus.Active,
            Metadata = WarehouseMetadata.Create<WarehouseMetadata>(),
        };
    }
    #endregion

    #region Details
    public void UpdateDetails(string name, string description, int priority)
    {
        if (name.IsNullOrWhiteSpace())
            throw ExceptionFactory.RequiredField("Warehouse name is required.");

        Name = name.Trim();
        Description = description;
        Priority = priority;
    }

    public void UpdateAddress(Address address)
    {
        Address = address;
    }

    public void UpdateLocation(GeoLocation? location)
    {
        Location = location;
    }

    public void UpdateContact(ContactInfo? contact)
    {
        Contact = contact;
    }

    public void UpdateCapacity(WarehouseCapacity? capacity)
    {
        Capacity = capacity;
    }

    public void ConfigureCapabilities(
        bool supportsReceiving,
        bool supportsShipping,
        bool supportsReservation,
        bool supportsTransfer,
        bool supportsPicking,
        bool supportsReturns,
        bool allowNegativeStock)
    {
        SupportsReceiving = supportsReceiving;
        SupportsShipping = supportsShipping;
        SupportsReservation = supportsReservation;
        SupportsTransfer = supportsTransfer;
        SupportsPicking = supportsPicking;
        SupportsReturns = supportsReturns;
        AllowNegativeStock = allowNegativeStock;
    }
    #endregion

    #region Status
    public void Activate()
    {
        if (Status == WarehouseStatus.Closed)
            throw ExceptionFactory.InvalidStatus("Cannot activate a closed warehouse.");

        Status = WarehouseStatus.Active;
    }

    public void Deactivate()
    {
        if (Status == WarehouseStatus.Closed)
            throw ExceptionFactory.InvalidStatus("Cannot deactivate a closed warehouse.");

        Status = WarehouseStatus.Inactive;
    }

    public void SetMaintenance()
    {
        if (Status == WarehouseStatus.Closed)
            throw ExceptionFactory.InvalidStatus("Cannot set a closed warehouse to maintenance.");

        Status = WarehouseStatus.Maintenance;
    }

    public void Close()
    {
        if (Status == WarehouseStatus.Closed)
            throw ExceptionFactory.InvalidStatus("Warehouse is already closed.");

        Status = WarehouseStatus.Closed;
    }
    #endregion

    #region Zones
    /// <summary>Only Warehouse may construct/mutate its Zones - same reasoning Order applies to OrderItem.</summary>
    public WarehouseZone AddZone(
        string code,
        string name,
        WarehouseZoneType type,
        int priority = 0,
        WarehouseCapacity? capacity = null,
        TemperatureRange? temperature = null,
        HumidityRange? humidity = null,
        PickingStrategy pickingStrategy = PickingStrategy.FIFO,
        bool allowMixedLot = false)
    {
        if (Zones.Any(z => z.Code.Equals(code, StringComparison.OrdinalIgnoreCase)))
            throw ExceptionFactory.Duplicate($"A zone with code '{code}' already exists in this warehouse.");

        var zone = WarehouseZone.Create(
            Id,
            code,
            name,
            type,
            priority,
            capacity,
            temperature,
            humidity,
            pickingStrategy,
            allowMixedLot);

        Zones.Add(zone);

        return zone;
    }

    public void RemoveZone(Guid zoneId)
    {
        var zone = Zones.FirstOrDefault(z => z.Id == zoneId)
            ?? throw ExceptionFactory.EntityNotFound<WarehouseZone>(zoneId);

        Zones.Remove(zone);
    }
    #endregion
}
