using Gamestore.BLL.Enums;

namespace Gamestore.BLL.DTOs.Users;

public record BanUserRequest
{
    public string User { get; init; }

    public BanDuration Duration { get; init; }
}