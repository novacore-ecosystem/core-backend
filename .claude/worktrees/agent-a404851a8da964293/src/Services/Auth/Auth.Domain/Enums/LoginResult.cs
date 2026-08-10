namespace NovaCore.Auth.Domain.Enums;

public enum LoginResult : short
{
    Success = 0,
    InvalidCredentials = 1,
    AccountLocked = 2,
    AccountSuspended = 3,
    MfaRequired = 4,
    MfaFailed = 5,
}
