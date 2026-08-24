using System.Security.Claims;
using Gamestore.DAL.Services.Interfaces;
using Gamestore.Domain.Constants;
using Gamestore.Domain.Enums;

namespace Gamestore.WebApi.Services.Auth;

public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(RoleType role)
    {
        return User?.IsInRole(role.ToString()) ?? false;
    }

    public bool HasPermission(string permission)
    {
        return User?.HasClaim(CustomClaimTypes.Permission, permission) ?? false;
    }
}