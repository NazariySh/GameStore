namespace Gamestore.BLL.DTOs.Games.Genres;

public record UpdateGenreRequest
{
    public GenreUpdateDto Genre { get; init; }
}