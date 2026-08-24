using Gamestore.BLL.DTOs;
using Gamestore.BLL.DTOs.Payments;
using Gamestore.BLL.DTOs.Payments.Bank;
using Gamestore.BLL.Interfaces.Payments;
using Gamestore.BLL.Services.Payments.Processors;
using Gamestore.BLL.Tests.Factories;
using Gamestore.Domain.Exceptions;
using Gamestore.Domain.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Gamestore.BLL.Tests.Services.Payments.Processors;

public class BankPaymentProcessorTests
{
    private readonly Mock<IPaymentInvoiceGenerator> _mockInvoiceGenerator;
    private readonly BankPaymentProcessor _bankPaymentProcessor;

    public BankPaymentProcessorTests()
    {
        _mockInvoiceGenerator = new Mock<IPaymentInvoiceGenerator>();
        var bankPaymentSettings = Options.Create(new BankPaymentSettings
        {
            ValidityPeriodInDays = 30,
        });
        _bankPaymentProcessor = new BankPaymentProcessor(
            _mockInvoiceGenerator.Object,
            MapperFactory.Create(),
            bankPaymentSettings,
            Mock.Of<ILogger<BankPaymentProcessor>>());
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldThrowArgumentNullException_WhenPaymentRequestIsNull()
    {
        var act = () => _bankPaymentProcessor.ProcessPaymentAsync(null!);

        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldThrowPaymentProcessingException_WhenInvoiceGenerationFails()
    {
        var paymentRequest = GetValidPaymentRequest();

        SetupMockPaymentInvoiceGeneratorGenerateFails();

        var act = () => _bankPaymentProcessor.ProcessPaymentAsync(paymentRequest);

        await Assert.ThrowsAsync<PaymentProcessingException>(act);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldReturnGeneratedInvoiceFile_WhenRequestIsValid()
    {
        var paymentRequest = GetValidPaymentRequest();

        var fileContent = new DownloadFileContentDto
        {
            FileName = "invoice.pdf",
            ContentType = "application/pdf",
            Content = [1, 2, 3, 4, 5],
        };

        SetupMockPaymentInvoiceGeneratorGenerate(fileContent);

        var result = await _bankPaymentProcessor.ProcessPaymentAsync(paymentRequest);

        var response = Assert.IsType<BankPaymentResponse>(result);

        Assert.NotNull(result);
        Assert.Equal(fileContent, response.InvoiceFile);
    }

    private void SetupMockPaymentInvoiceGeneratorGenerate(DownloadFileContentDto fileContent)
    {
        _mockInvoiceGenerator
            .Setup(g => g.Generate(It.IsAny<PaymentInvoiceDto>()))
            .Returns(fileContent);
    }

    private void SetupMockPaymentInvoiceGeneratorGenerateFails()
    {
        _mockInvoiceGenerator
            .Setup(g => g.Generate(It.IsAny<PaymentInvoiceDto>()))
            .Throws(new InvalidOperationException("Invoice generation failed"));
    }

    private static OrderPaymentRequest GetValidPaymentRequest()
    {
        return new OrderPaymentRequest
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TotalAmount = 100.00m,
        };
    }
}