using Gamestore.BLL.DTOs.Payments.PaymentMethods;

namespace Gamestore.WebApi.Models.Payments.PaymentMethods;

public class PaymentMethodsResponseModel
{
    public IReadOnlyList<PaymentMethodDto> PaymentMethods { get; set; }
}