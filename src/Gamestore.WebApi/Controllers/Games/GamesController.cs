using Gamestore.BLL.DTOs.Games;
using Gamestore.BLL.DTOs.Games.Comments;
using Gamestore.BLL.DTOs.Games.Platforms;
using Gamestore.BLL.Interfaces;
using Gamestore.BLL.Interfaces.Games;
using Gamestore.BLL.Interfaces.Orders;
using Gamestore.WebApi.Constants;
using Gamestore.WebApi.Extensions;
using Gamestore.WebApi.Models.Games;
using Gamestore.WebApi.Models.Games.Comments;
using Gamestore.WebApi.Models.Games.Genres;
using Gamestore.WebApi.Models.Games.Publishers;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Gamestore.WebApi.Controllers.Games;

public class GamesController : BaseApiController
{
    private readonly IGameService _gameService;
    private readonly ICartService _cartService;
    private readonly ICommentService _commentService;
    private readonly IOptionsProvider _optionsProvider;
    private readonly IMapper _mapper;

    public GamesController(
        IGameService gameService,
        ICartService cartService,
        ICommentService commentService,
        IOptionsProvider optionsProvider,
        IMapper mapper)
    {
        _gameService = gameService;
        _cartService = cartService;
        _commentService = commentService;
        _optionsProvider = optionsProvider;
        _mapper = mapper;
    }

    [HttpGet("pagination-options")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public IActionResult GetPaginationOptions()
    {
        return Ok(_optionsProvider.GetPaginationOptions());
    }

    [HttpGet("sorting-options")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public IActionResult GetSortOptions()
    {
        return Ok(_optionsProvider.GetGameSortOptions());
    }

    [HttpGet("publish-date-options")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public IActionResult GetPublishDateOptions()
    {
        return Ok(_optionsProvider.GetPublishDateOptions());
    }

    [HttpGet]
    [OutputCache(NoStore = true)]
    [ProducesResponseType(typeof(PagedGameList<GameModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllFiltered([FromQuery] GameQueryModel queryModel, CancellationToken cancellationToken)
    {
        var query = _mapper.Map<GameQueryDto>(queryModel);
        var games = await _gameService.GetAllFilteredAsync(query, cancellationToken);
        return Ok(_mapper.Map<PagedGameList<GameModel>>(games));
    }

    [HttpGet("all")]
    [OutputCache(NoStore = true)]
    [ProducesResponseType(typeof(IReadOnlyList<GameModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var games = await _gameService.GetAllAsync(cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<GameModel>>(games));
    }

    [HttpGet("{key}/genres")]
    [ProducesResponseType(typeof(IReadOnlyList<GenreModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGameGenres(string key, CancellationToken cancellationToken)
    {
        var genres = await _gameService.GetGameGenresAsync(key, cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<GenreModel>>(genres));
    }

    [HttpGet("{key}/platforms")]
    [ProducesResponseType(typeof(IReadOnlyList<PlatformDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGamePlatforms(string key, CancellationToken cancellationToken)
    {
        return Ok(await _gameService.GetGamePlatformsAsync(key, cancellationToken));
    }

    [HttpGet("{key}/publisher")]
    [ProducesResponseType(typeof(PublisherModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGamePublisher(string key, CancellationToken cancellationToken)
    {
        var publisher = await _gameService.GetGamePublisherAsync(key, cancellationToken);
        return Ok(_mapper.Map<PublisherModel>(publisher));
    }

    [HttpGet("{key}/comments")]
    [OutputCache(NoStore = true)]
    [ProducesResponseType(typeof(IReadOnlyList<CommentTreeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGameComments(string key, CancellationToken cancellationToken)
    {
        return Ok(await _commentService.GetAllAsync(key, cancellationToken));
    }

    [HttpGet("{key}/image")]
    [OutputCache(NoStore = true)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGameImage(string key, CancellationToken cancellationToken)
    {
        var image = await _gameService.GetGameImageAsync(key, cancellationToken);
        return File(image.Content, image.ContentType, image.FileName);
    }

    [HttpGet("{key}")]
    [OutputCache(NoStore = true)]
    [ProducesResponseType(typeof(GameModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByKey(string key, CancellationToken cancellationToken)
    {
        var game = await _gameService.GetByKeyAsync(key, cancellationToken);
        return Ok(_mapper.Map<GameModel>(game));
    }

    [HttpGet("find/{id}")]
    [ProducesResponseType(typeof(GameModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var game = await _gameService.GetByIdAsync(id, cancellationToken);
        return Ok(_mapper.Map<GameModel>(game));
    }

    [HttpGet("{key}/file")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadGameFile(string key, CancellationToken cancellationToken)
    {
        var file = await _gameService.DownloadFileAsync(key, cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [Authorize(Policy = CustomPolicyNames.CanManageGames)]
    [HttpPost]
    [ProducesResponseType(typeof(CreateGameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(CreateGameRequestModel model, CancellationToken cancellationToken)
    {
        var request = _mapper.Map<CreateGameRequest>(model);
        return Ok(await _gameService.CreateAsync(request, cancellationToken));
    }

    [Authorize(Policy = CustomPolicyNames.CanManageGames)]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(UpdateGameRequestModel model, CancellationToken cancellationToken)
    {
        var request = _mapper.Map<UpdateGameRequest>(model);
        await _gameService.UpdateAsync(request, cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = CustomPolicyNames.CanManageGames)]
    [HttpDelete("{key}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteByKey(string key, CancellationToken cancellationToken)
    {
        await _gameService.DeleteByKeyAsync(key, cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = CustomPolicyNames.CanBuyGames)]
    [HttpPost("{key}/buy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddGameToCart(string key, CancellationToken cancellationToken)
    {
        await _cartService.AddAsync(key, User.GetId(), cancellationToken);
        return Ok();
    }

    [Authorize(Policy = CustomPolicyNames.CanCommentGames)]
    [HttpPost("{key}/comments")]
    [ProducesResponseType(typeof(CreateCommentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateComment(string key, [FromBody] CreateCommentRequestModel model, CancellationToken cancellationToken)
    {
        var request = _mapper.Map<CreateCommentRequest>(model);
        return Ok(await _commentService.CreateAsync(request, key, User.GetUserName(), cancellationToken));
    }

    [Authorize(Policy = CustomPolicyNames.CanManageGameComments)]
    [HttpDelete("{key}/comments/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteComment(string key, Guid id, CancellationToken cancellationToken)
    {
        await _commentService.DeleteAsync(id, key, cancellationToken);
        return NoContent();
    }
}
