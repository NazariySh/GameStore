namespace Gamestore.BLL.DTOs.Games;

public sealed class GameDto : IEquatable<GameDto>
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Key { get; set; }

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int UnitInStock { get; set; }

    public int Discount { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? ProductId { get; set; }

    public bool Equals(GameDto? other)
    {
        return other is not null && (ReferenceEquals(this, other) || Key == other.Key);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as GameDto);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Key);
    }
}