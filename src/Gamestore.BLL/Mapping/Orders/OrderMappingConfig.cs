using Gamestore.BLL.DTOs.Orders;
using Gamestore.Domain.Entities.Orders;
using Mapster;

namespace Gamestore.BLL.Mapping.Orders;

public class OrderMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Order, Order>()
            .Ignore(dest => dest.OrderGames);

        config.NewConfig<OrderGame, OrderGame>()
            .Ignore(dest => dest.Order);

        config.NewConfig<Order, OrderDto>();

        config.NewConfig<OrderGame, OrderGameDto>();
    }
}