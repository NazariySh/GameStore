using Gamestore.BLL.DTOs.Users;
using Gamestore.BLL.Enums;
using Gamestore.WebApi.Models.Users;
using Mapster;

namespace Gamestore.WebApi.Mapping.Users;

public class BanUserModelMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<BanUserRequestModel, BanUserRequest>()
            .Map(dest => dest.Duration, src => BanDuration.FromDisplayName(src.Duration));
    }
}