namespace NovaCore.BuildingBlock.Application.Abstractions.Services;

public interface ICurrentLocaleService
{
    /// <summary>
    /// The caller's locale for this request (e.g. "en", "vi-VN"), resolved from the request's
    /// Accept-Language header. Falls back to a default (see implementation) when the header is
    /// missing, empty, or unparseable - never null, never throws.
    /// </summary>
    string GetLocale();
}
