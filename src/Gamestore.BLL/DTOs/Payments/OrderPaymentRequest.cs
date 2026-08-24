namespace Gamestore.BLL.DTOs.Payments;

public record OrderPaymentRequest
{
    public Guid CustomerId { get; init; }

    public Guid OrderId { get; init; }

    public decimal TotalAmount { get; init; }

    public CardDetailsDto? CardDetails { get; init; }
}