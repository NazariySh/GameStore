using FluentValidation;
using Gamestore.BLL.DTOs.Payments;
using Gamestore.BLL.DTOs.Payments.Visa;
using Gamestore.BLL.Integrations.Interfaces;
using Gamestore.BLL.Interfaces;
using Gamestore.BLL.Services.Payments.Processors;
using Gamestore.BLL.Tests.Extensions;
using Gamestore.BLL.Tests.Factories;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gamestore.BLL.Tests.Services.Payments.Processors;

public class VisaPaymentProcessorTests
{
    private readonly Mock<IPaymentApiClient> _mockPaymentApiClient;
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly VisaPaymentProcessor _visaPaymentProcessor;

    public VisaPaymentProcessorTests()
    {
        _mockPaymentApiClient = new Mock<IPaymentApiClient>();
        _mockValidationService = new Mock<IValidationService>();
        _visaPaymentProcessor = new VisaPaymentProcessor(
            _mockPaymentApiClient.Object,
            MapperFactory.Create(),
            _mockValidationService.Object,
            Mock.Of<ILogger<VisaPaymentProcessor>>());
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldThrowArgumentNullException_WhenPaymentRequestIsNull()
    {
        var act = () => _visaPaymentProcessor.ProcessPaymentAsync(null!);

        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldThrowArgumentException_WhenCardDetailsAreNull()
    {
        var paymentRequest = new OrderPaymentRequest
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TotalAmount = 100.00m,
            CardDetails = null,
        };

        var act = () => _visaPaymentProcessor.ProcessPaymentAsync(paymentRequest);

        await Assert.ThrowsAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldThrowValidationException_WhenCardDetailsInvalid()
    {
        var paymentRequest = new OrderPaymentRequest
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TotalAmount = 100.00m,
            CardDetails = new CardDetailsDto(),
        };

        _mockValidationService.SetupValidationThrows(paymentRequest.CardDetails);

        var act = () => _visaPaymentProcessor.ProcessPaymentAsync(paymentRequest);

        await Assert.ThrowsAsync<ValidationException>(act);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldCallApiClientAndReturnResponse_WhenRequestIsValid()
    {
        var paymentRequest = GetValidPaymentRequest();

        var response = await _visaPaymentProcessor.ProcessPaymentAsync(paymentRequest);

        Assert.IsType<VisaPaymentResponse>(response);

        _mockPaymentApiClient.Verify(
            x => x.ProcessVisaPaymentAsync(It.IsAny<VisaTransactionRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static OrderPaymentRequest GetValidPaymentRequest()
    {
        return new OrderPaymentRequest
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TotalAmount = 100.00m,
            CardDetails = new CardDetailsDto
            {
                Holder = "John Doe",
                CardNumber = "4111111111111111",
                MonthExpire = 12,
                YearExpire = 2030,
                Cvv2 = 123,
            },
        };
    }
}