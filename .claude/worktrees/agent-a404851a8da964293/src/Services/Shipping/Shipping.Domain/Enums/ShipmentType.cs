namespace NovaCore.Shipping.Domain.Enums;

/// <summary>What kind of logistics movement a Shipment represents - the same execution machinery serves all of them.</summary>
public enum ShipmentType
{
    Delivery = 1,
    Return = 2,
    WarehouseTransfer = 3,
    SupplierImport = 4,
    Internal = 5,
}
