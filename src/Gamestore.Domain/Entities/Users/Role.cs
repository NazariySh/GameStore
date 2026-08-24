using Microsoft.AspNetCore.Identity;

namespace Gamestore.Domain.Entities.Users;

public class Role : IdentityRole<Guid>, IBaseEntity
{
    public virtual ICollection<UserRole> UserRoles { get; set; } = [];

    public virtual ICollection<RoleClaim> RoleClaims { get; set; } = [];
}