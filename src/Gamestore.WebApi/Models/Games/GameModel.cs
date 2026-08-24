namespace Gamestore.WebApi.Models.Games;

public class GameModel
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string Key { get; set; }

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int UnitInStock { get; set; }

    public int Discount { get; set; }

    public DateTime CreatedAt { get; set; }
}