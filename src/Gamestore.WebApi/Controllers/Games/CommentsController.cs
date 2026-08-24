using Gamestore.BLL.DTOs.Users;
using Gamestore.BLL.Interfaces;
using Gamestore.BLL.Interfaces.Users;
using Gamestore.WebApi.Constants;
using Gamestore.WebApi.Models.Users;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers.Games;

public class CommentsController : BaseApiController
{
    private readonly IUserBanService _userBanService;
    private readonly IOptionsProvider _optionsProvider;
    private readonly IMapper _mapper;

    public CommentsController(
        IUserBanService userBanService,
        IOptionsProvider optionsProvider,
        IMapper mapper)
    {
        _userBanService = userBanService;
        _optionsProvider = optionsProvider;
        _mapper = mapper;
    }

    [HttpGet("ban/durations")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public IActionResult GetBanDurations()
    {
        return Ok(_optionsProvider.GetBanDurationOptions());
    }

    [Authorize(Policy = CustomPolicyNames.CanBanUserFromCommenting)]
    [HttpPost("ban")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> BanUser(BanUserRequestModel model, CancellationToken cancellationToken)
    {
        var request = _mapper.Map<BanUserRequest>(model);
        await _userBanService.BanUserAsync(request, cancellationToken);
        return Ok();
    }
}