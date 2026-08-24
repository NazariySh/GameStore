using System.Security.Claims;
using System.Text;
using Gamestore.BLL.DTOs.Auth;
using Gamestore.BLL.Interfaces.Auth;
using Gamestore.Domain.Entities.Users;
using Gamestore.Domain.Settings;
using Gamestore.Domain.Shared;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Gamestore.BLL.Services.Auth;

public class JwtTokenProvider : ITokenProvider
{
    public const string Algorithm = SecurityAlgorithms.HmacSha256;

    private readonly JwtSettings _jwtSettings;

    public JwtTokenProvider(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public TokenDto GenerateToken(User user, IEnumerable<string> roles, IEnumerable<Claim> claims)
    {
        Guard.AgainstNull(user);

        return new TokenDto(GenerateAccessToken(user, roles, claims));
    }

    private string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<Claim> claims)
    {
        var tokenDescriptor = GetTokenDescriptor(GetTokenClaims(user, roles, claims));
        return new JsonWebTokenHandler().CreateToken(tokenDescriptor);
    }

    private SecurityTokenDescriptor GetTokenDescriptor(IEnumerable<Claim> claims)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));

        return new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryInMinutes),
            SigningCredentials = new SigningCredentials(securityKey, Algorithm),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
        };
    }

    private static List<Claim> GetTokenClaims(User user, IEnumerable<string> roles, IEnumerable<Claim> claims)
    {
        ArgumentException.ThrowIfNullOrEmpty(user.UserName);

        var tokenClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
        };

        tokenClaims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        tokenClaims.AddRange(claims);

        return tokenClaims;
    }
}