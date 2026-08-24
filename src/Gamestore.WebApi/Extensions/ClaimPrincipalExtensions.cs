using System.Security.Claims;

namespace Gamestore.WebApi.Extensions;

public static class ClaimPrincipalExtensions
{
    public static Guid GetId(this ClaimsPrincipal claimsPrincipal)
    {
        return GetIdOrDefault(claimsPrincipal)
               ?? throw new UnauthorizedAccessException("User Id not found or invalid.");
    }

    public static Guid? GetIdOrDefault(this ClaimsPrincipal claimsPrincipal)
    {
        var id = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(id, out var result) ? result : null;
    }

    public static string GetUserName(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.FindFirstValue(ClaimTypes.Name)
               ?? throw new UnauthorizedAccessException("User Name not found.");
    }
}