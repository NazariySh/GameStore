using System.Security.Claims;
using Gamestore.Domain.Enums;

namespace Gamestore.DAL.Services.Interfaces;

public interface IUserContext
{
    ClaimsPrincipal? User { get; }

    bool IsAuthenticated { get; }

    bool IsInRole(RoleType role);

    bool HasPermission(string permission);
}