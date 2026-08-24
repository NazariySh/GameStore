using System.ComponentModel.DataAnnotations;

namespace Gamestore.WebApi.Models.Games;

public class GameQueryModel
{
    public ICollection<string> Genres { get; set; } = [];

    public ICollection<Guid> Platforms { get; set; } = [];

    public ICollection<string> Publishers { get; set; } = [];

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public string? DatePublishing { get; set; }

    public string? Name { get; set; }

    public string? Sort { get; set; }

    public string? PageCount { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int Page { get; set; } = 1;
}