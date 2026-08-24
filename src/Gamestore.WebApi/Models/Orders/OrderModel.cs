namespace Gamestore.WebApi.Models.Orders;

public class OrderModel
{
    public string Id { get; set; }

    public DateTime? Date { get; set; }

    public DateTime? ShippedDate { get; set; }

    public string CustomerId { get; set; }
}