using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Gamestore.BLL.DTOs.Games.Comments;
using Gamestore.BLL.DTOs.Logging;
using Gamestore.BLL.Interfaces;
using Gamestore.BLL.Interfaces.Games;
using Gamestore.BLL.Interfaces.Logging;
using Gamestore.BLL.Interfaces.Users;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.DAL.Repositories.Interfaces.Games;
using Gamestore.DAL.Services.Interfaces;
using Gamestore.Domain.Constants;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Enums;
using Gamestore.Domain.Exceptions;
using Gamestore.Domain.Shared;
using MapsterMapper;
using Microsoft.Extensions.Logging;

namespace Gamestore.BLL.Services.Games;

public class CommentService : ICommentService
{
    public const string DeleteMessage = "A comment/quote was deleted";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Comment> _commentRepository;
    private readonly IGameRepository _gameRepository;
    private readonly IMapper _mapper;
    private readonly IValidationService _validationService;
    private readonly IUserBanService _userBanService;
    private readonly IEntityChangeLogService _entityChangeLogService;
    private readonly IUserContext _userContext;
    private readonly ILogger<CommentService> _logger;

    public CommentService(
        IServiceContext context,
        IUserBanService userBanService,
        IEntityChangeLogService entityChangeLogService,
        ILogger<CommentService> logger)
    {
        _unitOfWork = context.UnitOfWork;
        _commentRepository = _unitOfWork.Repositories.GetGeneric<Comment>();
        _gameRepository = _unitOfWork.Repositories.Get<IGameRepository>();
        _mapper = context.Mapper;
        _validationService = context.Validation;
        _userBanService = userBanService;
        _entityChangeLogService = entityChangeLogService;
        _userContext = context.UserContext;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CommentTreeDto>> GetAllAsync(string gameKey, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve all comments for game with key '{GameKey}'", gameKey);

        Guard.AgainstNullOrWhiteSpace(gameKey);

        var game = await GetGameByKeyOrThrowAsync(
            gameKey,
            g => new Game { Id = g.Id, ProductId = g.ProductId },
            cancellationToken);

        var comments = await _commentRepository.GetAllAsync<CommentDto>(
            c =>
                (game.Id != Guid.Empty && c.GameId == game.Id) ||
                (game.ProductId.HasValue && c.ProductId == game.ProductId),
            cancellationToken);

        var commentsHierarchy = BuildCommentsHierarchy(comments);

        _logger.LogInformation("Retrieved {Count} comments for game with key '{GameKey}'", commentsHierarchy.Count, gameKey);

        return commentsHierarchy;
    }

    public async Task<CreateCommentResponse> CreateAsync(
        CreateCommentRequest request,
        string gameKey,
        string userName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to create a new comment for game with key '{GameKey}'", gameKey);

        Guard.AgainstNullOrWhiteSpace(gameKey);
        Guard.AgainstNullOrWhiteSpace(userName);

        await EnsureGameExistsAsync(gameKey, cancellationToken);
        await EnsureUserNotBannedAsync(userName, cancellationToken);

        await _validationService.ValidateAndThrowAsync(request, cancellationToken);

        var game = await GetGameByKeyOrThrowAsync(gameKey, cancellationToken);

        EnsureCanManageCommentsIfGameDeleted(game);

        var newComment = _mapper.Map<Comment>(request);
        newComment.Name = userName;
        newComment.GameId = game.Id;
        newComment.ProductId = game.ProductId;
        newComment.Body = await FormatCommentBodyAsync(request, game, cancellationToken);

        await _commentRepository.AddAsync(newComment, cancellationToken);

        game.CommentCount++;

        await _gameRepository.UpdateCommentCountAsync(game, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<Comment>(LogAction.Add, newVersion: newComment), cancellationToken);

        _logger.LogInformation("Comment with Id '{CommentId}' created successfully for game with key '{GameKey}'", newComment.Id, gameKey);

        return new CreateCommentResponse
        {
            Id = newComment.Id,
        };
    }

    public async Task DeleteAsync(Guid id, string gameKey, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to delete comment with Id '{Id}' for game with key '{GameKey}'", id, gameKey);

        Guard.AgainstEmpty(id);
        Guard.AgainstNullOrWhiteSpace(gameKey);

        var game = await GetGameByKeyOrThrowAsync(gameKey, cancellationToken);

        EnsureCanManageCommentsIfGameDeleted(game);

        var comment = await GetCommentOrThrowAsync(id, game, cancellationToken);
        var oldVersion = _mapper.Map<Comment>(comment);

        await UpdateQuotesOfDeletedComment(comment, cancellationToken);
        comment.Body = DeleteMessage;
        comment.IsDeleted = true;

        await _commentRepository.UpdateAsync(comment, cancellationToken);

        game.CommentCount--;

        await _gameRepository.UpdateCommentCountAsync(game, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<Comment>(LogAction.SoftDelete, oldVersion, comment), cancellationToken);

        _logger.LogInformation("Comment with Id '{Id}' deleted successfully for game with key '{GameKey}'", id, gameKey);
    }

    private async Task UpdateQuotesOfDeletedComment(Comment comment, CancellationToken cancellationToken)
    {
        var commentQuotes = await GetCommentQuotesAsync(comment.Id, cancellationToken);

        var quotedOriginalText = FormatCommentReference(comment.Body);
        var newQuotedText = FormatCommentReference(DeleteMessage);

        foreach (var commentQuote in commentQuotes)
        {
            commentQuote.Body = commentQuote.Body.Replace(quotedOriginalText, newQuotedText);
            await _commentRepository.UpdateAsync(commentQuote, cancellationToken);
        }
    }

    private async Task<string> FormatCommentBodyAsync(CreateCommentRequest request, Game game, CancellationToken cancellationToken)
    {
        var action = request.Action;
        var parentId = request.ParentId;
        var content = request.Comment.Body.Trim();

        if (!action.HasValue || !parentId.HasValue)
        {
            return content;
        }

        var parentComment = await GetCommentOrThrowAsync(parentId.Value, game, cancellationToken);

        return action switch
        {
            CommentAction.Reply => $"{FormatCommentReference(parentComment.Name)}, {content}",
            CommentAction.Quote => $"{FormatCommentReference(parentComment.Body)}, {content}",
            _ => content,
        };
    }

    private static string FormatCommentReference(string referenceText)
    {
        return $"[{referenceText}]";
    }

    private List<CommentTreeDto> BuildCommentsHierarchy(IReadOnlyList<CommentDto> comments)
    {
        var commentTreeDtos = _mapper.Map<IReadOnlyList<CommentTreeDto>>(comments);
        var commentLookup = commentTreeDtos.ToDictionary(c => c.Id);
        var rootComments = new List<CommentTreeDto>();

        foreach (var comment in comments)
        {
            var commentTreeDto = commentLookup[comment.Id];
            var parentId = comment.ParentCommentId;

            if (parentId.HasValue && commentLookup.TryGetValue(parentId.Value, out var parentComment))
            {
                parentComment.ChildComments.Add(commentTreeDto);
            }
            else
            {
                rootComments.Add(commentTreeDto);
            }
        }

        return rootComments;
    }

    private Task<ICollection<Comment>> GetCommentQuotesAsync(Guid commentId, CancellationToken cancellationToken)
    {
        return _commentRepository.GetAllAsync(
            c => c.ParentCommentId == commentId && c.Type == CommentType.Quote && !c.IsDeleted,
            cancellationToken);
    }

    private async Task<Comment> GetCommentOrThrowAsync(Guid id, Game game, CancellationToken cancellationToken)
    {
        var comment = await _commentRepository.GetSingleAsync(
            c => c.Id == id && !c.IsDeleted &&
                 ((game.Id != Guid.Empty && c.GameId == game.Id) ||
                  (game.ProductId.HasValue && c.ProductId == game.ProductId)),
            cancellationToken: cancellationToken);

        if (comment is null)
        {
            _logger.LogError("Comment with Id '{Id}' not found for game with key '{GameKey}'", id, game.Key);
            throw new NotFoundException($"Comment with Id '{id}' not found for game with key '{game.Key}'.");
        }

        return comment;
    }

    private async Task<Game> GetGameByKeyOrThrowAsync(
        string key,
        Expression<Func<Game, Game>> selector,
        CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetFirstSelectedAsync(
            g => g.Key == key,
            selector,
            cancellationToken);

        EnsureGameExists(game, key);

        return game;
    }

    private async Task<Game> GetGameByKeyOrThrowAsync(string gameKey, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetSingleAsync(
            g => g.Key == gameKey,
            cancellationToken: cancellationToken);

        EnsureGameExists(game, gameKey);

        return game;
    }

    private async Task EnsureUserNotBannedAsync(string userName, CancellationToken cancellationToken)
    {
        if (await _userBanService.IsUserBannedAsync(userName, cancellationToken))
        {
            _logger.LogError("User '{UserName}' is banned and cannot create comments", userName);
            throw new UserBannedException(userName);
        }
    }

    private async Task EnsureGameExistsAsync(string gameKey, CancellationToken cancellationToken)
    {
        if (!await GameExistsAsync(gameKey, cancellationToken))
        {
            ThrowGameNotFound(gameKey);
        }
    }

    private void EnsureCanManageCommentsIfGameDeleted(Game game)
    {
        if (game.IsDeleted && !_userContext.HasPermission(Permissions.ManageCommentsForDeletedGames))
        {
            _logger.LogError("User does not have permission to manage comments for deleted games");
            throw new ForbiddenException("User does not have permission to manage comments for deleted games.");
        }
    }

    private void EnsureGameExists<T>([NotNull] T? game, string key)
    {
        if (game is null)
        {
            ThrowGameNotFound(key);
        }
    }

    private Task<bool> GameExistsAsync(string gameKey, CancellationToken cancellationToken)
    {
        return _gameRepository.ExistsAsync(g => g.Key == gameKey, cancellationToken);
    }

    [DoesNotReturn]
    private void ThrowGameNotFound(string key)
    {
        _logger.LogError("Game with key '{Key}' not found", key);
        throw new GameNotFoundException(key);
    }
}