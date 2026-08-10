namespace NovaCore.Payment.Domain.Enums;

/// <summary>
/// The business module a Payment/Refund/Invoice is linked to, via ReferenceType + ReferenceId.
/// PaymentService never depends on the referenced service's assemblies or database - this enum
/// is the entire integration surface. Extend with new values as new consumer modules onboard;
/// never branch domain logic on a specific value (e.g. Order) inside this service.
/// </summary>
public enum ReferenceType : byte
{
    Order = 1,
    Subscription = 2,
    WalletTopup = 3,
    Invoice = 4,
    Booking = 5,
    Donation = 6,
    Membership = 7,
    Manual = 8,
    Other = 99,
}
