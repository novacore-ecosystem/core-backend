namespace NovaCore.Shipping.Domain.Enums;

/// <summary>How a ShippingProvider is operated - external carrier, the company's own fleet, or an individual freelancer.</summary>
public enum ProviderType
{
    External = 1,
    Internal = 2,
    Freelancer = 3,
}
