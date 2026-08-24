using System.Security.Claims;
using Gamestore.DAL.Data;
using Gamestore.DAL.Repositories.Interfaces.Users;
using Gamestore.Domain.Constants;
using Gamestore.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.DAL.Repositories.Implementations.SqlRepositories.Users;

public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(GamestoreDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<string>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.RoleClaims
            .Where(rc => rc.ClaimType == CustomClaimTypes.Permission)
            .Select(rc => rc.ClaimValue!)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await DbContext.RoleClaims
            .Where(rc => rc.RoleId == roleId && rc.ClaimType == CustomClaimTypes.Permission)
            .Select(rc => rc.ClaimValue!)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Claim>> GetRoleClaimsAsync(string role, CancellationToken cancellationToken = default)
    {
        var normalizedRoleName = role.ToUpperInvariant();
        return await DbContext.RoleClaims
            .Where(rc => rc.Role.NormalizedName == normalizedRoleName)
            .Select(rc => new Claim(rc.ClaimType!, rc.ClaimValue!))
            .ToListAsync(cancellationToken);
    }
}