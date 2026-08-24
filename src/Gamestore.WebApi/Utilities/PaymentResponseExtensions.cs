using Gamestore.BLL.DTOs.Payments;
using Gamestore.BLL.DTOs.Payments.Bank;
using Gamestore.BLL.DTOs.Payments.IBox;
using Gamestore.BLL.DTOs.Payments.Visa;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Utilities;

public static class PaymentResponseExtensions
{
    public static IActionResult MapToActionResult(this PaymentResponse response)
    {
        return response switch
        {
            VisaPaymentResponse => new OkResult(),
            BoxPaymentResponse ibox => new OkObjectResult(ibox),
            BankPaymentResponse bank => new FileContentResult(
                bank.InvoiceFile.Content,
                bank.InvoiceFile.ContentType)
            {
                FileDownloadName = bank.InvoiceFile.FileName,
            },
            _ => new OkResult(),
        };
    }
}