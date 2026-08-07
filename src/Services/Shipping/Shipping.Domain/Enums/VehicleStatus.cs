namespace NovaCore.Shipping.Domain.Enums;

/// <summary>Operational availability of a TransportationVehicle.</summary>
public enum VehicleStatus
{
    Available = 1,
    InUse = 2,
    Maintenance = 3,
    Inactive = 4,
}
