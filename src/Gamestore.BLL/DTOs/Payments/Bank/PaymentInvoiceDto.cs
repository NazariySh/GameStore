namespace Gamestore.BLL.DTOs.Payments.Bank;

public class PaymentInvoiceDto
{
    public Guid UserId { get; set; }

    public Guid OrderId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ValidUntil { get; set; }

    public decimal Sum { get; set; }
}