namespace Gamestore.WebApi.Models.Games.Genres;

public record GenreCreateModel
{
    public string Name { get; init; }

    public string? ParentGenreId { get; init; }
}