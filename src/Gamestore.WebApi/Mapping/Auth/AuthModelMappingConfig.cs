using Gamestore.BLL.DTOs.Auth;
using Gamestore.WebApi.Models.Auth;
using Mapster;

namespace Gamestore.WebApi.Mapping.Auth;

public class AuthModelMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<LoginDtoModel, LoginRequest>();
    }
}