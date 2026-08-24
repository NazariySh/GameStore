namespace Gamestore.BLL.DTOs.Payments.PaymentMethods;

public record PaymentMethodDto
{
    public string Title { get; init; }

    public string Description { get; init; }

    public string ImageUrl { get; init; }
}