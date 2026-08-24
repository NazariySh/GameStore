using Gamestore.BLL.DTOs.Games.Publishers;
using Gamestore.BLL.Interfaces.Games;
using Gamestore.WebApi.Constants;
using Gamestore.WebApi.Models.Games;
using Gamestore.WebApi.Models.Games.Publishers;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers.Games;

public class PublishersController : BaseApiController
{
    private readonly IPublisherService _publisherService;
    private readonly IMapper _mapper;

    public PublishersController(
        IPublisherService publisherService,
        IMapper mapper)
    {
        _publisherService = publisherService;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PublisherModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var publishers = await _publisherService.GetAllAsync(cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<PublisherModel>>(publishers));
    }

    [HttpGet("{companyName}/games")]
    [ProducesResponseType(typeof(IReadOnlyList<GameModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPublisherGames(string companyName, CancellationToken cancellationToken)
    {
        var games = await _publisherService.GetPublisherGamesAsync(companyName, cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<GameModel>>(games));
    }

    [HttpGet("{companyName}")]
    [ProducesResponseType(typeof(PublisherModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCompanyName(string companyName, CancellationToken cancellationToken)
    {
        var publisher = await _publisherService.GetByCompanyNameAsync(companyName, cancellationToken);
        return Ok(_mapper.Map<PublisherModel>(publisher));
    }

    [HttpGet("find/{id}")]
    [ProducesResponseType(typeof(PublisherModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var publisher = await _publisherService.GetByIdAsync(id, cancellationToken);
        return Ok(_mapper.Map<PublisherModel>(publisher));
    }

    [Authorize(Policy = CustomPolicyNames.CanManagePublishers)]
    [HttpPost]
    [ProducesResponseType(typeof(CreatePublisherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(CreatePublisherRequestModel model, CancellationToken cancellationToken)
    {
        var request = _mapper.Map<CreatePublisherRequest>(model);
        return Ok(await _publisherService.CreateAsync(request, cancellationToken));
    }

    [Authorize(Policy = CustomPolicyNames.CanManagePublishers)]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(UpdatePublisherRequestModel model, CancellationToken cancellationToken)
    {
        var request = _mapper.Map<UpdatePublisherRequest>(model);
        await _publisherService.UpdateAsync(request, cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = CustomPolicyNames.CanManagePublishers)]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await _publisherService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}