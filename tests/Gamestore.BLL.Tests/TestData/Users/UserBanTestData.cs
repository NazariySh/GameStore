using Gamestore.BLL.DTOs.Users;
using Gamestore.BLL.Enums;
using Gamestore.Domain.Entities.Users;

namespace Gamestore.BLL.Tests.TestData.Users;

public static class UserBanTestData
{
    public const string UserName = "John Doe";

    public static UserBan GetUserBan()
    {
        return new UserBan
        {
            Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d481"),
            UserName = UserName,
            BanUntil = DateTime.UtcNow.AddDays(90),
        };
    }

    public static BanUserRequest GetBanUserRequest(string? userName = null, BanDuration? duration = null)
    {
        return new BanUserRequest
        {
            User = userName ?? UserName,
            Duration = duration ?? BanDuration.OneWeek,
        };
    }

    public static BanUserRequest GetInvalidBanUserRequest()
    {
        return new BanUserRequest
        {
            User = string.Empty,
            Duration = BanDuration.OneWeek,
        };
    }
}