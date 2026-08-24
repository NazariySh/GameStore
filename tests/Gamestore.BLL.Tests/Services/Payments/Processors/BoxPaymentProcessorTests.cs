using Gamestore.BLL.DTOs.Payments;
using Gamestore.BLL.DTOs.Payments.IBox;
using Gamestore.BLL.Integrations.Interfaces;
using Gamestore.BLL.Services.Payments.Processors;
using Gamestore.BLL.Tests.Factories;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gamestore.BLL.Tests.Services.Payments.Processors;

public class BoxPaymentProcessorTests
{
    private readonly Mock<IPaymentApiClient> _mockPaymentApiClient;
    private readonly BoxPaymentProcessor _boxPaymentProcessor;

    public BoxPaymentProcessorTests()
    {
        _mockPaymentApiClient = new Mock<IPaymentApiClient>();
        _boxPaymentProcessor = new BoxPaymentProcessor(
            _mockPaymentApiClient.Object,
            MapperFactory.Create(),
            Mock.Of<ILogger<BoxPaymentProcessor>>());
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldThrowArgumentNullException_WhenPaymentRequestIsNull()
    {
        var act = () => _boxPaymentProcessor.ProcessPaymentAsync(null!);

        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldCallApiClientAndReturnResponse_WhenRequestIsValid()
    {
        var paymentRequest = GetValidPaymentRequest();

        var expectedApiResponse = new BoxTransactionResponse
        {
            AccountNumber = paymentRequest.CustomerId,
            AccountId = paymentRequest.CustomerId.ToString(),
            InvoiceNumber = paymentRequest.OrderId,
        };

        SetupMockPaymentApiClientProcessIBoxPayment(expectedApiResponse);

        var result = await _boxPaymentProcessor.ProcessPaymentAsync(paymentRequest);

        var response = Assert.IsType<BoxPaymentResponse>(result);

        Assert.NotNull(result);
        Assert.Equal(paymentRequest.CustomerId, response.UserId);
        Assert.Equal(paymentRequest.OrderId, response.OrderId);
        Assert.Equal(paymentRequest.TotalAmount, response.Sum);
    }

    private static OrderPaymentRequest GetValidPaymentRequest()
    {
        return new OrderPaymentRequest
        {
            CustomerId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            TotalAmount = 100.00m,
        };
    }

    private void SetupMockPaymentApiClientProcessIBoxPayment(BoxTransactionResponse response)
    {
        _mockPaymentApiClient
            .Setup(client => client.ProcessIBoxPaymentAsync(
                It.IsAny<BoxTransactionRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
    }
}