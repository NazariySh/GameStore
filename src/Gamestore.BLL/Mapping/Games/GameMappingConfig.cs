using Gamestore.BLL.DTOs.Games;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Shared;
using Mapster;

namespace Gamestore.BLL.Mapping.Games;

public class GameMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Game, Game>()
            .Ignore(dest => dest.Publisher);

        config.NewConfig<GameGenre, GameGenre>()
            .Ignore(dest => dest.Game)
            .Ignore(dest => dest.Genre);

        config.NewConfig<GamePlatform, GamePlatform>()
            .Ignore(dest => dest.Game)
            .Ignore(dest => dest.Platform);

        config.NewConfig<Game, GameDto>();

        config.NewConfig<Game, GameDetailedDto>()
            .Map(
                dest => dest.Id,
                src => src.Id != Guid.Empty ? src.Id.ToString() : src.ProductId.ToString())
            .Map(dest => dest.Publisher, src => src.PublisherId != Guid.Empty ? (Guid?)src.PublisherId : null)
            .Map(dest => dest.Supplier, src => src.SupplierId)
            .Map(dest => dest.Category, src => src.CategoryId)
            .Map(dest => dest.Genres, src => src.GameGenres.Select(gg => gg.GenreId).ToList())
            .Map(dest => dest.Platforms, src => src.GamePlatforms.Select(gp => gp.PlatformId).ToList());

        config.NewConfig<GameCreateDto, Game>();
        config.NewConfig<GameUpdateDto, Game>();

        config.NewConfig<CreateGameRequest, Game>()
            .Map(dest => dest, src => src.Game)
            .Ignore(dest => dest.Publisher)
            .Ignore(dest => dest.PublisherId)
            .AfterMapping((src, dest) => MapPublisherId(src.Publisher, dest));

        config.NewConfig<UpdateGameRequest, Game>()
            .Map(dest => dest, src => src.Game)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Publisher)
            .Ignore(dest => dest.PublisherId)
            .AfterMapping((src, dest) => MapPublisherId(src.Publisher, dest));
    }

    private static void MapPublisherId(EntityId publisherId, Game dest)
    {
        if (publisherId.IsPrimary)
        {
            dest.PublisherId = publisherId.PrimaryId.Value;
            dest.SupplierId = null;
        }
        else
        {
            dest.SupplierId = publisherId.SecondaryId.Value;
            dest.PublisherId = Guid.Empty;
        }
    }
}