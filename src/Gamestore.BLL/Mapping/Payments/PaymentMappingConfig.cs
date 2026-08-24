using Gamestore.BLL.DTOs.Payments;
using Gamestore.BLL.DTOs.Payments.Bank;
using Gamestore.BLL.DTOs.Payments.IBox;
using Gamestore.BLL.DTOs.Payments.Visa;
using Mapster;

namespace Gamestore.BLL.Mapping.Payments;

public class PaymentMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<BoxTransactionResponse, BoxPaymentResponse>()
            .Map(dest => dest.UserId, src => src.AccountNumber)
            .Map(dest => dest.OrderId, src => src.InvoiceNumber)
            .Map(dest => dest.PaymentDate, src => DateTime.UtcNow);

        config.NewConfig<OrderPaymentRequest, BoxTransactionRequest>()
            .Map(dest => dest.TransactionAmount, src => src.TotalAmount)
            .Map(dest => dest.AccountNumber, src => src.CustomerId)
            .Map(dest => dest.InvoiceNumber, src => src.OrderId);

        config.NewConfig<OrderPaymentRequest, VisaTransactionRequest>()
            .Map(dest => dest.TransactionAmount, src => src.TotalAmount)
            .Map(dest => dest.CardHolderName, src => src.CardDetails.Holder)
            .Map(dest => dest.CardNumber, src => src.CardDetails.CardNumber)
            .Map(dest => dest.ExpirationMonth, src => src.CardDetails.MonthExpire)
            .Map(dest => dest.ExpirationYear, src => src.CardDetails.YearExpire)
            .Map(dest => dest.Cvv, src => src.CardDetails.Cvv2);

        config.NewConfig<OrderPaymentRequest, PaymentInvoiceDto>()
            .Map(dest => dest.UserId, src => src.CustomerId)
            .Map(dest => dest.OrderId, src => src.OrderId)
            .Map(dest => dest.CreatedAt, src => DateTime.UtcNow)
            .Map(dest => dest.Sum, src => src.TotalAmount);
    }
}