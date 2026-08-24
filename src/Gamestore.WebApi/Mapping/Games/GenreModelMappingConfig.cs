using Gamestore.BLL.DTOs.Games.Genres;
using Gamestore.Domain.Shared;
using Gamestore.WebApi.Models.Games.Genres;
using Mapster;

namespace Gamestore.WebApi.Mapping.Games;

public class GenreModelMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<GenreCreateModel, GenreCreateDto>()
            .Map(dest => dest.ParentGenreId, src => EntityId.ParseOrDefault(src.ParentGenreId));

        config.NewConfig<GenreUpdateModel, GenreUpdateDto>()
            .Map(dest => dest.Id, src => EntityId.Parse(src.Id))
            .Map(dest => dest.ParentGenreId, src => EntityId.ParseOrDefault(src.ParentGenreId));

        config.NewConfig<CreateGenreRequestModel, CreateGenreRequest>();
        config.NewConfig<UpdateGenreRequestModel, UpdateGenreRequest>();

        config.NewConfig<GenreDto, GenreModel>()
            .Map(
                dest => dest.Id,
                src => src.Id != Guid.Empty ? src.Id.ToString() : src.CategoryId.ToString());

        config.NewConfig<GenreDetailedDto, GenreDetailedModel>()
            .Map(
                dest => dest.Id,
                src => src.Id != Guid.Empty ? src.Id.ToString() : src.CategoryId.ToString());
    }
}