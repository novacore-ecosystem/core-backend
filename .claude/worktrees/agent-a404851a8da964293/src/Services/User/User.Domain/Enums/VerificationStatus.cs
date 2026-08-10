namespace NovaCore.User.Domain.Enums;

public enum VerificationStatus : byte
{
    Pending = 1,
    Verified = 2,
    Rejected = 3,
    Expired = 4,
}
