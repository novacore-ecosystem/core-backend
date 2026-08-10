namespace NovaCore.Auth.Domain.Constants;

public static class SessionDefaults
{
    public const string UnknownDeviceName = "Unknown Device";
    public static readonly TimeSpan DefaultLifetime = TimeSpan.FromDays(30);
}
