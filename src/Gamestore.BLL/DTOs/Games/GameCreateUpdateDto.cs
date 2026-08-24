namespace Gamestore.BLL.DTOs.Games;

public abstract record GameCreateUpdateDto
{
    public string Name { get; init; }

    public string? Description { get; init; }

    public decimal Price { get; init; }

    public int UnitInStock { get; init; }

    public int Discount { get; init; }
}