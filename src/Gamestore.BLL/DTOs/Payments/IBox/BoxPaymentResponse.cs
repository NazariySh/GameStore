namespace Gamestore.BLL.DTOs.Payments.IBox;

public class BoxPaymentResponse : PaymentResponse
{
    public Guid UserId { get; set; }

    public Guid OrderId { get; set; }

    public DateTime PaymentDate { get; set; }

    public decimal Sum { get; set; }
}