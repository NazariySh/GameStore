using Gamestore.BLL.DTOs.Payments;
using Gamestore.BLL.DTOs.Payments.PaymentMethods;

namespace Gamestore.BLL.Interfaces.Payments;

public interface IPaymentService
{
    Task<IReadOnlyList<PaymentMethodDto>> GetPaymentMethodsAsync(CancellationToken cancellationToken = default);

    Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request, Guid customerId, CancellationToken cancellationToken = default);
}