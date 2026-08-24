using Gamestore.BLL.DTOs.Payments;

namespace Gamestore.BLL.Interfaces.Payments.Processors;

public interface IPaymentProcessor
{
    string PaymentMethod { get; }

    Task<PaymentResponse> ProcessPaymentAsync(OrderPaymentRequest paymentRequest, CancellationToken cancellationToken = default);
}