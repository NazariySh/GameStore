using Gamestore.BLL.DTOs.Games.Comments;

namespace Gamestore.WebApi.Models.Games.Comments;

public record CreateCommentRequestModel
{
    public CommentCreateDto Comment { get; init; }

    public string? ParentId { get; init; }

    public string? Action { get; init; }
}