namespace NovaCore.Auth.Application.Abstractions.Apps;

/// <summary>
/// Lightweight, JSON-serializable projection of an App for the App collection cache - only the
/// fields Login/Register/RefreshToken actually need (Code resolution, active-status check, the
/// app_id token claim), not the full aggregate (e.g. no Translations).
/// </summary>
public sealed record CachedApp(Guid Id, string Code, string Name, bool IsActive);
