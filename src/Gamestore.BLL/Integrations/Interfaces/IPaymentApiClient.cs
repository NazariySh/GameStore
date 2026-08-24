using Gamestore.BLL.DTOs.Payments.IBox;
using Gamestore.BLL.DTOs.Payments.Visa;

namespace Gamestore.BLL.Integrations.Interfaces;

public interface IPaymentApiClient
{
    Task<BoxTransactionResponse> ProcessIBoxPaymentAsync(BoxTransactionRequest iboxRequest, CancellationToken cancellationToken = default);

    Task ProcessVisaPaymentAsync(VisaTransactionRequest visaRequest, CancellationToken cancellationToken = default);
}