namespace Gamestore.BLL.DTOs.Games.Genres;

public sealed class GenreDto : IEquatable<GenreDto>
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public int? CategoryId { get; set; }

    public bool Equals(GenreDto? other)
    {
        return other is not null && (ReferenceEquals(this, other) || Name == other.Name);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as GenreDto);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name);
    }
}