namespace NovaCore.Auth.Application.Common;

/// <summary>Shared invalid-attempt thresholds for code/token validation flows (email confirmation, password reset).</summary>
public static class VerificationAttemptPolicy
{
    public const int MaxInvalidAttempts = 3;
    public static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(3);
}
