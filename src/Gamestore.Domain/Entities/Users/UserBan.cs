namespace Gamestore.Domain.Entities.Users;

public class UserBan : BaseEntity
{
    public string UserName { get; set; }

    public DateTime BanUntil { get; set; }
}