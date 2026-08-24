using Gamestore.BLL.DTOs.Payments;
using Gamestore.BLL.DTOs.Payments.Bank;
using Gamestore.BLL.Interfaces.Payments;
using Gamestore.BLL.Interfaces.Payments.Processors;
using Gamestore.Domain.Exceptions;
using Gamestore.Domain.Settings;
using Gamestore.Domain.Shared;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gamestore.BLL.Services.Payments.Processors;

public class BankPaymentProcessor : IPaymentProcessor
{
    private readonly IPaymentInvoiceGenerator _invoiceGenerator;
    private readonly IMapper _mapper;
    private readonly BankPaymentSettings _bankPaymentSettings;
    private readonly ILogger<BankPaymentProcessor> _logger;

    public BankPaymentProcessor(
        IPaymentInvoiceGenerator invoiceGenerator,
        IMapper mapper,
        IOptions<BankPaymentSettings> bankPaymentSettings,
        ILogger<BankPaymentProcessor> logger)
    {
        _invoiceGenerator = invoiceGenerator;
        _mapper = mapper;
        _bankPaymentSettings = bankPaymentSettings.Value;
        _logger = logger;
    }

    public string PaymentMethod => "Bank";

    public Task<PaymentResponse> ProcessPaymentAsync(OrderPaymentRequest paymentRequest, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(paymentRequest);

        _logger.LogInformation("Processing bank payment for account '{CustomerId}' with order number '{OrderId}'", paymentRequest.CustomerId, paymentRequest.OrderId);

        var paymentInvoice = _mapper.Map<PaymentInvoiceDto>(paymentRequest);
        paymentInvoice.CreatedAt = DateTime.UtcNow;
        paymentInvoice.ValidUntil = paymentInvoice.CreatedAt.AddDays(_bankPaymentSettings.ValidityPeriodInDays);

        try
        {
            var invoiceFile = _invoiceGenerator.Generate(paymentInvoice);

            var response = new BankPaymentResponse
            {
                InvoiceFile = invoiceFile,
            };

            _logger.LogInformation("Bank payment processed successfully for order with number '{OrderId}'", paymentRequest.OrderId);

            return Task.FromResult<PaymentResponse>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate invoice for bank payment for order with number '{OrderId}'", paymentRequest.OrderId);
            throw new PaymentProcessingException($"Failed to generate invoice for bank payment. {ex.Message}", ex);
        }
    }
}