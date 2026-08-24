using Gamestore.BLL.Factories.Interfaces;
using Gamestore.BLL.Interfaces.Payments.Processors;

namespace Gamestore.BLL.Factories.Implementations;

public class PaymentProcessorFactory : IPaymentProcessorFactory
{
    private readonly Dictionary<string, IPaymentProcessor> _paymentProcessors;

    public PaymentProcessorFactory(IEnumerable<IPaymentProcessor> paymentProcessors)
    {
        _paymentProcessors = paymentProcessors.ToDictionary(
            p => p.PaymentMethod,
            StringComparer.OrdinalIgnoreCase);
    }

    public IPaymentProcessor Create(string method)
    {
        return _paymentProcessors.TryGetValue(method, out var processor)
            ? processor
            : throw new ArgumentException($"Unsupported payment method: '{method}'", nameof(method));
    }
}