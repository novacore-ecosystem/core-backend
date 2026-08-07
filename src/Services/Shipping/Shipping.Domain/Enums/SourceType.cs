namespace NovaCore.Shipping.Domain.Enums;

/// <summary>Which business context asked for the shipment. ShippingService never references those modules - only SourceType + SourceReferenceId.</summary>
public enum SourceType
{
    Order = 1,
    WarehouseTransfer = 2,
    SupplierImport = 3,
    Internal = 4,
    Freelance = 5,
}
