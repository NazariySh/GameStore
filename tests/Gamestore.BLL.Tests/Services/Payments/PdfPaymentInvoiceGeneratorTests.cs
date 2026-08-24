using Gamestore.BLL.DTOs.Payments.Bank;
using Gamestore.BLL.Services.Payments;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gamestore.BLL.Tests.Services.Payments;

public class PdfPaymentInvoiceGeneratorTests
{
    private const string ContentType = "application/pdf";
    private const string FileExtension = ".pdf";

    private readonly PdfPaymentInvoiceGenerator _invoiceGenerator;

    public PdfPaymentInvoiceGeneratorTests()
    {
        _invoiceGenerator = new PdfPaymentInvoiceGenerator(Mock.Of<ILogger<PdfPaymentInvoiceGenerator>>());
    }

    [Fact]
    public void Generate_ShouldThrowArgumentNullException_WhenPaymentInvoiceIsNull()
    {
        var act = () => _invoiceGenerator.Generate(null!);

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Generate_ShouldReturnCorrectFileContent_WhenPaymentInvoiceIsValid()
    {
        var paymentInvoice = new PaymentInvoiceDto
        {
            UserId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            ValidUntil = DateTime.UtcNow.AddDays(30),
            Sum = 100.00m,
        };

        var result = _invoiceGenerator.Generate(paymentInvoice);

        Assert.NotNull(result);
        Assert.Equal($"invoice_{paymentInvoice.OrderId}{FileExtension}", result.FileName);
        Assert.Equal(ContentType, result.ContentType);
        Assert.NotEmpty(result.Content);
    }
}