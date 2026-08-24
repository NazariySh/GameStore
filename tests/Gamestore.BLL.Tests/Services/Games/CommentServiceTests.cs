using FluentValidation;
using Gamestore.BLL.DTOs.Games.Comments;
using Gamestore.BLL.Interfaces;
using Gamestore.BLL.Interfaces.Logging;
using Gamestore.BLL.Interfaces.Users;
using Gamestore.BLL.Services;
using Gamestore.BLL.Services.Games;
using Gamestore.BLL.Tests.Extensions;
using Gamestore.BLL.Tests.Factories;
using Gamestore.BLL.Tests.TestData;
using Gamestore.BLL.Tests.TestData.Games;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.DAL.Services.Interfaces;
using Gamestore.Domain.Constants;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Enums;
using Gamestore.Domain.Exceptions;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gamestore.BLL.Tests.Services.Games;

public class CommentServiceTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Comment> _commentRepository;
    private readonly IRepository<Game> _gameRepository;
    private readonly IMapper _mapper;
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly Mock<IUserBanService> _mockUserBanService;
    private readonly Mock<IUserContext> _mockUserContext;
    private readonly CommentService _commentService;

    public CommentServiceTests()
    {
        _mockUserContext = new Mock<IUserContext>();
        _mockUserContext.SetupHasPermission(Permissions.ManageCommentsForDeletedGames, false);
        _mockUserContext.SetupHasPermission(Permissions.ViewDeletedGames, false);

        _unitOfWork = UnitOfWorkFactory.Create(_mockUserContext.Object);
        _commentRepository = _unitOfWork.Repositories.GetGeneric<Comment>();
        _gameRepository = _unitOfWork.Repositories.GetGeneric<Game>();

        _mapper = MapperFactory.Create();
        _mockValidationService = new Mock<IValidationService>();
        _mockUserBanService = new Mock<IUserBanService>();

        var mockEntityChangeLogService = new Mock<IEntityChangeLogService>();

        _commentService = new CommentService(
            new ServiceContext(_unitOfWork, _mapper, _mockValidationService.Object, _mockUserContext.Object),
            _mockUserBanService.Object,
            mockEntityChangeLogService.Object,
            Mock.Of<ILogger<CommentService>>());
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task GetAllAsync_ShouldThrowArgumentException_WhenGameKeyIsInvalid(string invalidGameKey)
    {
        var act = () => _commentService.GetAllAsync(invalidGameKey);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task GetAllAsync_ShouldThrowGameNotFoundException_WhenGameDoesNotExist()
    {
        const string gameKey = "non-existent-game";

        var act = () => _commentService.GetAllAsync(gameKey);

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]
    public async Task GetAllAsync_ShouldThrowGameNotFoundException_WhenGameIsDeletedAndUserCannotViewDeletedGames()
    {
        var game = await SeedDeletedGameAsync();

        var act = () => _commentService.GetAllAsync(game.Key);

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnCommentsHierarchy_WhenCommentsExistForGameWithValidGuid()
    {
        var game = await SeedGameAsync();
        var comments = await SeedCommentsWithChildComments(game.Id);

        var commentHierarchyDtos = _mapper.Map<List<CommentTreeDto>>(comments);

        var result = await _commentService.GetAllAsync(game.Key);

        Assert.NotEmpty(result);
        Assert.Equivalent(commentHierarchyDtos, result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnCommentsHierarchy_WhenCommentsExistForGameWithValidIntId()
    {
        var game = await SeedGameAsync();
        var comments = await SeedCommentsWithChildComments(game.ProductId!.Value);

        var commentHierarchyDtos = _mapper.Map<List<CommentTreeDto>>(comments);

        var result = await _commentService.GetAllAsync(game.Key);

        Assert.NotEmpty(result);
        Assert.Equivalent(commentHierarchyDtos, result);
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task CreateAsync_ShouldThrowArgumentException_WhenGameKeyIsInvalid(string invalidGameKey)
    {
        var userName = CommentTestData.UserName;
        var request = CommentTestData.GetCreateRequest();

        var act = () => _commentService.CreateAsync(request, invalidGameKey, userName);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task CreateAsync_ShouldThrowArgumentException_WhenUserNameIsInvalid(string invalidUserName)
    {
        var game = await SeedGameAsync();
        var request = CommentTestData.GetCreateRequest();

        var act = () => _commentService.CreateAsync(request, game.Key, invalidUserName);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowGameNotFoundException_WhenGameDoesNotExist()
    {
        var userName = CommentTestData.UserName;
        const string gameKey = "non-existent-game";

        var request = CommentTestData.GetCreateRequest();

        var act = () => _commentService.CreateAsync(request, gameKey, userName);

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowGameNotFoundException_WhenGameIsDeletedAndUserCannotViewDeletedGames()
    {
        var userName = CommentTestData.UserName;
        var game = await SeedDeletedGameAsync();
        var request = CommentTestData.GetCreateRequest();

        var act = () => _commentService.CreateAsync(request, game.Key, userName);

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowUserBannedException_WhenUserIsBanned()
    {
        var userName = CommentTestData.UserName;
        var game = await SeedGameAsync();

        var request = CommentTestData.GetCreateRequest();

        SetupMockUserBanServiceIsUserBanned(true);

        var act = () => _commentService.CreateAsync(request, game.Key, userName);

        await Assert.ThrowsAsync<UserBannedException>(act);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenRequestIsInvalid()
    {
        var userName = CommentTestData.UserName;
        var game = await SeedGameAsync();

        var request = CommentTestData.GetInvalidCreateRequest();

        SetupMockUserBanServiceIsUserBanned(false);
        _mockValidationService.SetupValidationThrows(request);

        var act = () => _commentService.CreateAsync(request, game.Key, userName);

        await Assert.ThrowsAsync<ValidationException>(act);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowForbiddenException_WhenGameIsDeletedAndUserCannotManageCommentsForDeletedGames()
    {
        var userName = CommentTestData.UserName;
        var game = await SeedDeletedGameAsync();
        var request = CommentTestData.GetCreateRequest();

        SetupMockUserBanServiceIsUserBanned(false);
        _mockUserContext.SetupHasPermission(Permissions.ViewDeletedGames, true);
        _mockUserContext.SetupHasPermission(Permissions.ManageCommentsForDeletedGames, false);

        var act = () => _commentService.CreateAsync(request, game.Key, userName);

        await Assert.ThrowsAsync<ForbiddenException>(act);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateComment_WhenRequestIsValid()
    {
        var userName = CommentTestData.UserName;
        var game = await SeedGameAsync();
        var gameCommentCount = game.CommentCount;
        var request = CommentTestData.GetCreateRequest();

        SetupMockUserBanServiceIsUserBanned(false);

        var result = await _commentService.CreateAsync(request, game.Key, userName);

        Assert.NotNull(result);

        var createdComment = await GetCommentAsync(result.Id);

        Assert.NotNull(createdComment);
        Assert.Equal(userName, createdComment.Name);
        Assert.Equal(request.Comment.Body.Trim(), createdComment.Body);
        Assert.Equal(game.Id, createdComment.GameId);
        Assert.Equal(CommentType.Root, createdComment.Type);
        Assert.Null(createdComment.ParentCommentId);
        Assert.True(await CheckGameCommentsCountAsync(game.Id, gameCommentCount + 1));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowNotFoundException_WhenParentCommentDoesNotExist()
    {
        var userName = CommentTestData.UserName;
        var game = await SeedGameAsync();
        var parentCommentId = Guid.NewGuid();

        var request = CommentTestData.GetCreateReplyRequest(parentCommentId);

        SetupMockUserBanServiceIsUserBanned(false);

        var act = () => _commentService.CreateAsync(request, game.Key, userName);

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateReplyComment_WhenRequestIsValidAndActionIsReply()
    {
        var userName = CommentTestData.UserName;
        var game = await SeedGameAsync();
        var gameCommentCount = game.CommentCount;
        var parentComment = await SeedCommentAsync(game.Id);

        var request = CommentTestData.GetCreateReplyRequest(parentComment.Id);

        SetupMockUserBanServiceIsUserBanned(false);

        var result = await _commentService.CreateAsync(request, game.Key, userName);

        Assert.NotNull(result);

        var createdComment = await GetCommentAsync(result.Id);

        Assert.NotNull(createdComment);
        Assert.Equal(parentComment.Id, createdComment.ParentCommentId);
        Assert.Equal(CommentType.Reply, createdComment.Type);
        Assert.Equal($"[{parentComment.Name}], {request.Comment.Body.Trim()}", createdComment.Body);
        Assert.True(await CheckGameCommentsCountAsync(game.Id, gameCommentCount + 1));
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateQuoteComment_WhenRequestIsValidAndActionIsQuote()
    {
        var userName = CommentTestData.UserName;
        var game = await SeedGameAsync();
        var gameCommentCount = game.CommentCount;
        var parentComment = await SeedCommentAsync(game.Id);

        var request = CommentTestData.GetCreateQuoteRequest(parentComment.Id);

        SetupMockUserBanServiceIsUserBanned(false);

        var result = await _commentService.CreateAsync(request, game.Key, userName);

        Assert.NotNull(result);

        var createdComment = await GetCommentAsync(result.Id);

        Assert.NotNull(createdComment);
        Assert.Equal(parentComment.Id, createdComment.ParentCommentId);
        Assert.Equal(CommentType.Quote, createdComment.Type);
        Assert.Contains($"[{parentComment.Body}], {request.Comment.Body.Trim()}", createdComment.Body);
        Assert.True(await CheckGameCommentsCountAsync(game.Id, gameCommentCount + 1));
    }

    [Theory]
    [ClassData(typeof(InvalidGuidTestData))]
    public async Task DeleteAsync_ShouldThrowArgumentException_WhenIdIsInvalid(Guid invalidId)
    {
        var game = await SeedGameAsync();

        var act = () => _commentService.DeleteAsync(invalidId, game.Key);

        await Assert.ThrowsAsync<ArgumentException>(act);
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task DeleteAsync_ShouldThrowArgumentException_WhenGameKeyIsInvalid(string invalidGameKey)
    {
        var commentId = Guid.NewGuid();

        var act = () => _commentService.DeleteAsync(commentId, invalidGameKey);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowGameNotFoundException_WhenGameDoesNotExist()
    {
        const string gameKey = "non-existent-game";
        var commentId = Guid.NewGuid();

        var act = () => _commentService.DeleteAsync(commentId, gameKey);

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowGameNotFoundException_WhenGameIsDeletedAndUserCannotViewDeletedGames()
    {
        var game = await SeedDeletedGameAsync();
        var commentId = Guid.NewGuid();

        var act = () => _commentService.DeleteAsync(commentId, game.Key);

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowForbiddenException_WhenGameIsDeletedAndUserCannotManageCommentsForDeletedGames()
    {
        var game = await SeedDeletedGameAsync();
        var commentId = Guid.NewGuid();

        _mockUserContext.SetupHasPermission(Permissions.ViewDeletedGames, true);
        _mockUserContext.SetupHasPermission(Permissions.ManageCommentsForDeletedGames, false);

        var act = () => _commentService.DeleteAsync(commentId, game.Key);

        await Assert.ThrowsAsync<ForbiddenException>(act);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowNotFoundException_WhenCommentDoesNotExist()
    {
        var game = await SeedGameAsync();
        var commentId = Guid.NewGuid();

        var act = () => _commentService.DeleteAsync(commentId, game.Key);

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowNotFoundException_WhenCommentIsSoftDeleted()
    {
        var game = await SeedGameAsync();
        var comment = await SeedDeletedCommentAsync(game.Id);

        var act = () => _commentService.DeleteAsync(comment.Id, game.Key);

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task DeleteAsync_ShouldUpdateCommentBody_WhenCommentExists()
    {
        var game = await SeedGameAsync();
        var gameCommentCount = game.CommentCount;
        var comment = await SeedCommentAsync(game.Id);

        await _commentService.DeleteAsync(comment.Id, game.Key);

        var deletedComment = await GetCommentAsync(comment.Id);

        Assert.NotNull(deletedComment);
        Assert.Equal(CommentService.DeleteMessage, deletedComment.Body);
        Assert.True(await CheckGameCommentsCountAsync(game.Id, gameCommentCount - 1));
    }

    [Fact]
    public async Task DeleteAsync_ShouldUpdateQuotedComments_WhenCommentHasQuotes()
    {
        var game = await SeedGameAsync();
        var parentComment = await SeedCommentAsync(game.Id);
        var parentCommentBody = parentComment.Body;
        var quoteComment = await SeedQuoteCommentAsync(game.Id, parentComment);

        await _commentService.DeleteAsync(parentComment.Id, game.Key);

        var updatedQuoteComment = await GetCommentAsync(quoteComment.Id);

        Assert.NotNull(updatedQuoteComment);
        Assert.Contains($"[{CommentService.DeleteMessage}]", updatedQuoteComment.Body);
        Assert.DoesNotContain(parentCommentBody, updatedQuoteComment.Body);
    }

    private void SetupMockUserBanServiceIsUserBanned(bool isBanned)
    {
        _mockUserBanService.Setup(s => s.IsUserBannedAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(isBanned);
    }

    private Task<bool> CheckGameCommentsCountAsync(Guid gameId, int count)
    {
        return _gameRepository.ExistsAsync(g => g.Id == gameId && g.CommentCount == count);
    }

    private Task<Comment?> GetCommentAsync(Guid commentId)
    {
        return _commentRepository.GetByIdAsync(commentId);
    }

    private async Task<Comment> SeedCommentAsync(Guid gameId)
    {
        var comment = CommentTestData.GetComment(gameId);
        await _commentRepository.AddAsync(comment);
        await _unitOfWork.SaveChangesAsync();
        return comment;
    }

    private async Task<Comment> SeedDeletedCommentAsync(Guid gameId)
    {
        var comment = CommentTestData.GetComment(gameId);
        comment.Body = CommentService.DeleteMessage;
        comment.IsDeleted = true;
        await _commentRepository.AddAsync(comment);
        await _unitOfWork.SaveChangesAsync();
        return comment;
    }

    private async Task<Comment> SeedQuoteCommentAsync(Guid gameId, Comment parentComment)
    {
        var comment = CommentTestData.GetQuoteComment(gameId, parentComment);
        await _commentRepository.AddAsync(comment);
        await _unitOfWork.SaveChangesAsync();
        return comment;
    }

    private async Task<Game> SeedDeletedGameAsync()
    {
        var game = GameTestData.GetGame();
        game.IsDeleted = true;
        await _gameRepository.AddAsync(game);
        await _unitOfWork.SaveChangesAsync();
        return game;
    }

    private async Task<Game> SeedGameAsync()
    {
        var game = GameTestData.GetGame();
        await _gameRepository.AddAsync(game);
        await _unitOfWork.SaveChangesAsync();
        return game;
    }

    private async Task<List<Comment>> SeedCommentsWithChildComments(Guid gameId)
    {
        var comments = CommentTestData.CommentsWithChildComments(gameId: gameId);
        await _commentRepository.AddRangeAsync(comments);
        await _unitOfWork.SaveChangesAsync();
        return comments;
    }

    private async Task<List<Comment>> SeedCommentsWithChildComments(int productId)
    {
        var comments = CommentTestData.CommentsWithChildComments(productId: productId);
        await _commentRepository.AddRangeAsync(comments);
        await _unitOfWork.SaveChangesAsync();
        return comments;
    }
}