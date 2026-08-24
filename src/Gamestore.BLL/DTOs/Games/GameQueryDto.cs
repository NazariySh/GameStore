namespace Gamestore.BLL.DTOs.Games;

public class GameQueryDto
{
    public ICollection<string> Genres { get; set; } = [];

    public ICollection<Guid> Platforms { get; set; } = [];

    public ICollection<string> Publishers { get; set; } = [];

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public string? DatePublishing { get; set; }

    public string? Name { get; set; }

    public string? Sort { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}