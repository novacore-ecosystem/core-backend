namespace NovaCore.Order.Domain.Enums;

public enum ShippingMethod : short
{
    /// <summary>Pickup shipping method, where the customer picks up the order from a designated location.</summary>
    Pickup = 1,
    /// <summary>Standard shipping method, typically involving regular delivery times.</summary>
    Standard = 2,
    /// <summary>Express shipping method, offering faster delivery times.</summary>
    Express = 3,
    /// <summary>Same-day shipping method, delivering the order on the same day of purchase.</summary>
    SameDay = 4,
    /// <summary>Next-day shipping method, delivering the order the day after purchase.</summary>
    NextDay = 5,
    /// <summary>Scheduled shipping method, allowing customers to specify a preferred delivery date.</summary>
    Scheduled = 6,
    /// <summary>Overnight shipping method, delivering the order within 24 hours of purchase.</summary>
    Overnight = 7,
}
