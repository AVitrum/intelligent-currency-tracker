using Application.Common.Payload.Requests;
using Domain.Common;
using Infrastructure.Identity.Results;

namespace WebApi.Controllers;

//TODO: Refactor whole controller
[ApiController]
[Route("api/[controller]")]
public class IdentityController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public IdentityController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(CreateUserRequest request)
    {
        BaseResult result = await _identityService.CreateUserAsync(request);
        if (result.Success) return CreatedAtAction(nameof(Register), new { request.UserName }, null);

        return Conflict(result.Errors);
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        BaseResult result = await _identityService.LoginAsync(request);
        if (result is not IdentityServiceResult identityServiceResult) return Unauthorized("Invalid login attempt");

        string token = identityServiceResult.Token;

        return Ok(new { Token = token });
    }
}