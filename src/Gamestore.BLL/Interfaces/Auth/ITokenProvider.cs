using System.Security.Claims;
using Gamestore.BLL.DTOs.Auth;
using Gamestore.Domain.Entities.Users;

namespace Gamestore.BLL.Interfaces.Auth;

public interface ITokenProvider
{
    TokenDto GenerateToken(User user, IEnumerable<string> roles, IEnumerable<Claim> claims);
}