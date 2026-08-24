using Gamestore.BLL.DTOs;
using Gamestore.BLL.DTOs.Payments.Bank;

namespace Gamestore.BLL.Interfaces.Payments;

public interface IPaymentInvoiceGenerator
{
    DownloadFileContentDto Generate(PaymentInvoiceDto paymentInvoice);
}