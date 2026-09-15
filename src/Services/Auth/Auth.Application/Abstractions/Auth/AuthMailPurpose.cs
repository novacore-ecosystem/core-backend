namespace NovaCore.Auth.Application.Abstractions.Auth;

/// <summary>
/// The authentication email a resend request targets
/// kept explicit so one purpose's token can never be mistaken for another's.
/// </summary>
public enum AuthMailPurpose
{
    EmailVerification = 1,
    PasswordReset = 2,
}
