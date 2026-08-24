namespace Gamestore.BLL.DTOs.Payments.Visa;

public record VisaTransactionRequest
{
    public decimal TransactionAmount { get; init; }

    public string CardHolderName { get; init; }

    public string CardNumber { get; init; }

    public int ExpirationMonth { get; init; }

    public int Cvv { get; init; }

    public int ExpirationYear { get; init; }
}