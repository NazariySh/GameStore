using System.Diagnostics.CodeAnalysis;
using Gamestore.BLL.DTOs.Payments;
using Gamestore.BLL.DTOs.Payments.Visa;
using Gamestore.BLL.Integrations.Interfaces;
using Gamestore.BLL.Interfaces;
using Gamestore.BLL.Interfaces.Payments.Processors;
using Gamestore.Domain.Shared;
using MapsterMapper;
using Microsoft.Extensions.Logging;

namespace Gamestore.BLL.Services.Payments.Processors;

public class VisaPaymentProcessor : IPaymentProcessor
{
    private readonly IPaymentApiClient _paymentApiClient;
    private readonly IMapper _mapper;
    private readonly IValidationService _validationService;
    private readonly ILogger<VisaPaymentProcessor> _logger;

    public VisaPaymentProcessor(
        IPaymentApiClient paymentApiClient,
        IMapper mapper,
        IValidationService validationService,
        ILogger<VisaPaymentProcessor> logger)
    {
        _paymentApiClient = paymentApiClient;
        _mapper = mapper;
        _validationService = validationService;
        _logger = logger;
    }

    public string PaymentMethod => "Visa";

    public async Task<PaymentResponse> ProcessPaymentAsync(OrderPaymentRequest paymentRequest, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(paymentRequest);

        _logger.LogInformation("Processing Visa payment for account '{CustomerId}' with order number '{OrderId}'", paymentRequest.CustomerId, paymentRequest.OrderId);

        EnsureCardDetailsNotNull(paymentRequest.CardDetails);

        await _validationService.ValidateAndThrowAsync(paymentRequest.CardDetails, cancellationToken);

        var visaPaymentRequest = _mapper.Map<VisaTransactionRequest>(paymentRequest);

        await _paymentApiClient.ProcessVisaPaymentAsync(visaPaymentRequest, cancellationToken);

        _logger.LogInformation("Visa payment processed successfully for order with number '{OrderId}'", paymentRequest.OrderId);

        return new VisaPaymentResponse();
    }

    private void EnsureCardDetailsNotNull([NotNull] CardDetailsDto? cardDetails)
    {
        if (cardDetails is null)
        {
            _logger.LogError("Payment card details are required but were not provided");
            throw new ArgumentException("Payment card details are required.");
        }
    }
}