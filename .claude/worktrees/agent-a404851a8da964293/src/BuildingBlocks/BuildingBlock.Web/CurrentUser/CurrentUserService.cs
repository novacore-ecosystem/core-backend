using System.Security.Claims;

using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using Microsoft.AspNetCore.Http;

namespace NovaCore.BuildingBlock.Web.CurrentUser;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly HttpContext? _httpContext = httpContextAccessor.HttpContext;

    public Guid? GetUserId()
    {
        var claim = _httpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        return claim is not null && Guid.TryParse(claim.Value, out var userId) ? userId : null;
    }

    public string GetUserEmail()
    {
        return _httpContext?.User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
    }

    public string GetUserName()
    {
        return _httpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
    }

    public List<string> GetRoles()
    {
        return _httpContext?.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? [];
    }

    public bool IsAuthenticated()
    {
        return _httpContext?.User.Identity?.IsAuthenticated ?? false;
    }

    public bool IsInRole(string role)
    {
        return _httpContext?.User.IsInRole(AppRoleConstant.Root) == true
            || _httpContext?.User.IsInRole(role) == true;
    }

    public void SetAccessToken(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddMinutes(15)
        };
        _httpContextAccessor.HttpContext?.Response.Cookies.Append("AccessToken", token, cookieOptions);
    }

    public void SetRefreshToken(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        _httpContextAccessor.HttpContext?.Response.Cookies.Append("RefreshToken", token, cookieOptions);
    }

    public string GetAccessToken()
    {
        if (_httpContextAccessor.HttpContext?.Request.Cookies.TryGetValue("AccessToken", out var token) == true)
            return token;
        return string.Empty;
    }

    public string GetRefreshToken()
    {
        if (_httpContextAccessor.HttpContext?.Request.Cookies.TryGetValue("RefreshToken", out var token) == true)
            return token;
        return string.Empty;
    }

    public void RemoveAccessToken()
    {
        _httpContextAccessor.HttpContext?.Response.Cookies.Delete("AccessToken");
    }

    public void RemoveRefreshToken()
    {
        _httpContextAccessor.HttpContext?.Response.Cookies.Delete("RefreshToken");
    }

    public string GetCorrelationId()
    {
        if (_httpContextAccessor.HttpContext is null)
            return Guid.NewGuid().ToString();

        return _httpContextAccessor.HttpContext.Request.Headers.TryGetValue(
            HeaderKeyConstant.CorrelationId,
            out var correlationIdValue)
                ? correlationIdValue.ToString()
                : Guid.NewGuid().ToString();
    }

    public string? GetIdempotencyKey()
    {
        if (_httpContextAccessor.HttpContext is null)
            return null;

        return _httpContextAccessor.HttpContext.Request.Headers.TryGetValue(
            HeaderKeyConstant.IdempotencyKey,
            out var idempotencyKeyValue)
                ? idempotencyKeyValue.ToString()
                : null;
    }

    public string GetIpAddress()
    {
        if (_httpContext?.Connection.RemoteIpAddress is null)
            return string.Empty;

        var ipAddress = _httpContext.Connection.RemoteIpAddress.ToString();

        if (_httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            ipAddress = forwardedFor.ToString().Split(',')[0];

        return ipAddress;
    }
}
