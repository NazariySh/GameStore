namespace Gamestore.BLL.DTOs.Payments.IBox;

public class BoxTransactionResponse
{
    public Guid AccountNumber { get; set; }

    public Guid InvoiceNumber { get; set; }

    public string AccountId { get; set; }
}