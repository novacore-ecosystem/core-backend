using NovaCore.BuildingBlock.Application.Abstractions.Services;

namespace NovaCore.TestKit.Fakes;

/// <summary>
/// In-memory <see cref="ICurrentUserService"/> for handler tests that need a caller identity
/// without standing up HttpContext/JWT. Defaults to an authenticated user with no roles;
/// override via the settable properties before invoking the handler under test.
/// </summary>
public sealed class FakeCurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; set; } = Guid.CreateVersion7();
    public string UserEmail { get; set; } = "test-user@novacore.local";
    public string UserName { get; set; } = "test-user";
    public List<string> Roles { get; set; } = [];
    public bool Authenticated { get; set; } = true;
    public string? CorrelationId { get; set; } = Guid.CreateVersion7().ToString();
    public string? IdempotencyKey { get; set; } = Guid.CreateVersion7().ToString();
    public string IpAddress { get; set; } = "127.0.0.1";

    private string _accessToken = string.Empty;
    private string _refreshToken = string.Empty;

    public Guid? GetUserId() => UserId;
    public string GetUserEmail() => UserEmail;
    public string GetUserName() => UserName;
    public List<string> GetRoles() => Roles;
    public bool IsAuthenticated() => Authenticated;
    public bool IsInRole(string role) => Roles.Contains(role);
    public void SetAccessToken(string token) => _accessToken = token;
    public void SetRefreshToken(string token) => _refreshToken = token;
    public string GetAccessToken() => _accessToken;
    public string GetRefreshToken() => _refreshToken;
    public void RemoveAccessToken() => _accessToken = string.Empty;
    public void RemoveRefreshToken() => _refreshToken = string.Empty;
    public string? GetCorrelationId() => CorrelationId;
    public string? GetIdempotencyKey() => IdempotencyKey;
    public string GetIpAddress() => IpAddress;
}
