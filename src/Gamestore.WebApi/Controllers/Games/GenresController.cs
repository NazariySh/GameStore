using Gamestore.BLL.DTOs.Games.Genres;
using Gamestore.BLL.Interfaces.Games;
using Gamestore.WebApi.Constants;
using Gamestore.WebApi.Models.Games;
using Gamestore.WebApi.Models.Games.Genres;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers.Games;

public class GenresController : BaseApiController
{
    private readonly IGenreService _genreService;
    private readonly IMapper _mapper;

    public GenresController(
        IGenreService genreService,
        IMapper mapper)
    {
        _genreService = genreService;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<GenreModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var genres = await _genreService.GetAllAsync(cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<GenreModel>>(genres));
    }

    [HttpGet("{id}/[controller]")]
    [ProducesResponseType(typeof(IReadOnlyList<GenreModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubGenres(string id, CancellationToken cancellationToken)
    {
        var genres = await _genreService.GetSubGenresAsync(id, cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<GenreModel>>(genres));
    }

    [HttpGet("{id}/games")]
    [ProducesResponseType(typeof(IReadOnlyList<GameModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGenreGames(string id, CancellationToken cancellationToken)
    {
        var games = await _genreService.GetGenreGamesAsync(id, cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<GameModel>>(games));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GenreDetailedModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var genre = await _genreService.GetByIdAsync(id, cancellationToken);
        return Ok(_mapper.Map<GenreDetailedModel>(genre));
    }

    [Authorize(Policy = CustomPolicyNames.CanManageGenres)]
    [HttpPost]
    [ProducesResponseType(typeof(CreateGenreResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(CreateGenreRequestModel model, CancellationToken cancellationToken)
    {
        var request = _mapper.Map<CreateGenreRequest>(model);
        return Ok(await _genreService.CreateAsync(request, cancellationToken));
    }

    [Authorize(Policy = CustomPolicyNames.CanManageGenres)]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(UpdateGenreRequestModel model, CancellationToken cancellationToken)
    {
        var request = _mapper.Map<UpdateGenreRequest>(model);
        await _genreService.UpdateAsync(request, cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = CustomPolicyNames.CanManageGenres)]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await _genreService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
