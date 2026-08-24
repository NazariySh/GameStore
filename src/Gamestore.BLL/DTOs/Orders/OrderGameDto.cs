namespace Gamestore.BLL.DTOs.Orders;

public record OrderGameDto
{
    public Guid Id { get; init; }

    public Guid ProductId { get; init; }

    public decimal Price { get; init; }

    public int Quantity { get; init; }

    public int? Discount { get; init; }

    public int? MongoProductId { get; init; }
}