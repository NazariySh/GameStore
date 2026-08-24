using Microsoft.AspNetCore.Identity;
using MongoDB.Bson.Serialization.Attributes;

namespace Gamestore.Domain.Entities.Users;

public class RoleClaim : IdentityRoleClaim<Guid>
{
    [BsonIgnore]
    public virtual Role Role { get; set; }
}