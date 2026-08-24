using Microsoft.AspNetCore.Identity;

namespace Gamestore.Domain.Entities.Users;

public class User : IdentityUser<Guid>, IBaseEntity
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = [];
}