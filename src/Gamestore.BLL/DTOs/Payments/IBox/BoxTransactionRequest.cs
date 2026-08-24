namespace Gamestore.BLL.DTOs.Payments.IBox;

public record BoxTransactionRequest
{
    public decimal TransactionAmount { get; init; }

    public Guid AccountNumber { get; init; }

    public Guid InvoiceNumber { get; init; }
}