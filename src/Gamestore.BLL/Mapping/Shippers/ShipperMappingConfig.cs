using Gamestore.BLL.DTOs.Shippers;
using Gamestore.Domain.Entities.Shippers;
using Mapster;

namespace Gamestore.BLL.Mapping.Shippers;

public class ShipperMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Shipper, ShipperDto>();
    }
}