namespace NovaCore.Shipping.Domain.Enums;

/// <summary>Verification state shared by VerifiedShippingAddress and ShippingProfile.</summary>
public enum VerificationStatus
{
    Unverified = 1,
    Pending = 2,
    Verified = 3,
    Rejected = 4,
}
