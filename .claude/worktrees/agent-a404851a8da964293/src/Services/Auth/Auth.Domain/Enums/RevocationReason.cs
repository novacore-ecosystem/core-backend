namespace NovaCore.Auth.Domain.Enums;

public enum RevocationReason : short
{
    UserLogout = 0,
    AdminForced = 1,
    PasswordChanged = 2,
    SuspiciousActivity = 3,
    Superseded = 4,
}
