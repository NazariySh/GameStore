using System.Security.Claims;
using Gamestore.BLL.DTOs.Auth;

namespace Gamestore.BLL.Interfaces.Auth;

public interface IAccessService
{
    Task<bool> CheckUserAccessAsync(ClaimsPrincipal user, CheckAccessRequest request, CancellationToken cancellationToken = default);
}