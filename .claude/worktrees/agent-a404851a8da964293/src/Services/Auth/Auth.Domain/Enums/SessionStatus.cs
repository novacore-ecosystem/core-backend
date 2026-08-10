namespace NovaCore.Auth.Domain.Enums;

public enum SessionStatus : byte
{
    Active = 0,
    Expired = 1,
    Revoked = 2,
    LoggedOut = 3,
}
