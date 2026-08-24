namespace Gamestore.BLL.DTOs.Games.Comments;

public record CommentDto
{
    public Guid Id { get; init; }

    public string Name { get; init; }

    public string Body { get; init; }

    public Guid? ParentCommentId { get; init; }
}