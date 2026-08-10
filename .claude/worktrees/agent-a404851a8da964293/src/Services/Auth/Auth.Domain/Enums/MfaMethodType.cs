namespace NovaCore.Auth.Domain.Enums;

public enum MfaMethodType : byte
{
    Totp = 0,
    Sms = 1,
    Email = 2,
    BackupCode = 3,
}
