namespace Gamestore.WebApi.Models.Games.Genres;

public record UpdateGenreRequestModel
{
    public GenreUpdateModel Genre { get; init; }
}