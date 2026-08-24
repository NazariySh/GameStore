using System.Net;
using Gamestore.BLL.DTOs.Payments.IBox;
using Gamestore.BLL.DTOs.Payments.Visa;
using Gamestore.BLL.Integrations.Implementations;
using Gamestore.BLL.Tests.Extensions;
using Gamestore.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gamestore.BLL.Tests.Integrations;

public class PaymentApiClientTests
{
    private const string BaseUrl = "https://api.example.com/";

    private readonly Mock<HttpMessageHandler> _mockHandler;
    private readonly PaymentApiClient _paymentApiClient;

    public PaymentApiClientTests()
    {
        _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(_mockHandler.Object)
        {
            BaseAddress = new Uri(BaseUrl),
        };
        _paymentApiClient = new PaymentApiClient(
            httpClient,
            Mock.Of<ILogger<PaymentApiClient>>());
    }

    [Fact]
    public async Task ProcessIBoxPaymentAsync_ShouldThrowArgumentNullException_WhenRequestIsNull()
    {
        var act = () => _paymentApiClient.ProcessIBoxPaymentAsync(null!);

        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task ProcessIBoxPaymentAsync_ShouldThrowPaymentProcessingException_WhenResponseIsNotSuccessful()
    {
        var request = GetBoxTransactionRequest();

        var problemDetails = new ProblemDetails
        {
            Title = "Payment processing error",
            Detail = "An error occurred while processing the payment.",
        };

        var statusCode = HttpStatusCode.BadRequest;

        _mockHandler.SetupSendPostFailed($"{BaseUrl}payments/ibox", statusCode, problemDetails);

        var act = () => _paymentApiClient.ProcessIBoxPaymentAsync(request);

        var exception = await Assert.ThrowsAsync<PaymentProcessingException>(act);

        Assert.Equal((int)statusCode, exception.StatusCode);
        Assert.Equal(problemDetails.Title, exception.Title);
        Assert.Equal(problemDetails.Detail, exception.Detail);
    }

    [Fact]
    public async Task ProcessIBoxPaymentAsync_ShouldReturnSuccessResponse_WhenRequestIsValid()
    {
        var request = GetBoxTransactionRequest();

        var expectedResponse = new BoxTransactionResponse
        {
            AccountNumber = request.AccountNumber,
            InvoiceNumber = request.InvoiceNumber,
            AccountId = request.AccountNumber.ToString(),
        };

        _mockHandler.SetupSendPost($"{BaseUrl}payments/ibox", expectedResponse);

        var result = await _paymentApiClient.ProcessIBoxPaymentAsync(request);

        Assert.NotNull(result);
        Assert.Equivalent(expectedResponse, result);
    }

    [Fact]
    public async Task ProcessVisaPaymentAsync_ShouldThrowArgumentNullException_WhenRequestIsNull()
    {
        var act = () => _paymentApiClient.ProcessVisaPaymentAsync(null!);

        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task ProcessVisaPaymentAsync_ShouldThrowPaymentProcessingException_WhenResponseIsNotSuccessful()
    {
        var request = GetVisaTransactionRequest();

        var problemDetails = new ProblemDetails
        {
            Title = "Payment processing error",
            Detail = "An error occurred while processing the payment.",
        };

        var statusCode = HttpStatusCode.BadRequest;
        _mockHandler.SetupSendPostFailed($"{BaseUrl}visa", statusCode, problemDetails);

        _mockHandler.SetupSendPostFailed($"{BaseUrl}payments/visa", statusCode, problemDetails);

        var act = () => _paymentApiClient.ProcessVisaPaymentAsync(request);

        var exception = await Assert.ThrowsAsync<PaymentProcessingException>(act);

        Assert.Equal((int)statusCode, exception.StatusCode);
        Assert.Equal(problemDetails.Title, exception.Title);
        Assert.Equal(problemDetails.Detail, exception.Detail);
    }

    [Fact]
    public async Task ProcessVisaPaymentAsync_ShouldReturnSuccessResponse_WhenRequestIsValid()
    {
        var request = GetVisaTransactionRequest();

        _mockHandler.SetupSendPost($"{BaseUrl}payments/visa", string.Empty);

        await _paymentApiClient.ProcessVisaPaymentAsync(request);

        _mockHandler.VerifyCalledOnce($"{BaseUrl}payments/visa");
    }

    private static BoxTransactionRequest GetBoxTransactionRequest()
    {
        return new BoxTransactionRequest
        {
            AccountNumber = Guid.NewGuid(),
            InvoiceNumber = Guid.NewGuid(),
            TransactionAmount = 100.00m,
        };
    }

    private static VisaTransactionRequest GetVisaTransactionRequest()
    {
        return new VisaTransactionRequest
        {
            CardHolderName = "John Doe",
            CardNumber = "4111111111111111",
            ExpirationMonth = 12,
            ExpirationYear = 2025,
            Cvv = 123,
            TransactionAmount = 100.00m,
        };
    }
}