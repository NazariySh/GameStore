namespace Gamestore.Domain.Entities;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
}