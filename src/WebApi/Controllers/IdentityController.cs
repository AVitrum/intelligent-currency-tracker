using Application.Common.Payload.Requests;
using Domain.Common;
using Infrastructure.Identity.Results;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IdentityController : ControllerBase
{
    private readonly IUserFactory _userFactory;

    public IdentityController(IUserFactory userFactory)
    {
        _userFactory = userFactory;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(CreateUserRequest request)
    {
        IUserService service = _userFactory.Create(request.ServiceType);
        BaseResult result = await service.CreateUserAsync(request);
        if (result.Success)
        {
            return CreatedAtAction(nameof(Register), new { request.UserName }, null);
        }

        return BadRequest(result.Errors);
    }
    
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        IUserService service = _userFactory.Create(request.ServiceType);
        BaseResult result = await service.LoginAsync(request);
        if (result is IdentityServiceResult identityServiceResult)
        {
            return Ok(new { identityServiceResult.Token });
        }

        return Unauthorized();
    }
}