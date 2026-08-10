namespace NovaCore.User.Domain.Enums;

public enum UserStatus : byte
{
    Active = 1,
    Inactive = 2,
    Suspended = 3,
    Locked = 4,
    PendingVerification = 5,
    Deleted = 6,
}
