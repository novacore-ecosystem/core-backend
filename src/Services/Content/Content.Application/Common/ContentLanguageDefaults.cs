using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.Content.Application.Common;

/// <summary>
/// Fallback language used across create/update/read/publish/translate/landing flows whenever a
/// caller omits an explicit language - the "single-language tenant" case (see WCM spec section 7).
/// TODO: replace with a real per-tenant default once Content Service can read tenant configuration
/// (Auth Service owns Tenant/TenantLocale today, and no cross-service lookup for it exists yet) -
/// until then every tenant effectively shares this one service-wide default.
/// </summary>
public static class ContentLanguageDefaults
{
    public const string Default = LanguageCodeConstant.English;

    public static string OrDefault(string? language) =>
        string.IsNullOrWhiteSpace(language) ? Default : language;
}
