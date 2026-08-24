namespace Gamestore.BLL.DTOs.Payments.Bank;

public class BankPaymentResponse : PaymentResponse
{
    public DownloadFileContentDto InvoiceFile { get; set; }
}