using Gamestore.BLL.Interfaces.Payments.Processors;

namespace Gamestore.BLL.Factories.Interfaces;

public interface IPaymentProcessorFactory
{
    IPaymentProcessor Create(string method);
}