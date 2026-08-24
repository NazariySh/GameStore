using Gamestore.BLL.DTOs.Games;
using Gamestore.BLL.Enums;
using Gamestore.Domain.Shared;
using Gamestore.WebApi.Models.Games;
using Mapster;

namespace Gamestore.WebApi.Mapping.Games;

public class GameModelMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<GameCreateModel, GameCreateDto>();

        config.NewConfig<GameUpdateModel, GameUpdateDto>()
            .Map(dest => dest.Id, src => EntityId.Parse(src.Id));

        config.NewConfig<CreateGameRequestModel, CreateGameRequest>()
            .Map(dest => dest.Genres, src => src.Genres.Select(EntityId.Parse).ToList())
            .Map(dest => dest.Publisher, src => EntityId.Parse(src.Publisher));

        config.NewConfig<UpdateGameRequestModel, UpdateGameRequest>()
            .Map(dest => dest.Genres, src => src.Genres.Select(EntityId.Parse).ToList())
            .Map(dest => dest.Publisher, src => EntityId.Parse(src.Publisher));

        config.NewConfig<GameDto, GameModel>()
            .Map(
                dest => dest.Id,
                src => src.Id != Guid.Empty ? src.Id.ToString() : src.ProductId.ToString());

        config.NewConfig<PagedGameList<GameDto>, PagedGameList<GameModel>>();

        config.NewConfig<GameQueryModel, GameQueryDto>()
            .Map(dest => dest.PageSize, src => PaginationOption.FromDisplayName(src.PageCount).PageSize);
    }
}