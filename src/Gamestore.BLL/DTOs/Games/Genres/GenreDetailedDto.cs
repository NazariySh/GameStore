namespace Gamestore.BLL.DTOs.Games.Genres;

public class GenreDetailedDto
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public Guid? ParentGenreId { get; set; }

    public int? CategoryId { get; set; }
}