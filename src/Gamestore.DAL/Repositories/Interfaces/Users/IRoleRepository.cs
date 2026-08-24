using System.Security.Claims;
using Gamestore.Domain.Entities.Users;

namespace Gamestore.DAL.Repositories.Interfaces.Users;

public interface IRoleRepository : IRepository<Role>
{
    Task<IReadOnlyList<string>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Claim>> GetRoleClaimsAsync(string role, CancellationToken cancellationToken = default);
}