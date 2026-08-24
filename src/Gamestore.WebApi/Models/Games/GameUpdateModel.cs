namespace Gamestore.WebApi.Models.Games;

public record GameUpdateModel
{
    public string Id { get; init; }

    public string Key { get; init; }

    public string Name { get; init; }

    public string? Description { get; init; }

    public decimal Price { get; init; }

    public int UnitInStock { get; init; }

    public int Discount { get; init; }
}