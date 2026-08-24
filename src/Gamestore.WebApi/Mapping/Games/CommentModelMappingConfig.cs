using Gamestore.BLL.DTOs.Games.Comments;
using Gamestore.WebApi.Models.Games.Comments;
using Gamestore.WebApi.Utilities;
using Mapster;

namespace Gamestore.WebApi.Mapping.Games;

public class CommentModelMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateCommentRequestModel, CreateCommentRequest>()
            .Map(dest => dest.ParentId, src => src.ParentId.ParseGuid())
            .Map(dest => dest.Action, src => src.Action.ParseEnum<CommentAction>());
    }
}