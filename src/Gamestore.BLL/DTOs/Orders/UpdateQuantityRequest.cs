namespace Gamestore.BLL.DTOs.Orders;

public record UpdateQuantityRequest
{
    public int Count { get; init; }
}