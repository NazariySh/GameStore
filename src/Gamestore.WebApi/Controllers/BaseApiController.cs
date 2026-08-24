using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public abstract class BaseApiController : ControllerBase;
