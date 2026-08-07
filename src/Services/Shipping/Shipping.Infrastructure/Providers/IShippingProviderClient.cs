namespace NovaCore.Shipping.Infrastructure.Providers;

/// <summary>
/// The extension point for talking to an external carrier's API (GHN, VNPost, DHL, ...). One
/// implementation per carrier will live beside this interface, selected by the
/// CarrierIntegration.IntegrationCode of the ShippingProvider bound to a Transportation.
///
/// Deliberately unimplemented in this foundation phase: no carrier contract has been chosen yet,
/// and inventing a lowest-common-denominator shape before a single real integration exists is
/// exactly the speculative abstraction this codebase avoids. The interface exists so the seam is
/// visible and Application code can be written against it - see
/// docs/services/shipping-service.md, "Planned phases".
/// </summary>
public interface IShippingProviderClient
{
    /// <summary>The CarrierIntegration.IntegrationCode this client handles (e.g. "GHN") - used to select the right implementation at runtime.</summary>
    string IntegrationCode { get; }

    /// <summary>Registers a Transportation with the carrier and returns the carrier's own tracking code.</summary>
    Task<string> CreateShipmentAsync(Guid transportationId, CancellationToken ct = default);

    /// <summary>Asks the carrier for the latest status of a previously-registered transportation.</summary>
    Task<string> GetTrackingStatusAsync(string carrierTrackingCode, CancellationToken ct = default);

    /// <summary>Requests cancellation of a previously-registered transportation.</summary>
    Task CancelShipmentAsync(string carrierTrackingCode, CancellationToken ct = default);
}
