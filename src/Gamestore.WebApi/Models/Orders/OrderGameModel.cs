namespace Gamestore.WebApi.Models.Orders;

public class OrderGameModel
{
    public Guid Id { get; set; }

    public string ProductId { get; set; }

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public int? Discount { get; set; }
}