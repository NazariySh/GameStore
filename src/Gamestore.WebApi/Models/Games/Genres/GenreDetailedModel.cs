namespace Gamestore.WebApi.Models.Games.Genres;

public class GenreDetailedModel
{
    public string Id { get; set; }

    public string Name { get; set; }

    public Guid? ParentGenreId { get; set; }
}