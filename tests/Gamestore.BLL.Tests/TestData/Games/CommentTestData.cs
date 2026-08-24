using Gamestore.BLL.DTOs.Games.Comments;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Enums;

namespace Gamestore.BLL.Tests.TestData.Games;

public static class CommentTestData
{
    public const string UserName = "Test User";

    public static List<Comment> CommentsWithChildComments(Guid? gameId = null, int? productId = null)
    {
        return
        [
            new Comment
            {
                Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479"),
                Name = "Parent Comment 1",
                Body = "This is the first parent comment.",
                Type = CommentType.Root,
                GameId = gameId ?? Guid.Empty,
                ProductId = productId,
                ChildComments =
                [
                    new Comment
                    {
                        Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d481"),
                        Name = "Child Comment 1.1",
                        Body = "[Parent Comment 1], This is a child comment.",
                        Type = CommentType.Reply,
                        GameId = gameId ?? Guid.Empty,
                        ProductId = productId,
                    },
                    new Comment
                    {
                        Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d485"),
                        Name = "Child Comment 1.2",
                        Body = "[This is the first parent comment.], This is another child comment.",
                        Type = CommentType.Quote,
                        GameId = gameId ?? Guid.Empty,
                        ProductId = productId,
                    },
                ],
            },
            new Comment
            {
                Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d480"),
                Name = "Parent Comment 2",
                Body = "This is the second parent comment.",
                Type = CommentType.Root,
                GameId = gameId ?? Guid.Empty,
                ProductId = productId,
            },
        ];
    }

    public static Comment GetComment(Guid? gameId = null, Guid? parentCommentId = null)
    {
        return new Comment
        {
            Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d481"),
            Name = "Test User",
            Body = "This is a test comment.",
            GameId = gameId ?? Guid.NewGuid(),
            Type = parentCommentId.HasValue ? CommentType.Reply : CommentType.Root,
            ParentCommentId = parentCommentId,
        };
    }

    public static Comment GetQuoteComment(Guid gameId, Comment parentComment)
    {
        return new Comment
        {
            Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d482"),
            Name = "QuoteUser",
            Body = $"[{parentComment.Body}], This is my response to the quote",
            Type = CommentType.Quote,
            GameId = gameId,
            ParentCommentId = parentComment.Id,
        };
    }

    public static CreateCommentRequest GetCreateRequest()
    {
        return new CreateCommentRequest
        {
            Comment = new CommentCreateDto
            {
                Body = "Test comment body",
            },
            ParentId = null,
            Action = null,
        };
    }

    public static CreateCommentRequest GetCreateReplyRequest(Guid parentId)
    {
        return new CreateCommentRequest
        {
            Comment = new CommentCreateDto
            {
                Body = "Reply to parent comment",
            },
            ParentId = parentId,
            Action = CommentAction.Reply,
        };
    }

    public static CreateCommentRequest GetCreateQuoteRequest(Guid parentId)
    {
        return new CreateCommentRequest
        {
            Comment = new CommentCreateDto
            {
                Body = "Quoted comment body",
            },
            ParentId = parentId,
            Action = CommentAction.Quote,
        };
    }

    public static CreateCommentRequest GetInvalidCreateRequest()
    {
        return new CreateCommentRequest
        {
            Comment = new CommentCreateDto
            {
                Body = string.Empty,
            },
            ParentId = null,
            Action = null,
        };
    }
}