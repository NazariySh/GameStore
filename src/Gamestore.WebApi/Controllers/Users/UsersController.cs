using Gamestore.BLL.DTOs.Auth;
using Gamestore.BLL.DTOs.Users;
using Gamestore.BLL.DTOs.Users.Roles;
using Gamestore.BLL.Interfaces.Auth;
using Gamestore.BLL.Interfaces.Users;
using Gamestore.WebApi.Constants;
using Gamestore.WebApi.Models.Auth;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers.Users;

[Authorize]
public class UsersController : BaseApiController
{
    private readonly IAuthService _authService;
    private readonly IAccessService _accessService;
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly IMapper _mapper;

    public UsersController(
        IAuthService authService,
        IAccessService accessService,
        IUserService userService,
        IRoleService roleService,
        IMapper mapper)
    {
        _authService = authService;
        _accessService = accessService;
        _userService = userService;
        _roleService = roleService;
        _mapper = mapper;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Login(LoginRequestModel model, CancellationToken cancellationToken)
    {
        var request = _mapper.Map<LoginRequest>(model.Model);

        var response = model.Model.InternalAuth
            ? await _authService.LoginAsync(request, cancellationToken)
            : await _authService.LoginWithExternalAuthAsync(request, cancellationToken);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("access")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckUserAccess(CheckAccessRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _accessService.CheckUserAccessAsync(User, request, cancellationToken));
    }

    [Authorize(Policy = CustomPolicyNames.CanManageUsers)]
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _userService.GetAllAsync(cancellationToken));
    }

    [Authorize(Policy = CustomPolicyNames.CanManageUsers)]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _userService.GetByIdAsync(id, cancellationToken));
    }

    [Authorize(Policy = CustomPolicyNames.CanManageUsers)]
    [HttpGet("{id:guid}/roles")]
    [ProducesResponseType(typeof(IReadOnlyList<RoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserRoles(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _roleService.GetUserRolesAsync(id, cancellationToken));
    }

    [Authorize(Policy = CustomPolicyNames.CanManageUsers)]
    [HttpPost]
    [ProducesResponseType(typeof(CreateUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _userService.CreateAsync(request, cancellationToken));
    }

    [Authorize(Policy = CustomPolicyNames.CanManageUsers)]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(UpdateUserRequest request, CancellationToken cancellationToken)
    {
        await _userService.UpdateAsync(request, cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = CustomPolicyNames.CanManageUsers)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _userService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}