namespace Gamestore.BLL.DTOs.Orders;

public record OrderDto
{
    public Guid Id { get; init; }

    public DateTime? Date { get; init; }

    public DateTime? ShippedDate { get; init; }

    public Guid CustomerId { get; init; }

    public int? MongoOrderId { get; init; }

    public string? MongoCustomerId { get; init; }
}