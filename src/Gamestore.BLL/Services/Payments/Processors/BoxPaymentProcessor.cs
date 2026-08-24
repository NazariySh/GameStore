using Gamestore.BLL.DTOs.Payments;
using Gamestore.BLL.DTOs.Payments.IBox;
using Gamestore.BLL.Integrations.Interfaces;
using Gamestore.BLL.Interfaces.Payments.Processors;
using Gamestore.Domain.Shared;
using MapsterMapper;
using Microsoft.Extensions.Logging;

namespace Gamestore.BLL.Services.Payments.Processors;

public class BoxPaymentProcessor : IPaymentProcessor
{
    private readonly IPaymentApiClient _paymentApiClient;
    private readonly IMapper _mapper;
    private readonly ILogger<BoxPaymentProcessor> _logger;

    public BoxPaymentProcessor(
        IPaymentApiClient paymentApiClient,
        IMapper mapper,
        ILogger<BoxPaymentProcessor> logger)
    {
        _paymentApiClient = paymentApiClient;
        _mapper = mapper;
        _logger = logger;
    }

    public string PaymentMethod => "IBox terminal";

    public async Task<PaymentResponse> ProcessPaymentAsync(OrderPaymentRequest paymentRequest, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(paymentRequest);

        _logger.LogInformation("Processing IBox payment for account '{CustomerId}' with order number '{OrderId}'", paymentRequest.CustomerId, paymentRequest.OrderId);

        var iboxPaymentRequest = _mapper.Map<BoxTransactionRequest>(paymentRequest);

        var response = await _paymentApiClient.ProcessIBoxPaymentAsync(iboxPaymentRequest, cancellationToken);

        var result = _mapper.Map<BoxPaymentResponse>(response);
        result.Sum = paymentRequest.TotalAmount;

        _logger.LogInformation("IBox payment processed successfully for order with number '{OrderId}'", paymentRequest.OrderId);

        return result;
    }
}