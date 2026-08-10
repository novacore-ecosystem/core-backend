namespace NovaCore.Order.Domain.Enums;

public enum ShippingStatus : short
{
    /// <summary>Indicates that the shipping is pending.</summary>
    Pending = 1,
    /// <summary>Indicates that the shipping has been shipped.</summary>
    Shipped = 2,
    /// <summary>Indicates that the shipping has arrived.</summary>
    Arrived = 3,
    /// <summary>Indicates that the shipping is in transit.</summary>
    InTransit = 4,
    /// <summary>Indicates that the shipping has been delivered.</summary>
    Delivered = 5,
    /// <summary>Indicates that the shipping has been canceled.</summary>
    Canceled = 6,
}
