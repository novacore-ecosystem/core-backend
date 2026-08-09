namespace NovaCore.Auth.Domain.Enums;

public enum TenantClientStatus : byte
{
    Active = 0,
    Revoked = 1,
    Expired = 2,
}
