namespace Gamestore.BLL.DTOs.Games.Comments;

public record CreateCommentRequest
{
    public CommentCreateDto Comment { get; init; }

    public Guid? ParentId { get; init; }

    public CommentAction? Action { get; init; }
}