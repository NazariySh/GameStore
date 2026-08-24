namespace Gamestore.BLL.DTOs.Games;

public class GameDetailedDto
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string Key { get; set; }

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int UnitInStock { get; set; }

    public int Discount { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? Publisher { get; set; }

    public int? Supplier { get; set; }

    public int? Category { get; set; }

    public ICollection<Guid> Genres { get; set; }

    public ICollection<Guid> Platforms { get; set; }
}