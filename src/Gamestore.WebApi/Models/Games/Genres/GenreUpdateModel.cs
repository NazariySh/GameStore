namespace Gamestore.WebApi.Models.Games.Genres;

public record GenreUpdateModel : GenreCreateModel
{
    public string Id { get; init; }
}