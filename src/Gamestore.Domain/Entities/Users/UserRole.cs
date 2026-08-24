using Microsoft.AspNetCore.Identity;
using MongoDB.Bson.Serialization.Attributes;

namespace Gamestore.Domain.Entities.Users;

public class UserRole : IdentityUserRole<Guid>
{
    [BsonIgnore]
    public virtual User User { get; set; }

    [BsonIgnore]
    public virtual Role Role { get; set; }
}