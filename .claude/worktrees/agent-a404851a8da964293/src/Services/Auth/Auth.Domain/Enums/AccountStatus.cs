namespace NovaCore.Auth.Domain.Enums;

public enum AccountStatus : byte
{
    Active = 0,
    Inactive = 1,
    Locked = 2,
    Suspended = 3,
    Deleted = 4,
}
