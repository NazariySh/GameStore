using Gamestore.BLL.DTOs.Games.Comments;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Enums;
using Mapster;

namespace Gamestore.BLL.Mapping.Games;

public class CommentMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Comment, Comment>()
            .Ignore(dest => dest.ChildComments)
            .Ignore(dest => dest.ParentComment);

        config.NewConfig<Comment, CommentDto>();

        config.NewConfig<CommentDto, CommentTreeDto>();

        config.NewConfig<CommentCreateDto, Comment>();

        config.NewConfig<CreateCommentRequest, Comment>()
            .Map(dest => dest, src => src.Comment)
            .Map(dest => dest.ParentCommentId, src => src.ParentId)
            .Map(dest => dest.Type, src => MapToCommentType(src.Action));
    }

    private static CommentType MapToCommentType(CommentAction? action)
    {
        return action switch
        {
            CommentAction.Reply => CommentType.Reply,
            CommentAction.Quote => CommentType.Quote,
            _ => CommentType.Root,
        };
    }
}