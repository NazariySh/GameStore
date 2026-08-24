namespace Gamestore.BLL.DTOs.Games.Genres;

public record CreateGenreRequest
{
    public GenreCreateDto Genre { get; init; }
}