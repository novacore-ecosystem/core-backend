namespace NovaCore.BuildingBlock.Application.Abstractions.Services;

public interface ICurrentUserService
{
    Guid? GetUserId();

    string GetUserEmail();

    string GetUserName();

    List<string> GetRoles();

    bool IsAuthenticated();

    bool IsInRole(string role);

    void SetAccessToken(string token);

    void SetRefreshToken(string token);

    string GetAccessToken();

    string GetRefreshToken();

    void RemoveAccessToken();

    void RemoveRefreshToken();

    string GetCorrelationId();

    string? GetIdempotencyKey();

    string GetIpAddress();
}
