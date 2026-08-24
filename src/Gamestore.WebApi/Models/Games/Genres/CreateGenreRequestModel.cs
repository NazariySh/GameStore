namespace Gamestore.WebApi.Models.Games.Genres;

public record CreateGenreRequestModel
{
    public GenreCreateModel Genre { get; init; }
}