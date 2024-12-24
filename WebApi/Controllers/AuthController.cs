using Domain.Common;
using Infrastructure.Identity.Results;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public AuthController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(CreateUserModel model)
    {
        BaseResult result = await _identityService.CreateUserAsync(model.UserName, model.Password);
        
        if (result.Success) return CreatedAtAction(nameof(Register), new { model.UserName }, null);

        return BadRequest(result.Errors);
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(CreateUserModel model)
    {
        BaseResult result = await _identityService.LoginAsync(model.UserName, model.Password);
        
        if (result is not IdentityServiceResult identityServiceResult) return Unauthorized("Invalid login attempt");

        var token = identityServiceResult.Token;
        
        return Ok(new { Token = token });
    }
}