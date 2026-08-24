using Gamestore.BLL.DTOs.Games.Genres;
using Gamestore.Domain.Entities.Games;
using Mapster;

namespace Gamestore.BLL.Mapping.Games;

public class GenreMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Genre, Genre>()
            .Ignore(dest => dest.ParentGenre)
            .Ignore(dest => dest.SubGenres)
            .Ignore(dest => dest.GameGenres);

        config.NewConfig<Genre, GenreDto>();
        config.NewConfig<Genre, GenreDetailedDto>();

        config.NewConfig<GenreCreateDto, Genre>()
            .Ignore(dest => dest.ParentGenreId)
            .AfterMapping(MapParentGenreId);

        config.NewConfig<GenreUpdateDto, Genre>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.ParentGenreId)
            .AfterMapping(MapParentGenreId);
    }

    private static void MapParentGenreId(GenreCreateUpdateDto src, Genre dest)
    {
        var parentGenreId = src.ParentGenreId;
        if (!parentGenreId.HasValue)
        {
            return;
        }

        if (parentGenreId.Value.IsPrimary)
        {
            dest.ParentGenreId = parentGenreId.Value.PrimaryId;
            dest.ParentCategoryId = null;
        }
        else
        {
            dest.ParentCategoryId = parentGenreId.Value.SecondaryId;
            dest.ParentGenreId = null;
        }
    }
}