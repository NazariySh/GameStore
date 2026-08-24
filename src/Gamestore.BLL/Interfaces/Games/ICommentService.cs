using Gamestore.BLL.DTOs.Games.Comments;

namespace Gamestore.BLL.Interfaces.Games;

public interface ICommentService
{
    Task<IReadOnlyList<CommentTreeDto>> GetAllAsync(string gameKey, CancellationToken cancellationToken = default);

    Task<CreateCommentResponse> CreateAsync(CreateCommentRequest request, string gameKey, string userName, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, string gameKey, CancellationToken cancellationToken = default);
}