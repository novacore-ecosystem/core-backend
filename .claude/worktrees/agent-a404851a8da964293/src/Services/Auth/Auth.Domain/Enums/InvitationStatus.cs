namespace NovaCore.Auth.Domain.Enums;

public enum InvitationStatus : byte
{
    Pending = 0,
    Accepted = 1,
    Expired = 2,
    Revoked = 3,
}
