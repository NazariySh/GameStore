namespace Gamestore.BLL.DTOs.Games.Publishers;

public sealed class PublisherDto : IEquatable<PublisherDto>
{
    public Guid Id { get; set; }

    public string CompanyName { get; set; }

    public string? HomePage { get; set; }

    public string? Description { get; set; }

    public int? SupplierId { get; set; }

    public bool Equals(PublisherDto? other)
    {
        return other is not null && (ReferenceEquals(this, other) || CompanyName == other.CompanyName);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as PublisherDto);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(CompanyName);
    }
}