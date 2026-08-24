using Gamestore.BLL.DTOs.Games.Platforms;
using Gamestore.Domain.Entities.Games;
using Mapster;

namespace Gamestore.BLL.Mapping.Games;

public class PlatformMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Platform, Platform>()
            .Ignore(dest => dest.GamePlatforms);

        config.NewConfig<Platform, PlatformDto>();

        config.NewConfig<PlatformCreateDto, Platform>();
        config.NewConfig<PlatformUpdateDto, Platform>();
    }
}