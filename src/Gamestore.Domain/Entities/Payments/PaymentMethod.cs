namespace Gamestore.Domain.Entities.Payments;

public class PaymentMethod : BaseEntity
{
    public string Title { get; set; }

    public string Description { get; set; }

    public string ImageUrl { get; set; }
}