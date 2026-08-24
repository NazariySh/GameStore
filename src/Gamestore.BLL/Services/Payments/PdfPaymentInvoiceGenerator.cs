using Gamestore.BLL.DTOs;
using Gamestore.BLL.DTOs.Payments.Bank;
using Gamestore.BLL.Interfaces.Payments;
using Gamestore.Domain.Shared;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Gamestore.BLL.Services.Payments;

public class PdfPaymentInvoiceGenerator : IPaymentInvoiceGenerator
{
    private const string ContentType = "application/pdf";
    private const string FileExtension = ".pdf";

    private readonly ILogger<PdfPaymentInvoiceGenerator> _logger;

    public PdfPaymentInvoiceGenerator(ILogger<PdfPaymentInvoiceGenerator> logger)
    {
        _logger = logger;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public DownloadFileContentDto Generate(PaymentInvoiceDto paymentInvoice)
    {
        Guard.AgainstNull(paymentInvoice);

        _logger.LogInformation("Generating PDF invoice for account '{UserId}' with order number '{OrderId}'", paymentInvoice.UserId, paymentInvoice.OrderId);

        var pdfFileContent = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

                page.Content().Column(column =>
                {
                    column.Spacing(15);

                    column.Item().Text("Invoice").FontSize(24).Bold().AlignCenter();
                    column.Item().Text($"User ID: {paymentInvoice.UserId}");
                    column.Item().Text($"Order ID: {paymentInvoice.OrderId}");
                    column.Item().Text($"Creation Date: {paymentInvoice.CreatedAt:yyyy-MM-dd HH:mm}");
                    column.Item().Text($"Valid Until Date: {paymentInvoice.ValidUntil:yyyy-MM-dd}");
                    column.Item().Text($"Total Sum: {paymentInvoice.Sum}");
                });
            });
        }).GeneratePdf();

        var fileName = GetFileName(paymentInvoice.OrderId);

        _logger.LogInformation("PDF invoice generated successfully for order '{OrderId}' with file name '{FileName}'", paymentInvoice.OrderId, fileName);

        return new DownloadFileContentDto
        {
            FileName = fileName,
            ContentType = ContentType,
            Content = pdfFileContent,
        };
    }

    private static string GetFileName(Guid orderId)
    {
        return $"invoice_{orderId}{FileExtension}";
    }
}