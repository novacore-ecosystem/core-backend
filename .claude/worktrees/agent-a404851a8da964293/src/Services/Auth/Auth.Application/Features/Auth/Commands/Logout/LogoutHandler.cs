using NovaCore.Auth.Application.Abstractions.Services;

using NovaCore.BuildingBlock.Application.Abstractions.Services;

namespace NovaCore.Auth.Application.Features.Auth.Commands.Logout;

public sealed class LogoutHandler(
    ICurrentUserService currentUserService,
    IRefreshTokenService refreshTokenService) : ICommandHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken ct = default)
    {
        var refreshToken = currentUserService.GetRefreshToken();
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await refreshTokenService.RevokeRefreshTokenByTokenStringAsync(refreshToken, ct);
        }

        currentUserService.RemoveAccessToken();
        currentUserService.RemoveRefreshToken();
    }
}
