namespace NovaCore.Shipping.Domain.Enums;

/// <summary>Connection state of a CarrierIntegration's credentials/endpoint - no provider API is actually called yet.</summary>
public enum IntegrationStatus
{
    NotConfigured = 1,
    Active = 2,
    Suspended = 3,
    Failed = 4,
}
