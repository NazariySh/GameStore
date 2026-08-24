using Gamestore.BLL.DTOs.Games.Platforms;
using Gamestore.BLL.Interfaces.Games;
using Gamestore.WebApi.Constants;
using Gamestore.WebApi.Models.Games;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers.Games;

public class PlatformsController : BaseApiController
{
    private readonly IPlatformService _platformService;
    private readonly IMapper _mapper;

    public PlatformsController(
        IPlatformService platformService,
        IMapper mapper)
    {
        _platformService = platformService;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PlatformDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _platformService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}/games")]
    [ProducesResponseType(typeof(IReadOnlyList<GameModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlatformGames(Guid id, CancellationToken cancellationToken)
    {
        var games = await _platformService.GetPlatformGamesAsync(id, cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<GameModel>>(games));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PlatformDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _platformService.GetByIdAsync(id, cancellationToken));
    }

    [Authorize(Policy = CustomPolicyNames.CanManagePlatforms)]
    [HttpPost]
    [ProducesResponseType(typeof(CreatePlatformResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(CreatePlatformRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _platformService.CreateAsync(request, cancellationToken));
    }

    [Authorize(Policy = CustomPolicyNames.CanManagePlatforms)]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(UpdatePlatformRequest request, CancellationToken cancellationToken)
    {
        await _platformService.UpdateAsync(request, cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = CustomPolicyNames.CanManagePlatforms)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _platformService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}