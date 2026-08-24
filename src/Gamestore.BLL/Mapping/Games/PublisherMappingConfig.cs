using Gamestore.BLL.DTOs.Games.Publishers;
using Gamestore.Domain.Entities.Games;
using Mapster;

namespace Gamestore.BLL.Mapping.Games;

public class PublisherMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Publisher, Publisher>()
            .Ignore(dest => dest.Games);

        config.NewConfig<Publisher, PublisherDto>();

        config.NewConfig<PublisherCreateDto, Publisher>();

        config.NewConfig<PublisherUpdateDto, Publisher>()
            .Ignore(dest => dest.Id);
    }
}