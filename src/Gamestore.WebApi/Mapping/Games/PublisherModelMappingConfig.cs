using Gamestore.BLL.DTOs.Games.Publishers;
using Gamestore.Domain.Shared;
using Gamestore.WebApi.Models.Games.Publishers;
using Mapster;

namespace Gamestore.WebApi.Mapping.Games;

public class PublisherModelMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PublisherCreateModel, PublisherCreateDto>();

        config.NewConfig<PublisherUpdateModel, PublisherUpdateDto>()
            .Map(dest => dest.Id, src => EntityId.Parse(src.Id));

        config.NewConfig<CreatePublisherRequestModel, CreatePublisherRequest>();
        config.NewConfig<UpdatePublisherRequestModel, UpdatePublisherRequest>();

        config.NewConfig<PublisherDto, PublisherModel>()
            .Map(
                dest => dest.Id,
                src => src.Id != Guid.Empty ? src.Id.ToString() : src.SupplierId.ToString());
    }
}