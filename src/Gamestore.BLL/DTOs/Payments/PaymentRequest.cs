namespace Gamestore.BLL.DTOs.Payments;

public record PaymentRequest
{
    public string Method { get; init; }

    public CardDetailsDto? Model { get; init; }
}