using Gamestore.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace Gamestore.Domain.Entities.Games;

public class Comment : BaseEntity, ISoftDeletable
{
    public string Name { get; set; }

    public string Body { get; set; }

    public CommentType Type { get; set; }

    public Guid? ParentCommentId { get; set; }

    public Guid GameId { get; set; }

    public int? ProductId { get; set; }

    public bool IsDeleted { get; set; }

    [BsonIgnore]
    public virtual Comment? ParentComment { get; set; }

    [BsonIgnore]
    public virtual ICollection<Comment> ChildComments { get; set; } = [];
}