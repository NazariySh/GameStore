namespace Gamestore.BLL.DTOs.Auth;

public record CheckAccessRequest
{
    public string TargetPage { get; init; }

    public string? TargetId { get; init; }
}