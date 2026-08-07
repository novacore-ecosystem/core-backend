namespace NovaCore.Shipping.Domain.Entities.TransportationVehicles;

/// <summary>
/// A vehicle that can be assigned to a Transportation. Standalone aggregate root referencing its
/// ShippingProvider by id - capacity and availability are its own long-lived state, not a
/// per-trip attribute.
/// </summary>
public sealed class TransportationVehicle : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid ProviderId { get; private set; }
    public string PlateNumber { get; private set; } = string.Empty;
    public string? Model { get; private set; }
    public decimal CapacityKg { get; private set; }
    public decimal? CapacityM3 { get; private set; }
    public VehicleStatus Status { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private TransportationVehicle() { }

    public static TransportationVehicle Create(
        Guid providerId,
        string plateNumber,
        decimal capacityKg,
        string? model = null,
        decimal? capacityM3 = null)
    {
        if (providerId == Guid.Empty)
            throw ExceptionFactory.RequiredField("Provider id is required.");

        if (string.IsNullOrWhiteSpace(plateNumber))
            throw ExceptionFactory.RequiredField("Vehicle plate number cannot be empty.");

        if (capacityKg <= 0)
            throw ExceptionFactory.InvalidRange("Vehicle capacity must be greater than zero.");

        if (capacityM3 is <= 0)
            throw ExceptionFactory.InvalidRange("Vehicle volume capacity must be greater than zero.");

        return new TransportationVehicle
        {
            Id = Guid.CreateVersion7(),
            ProviderId = providerId,
            PlateNumber = plateNumber.Trim().ToUpperInvariant(),
            Model = model?.Trim(),
            CapacityKg = capacityKg,
            CapacityM3 = capacityM3,
            Status = VehicleStatus.Available,
        };
    }

    public void MarkInUse() => Status = VehicleStatus.InUse;

    public void MarkAvailable() => Status = VehicleStatus.Available;

    public void SendToMaintenance() => Status = VehicleStatus.Maintenance;

    public void Retire() => Status = VehicleStatus.Inactive;
}
