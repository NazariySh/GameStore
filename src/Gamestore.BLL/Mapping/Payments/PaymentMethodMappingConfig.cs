using Gamestore.BLL.DTOs.Payments.PaymentMethods;
using Gamestore.Domain.Entities.Payments;
using Mapster;

namespace Gamestore.BLL.Mapping.Payments;

public class PaymentMethodMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PaymentMethod, PaymentMethodDto>();
    }
}