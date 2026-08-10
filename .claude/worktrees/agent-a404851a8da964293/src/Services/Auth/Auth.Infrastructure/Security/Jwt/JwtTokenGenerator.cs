using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using NovaCore.Auth.Application.Abstractions.Security.Jwt;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Security;

using Microsoft.IdentityModel.Tokens;

namespace NovaCore.Auth.Infrastructure.Security.Jwt;

public sealed class JwtTokenGenerator(JwtSettings settings) : IJwtTokenGenerator
{
    private readonly JwtSettings _settings = settings;
    private readonly SymmetricSecurityKey _key = new(Encoding.UTF8.GetBytes(settings.SecretKey));

    public string GenerateAccessToken(
        Guid userId,
        string email,
        string username,
        IEnumerable<string> roles,
        IEnumerable<string> permissions,
        Guid? jwtId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, username),
        };

        if (jwtId.HasValue)
            claims.Add(new Claim("jti", jwtId.Value.ToString()));

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(permissions.Select(permission => new Claim(AppClaimTypes.Permission, permission)));

        var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,
                ValidateIssuer = true,
                ValidIssuer = _settings.Issuer,
                ValidateAudience = true,
                ValidAudience = _settings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
