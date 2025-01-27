using Domain.Enums;
using Infrastructure.Identity.Results;
using Microsoft.AspNetCore.Authorization;
using Shared.Payload;

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
    public async Task<IActionResult> Register(CreateUserDto dto)
    {
        if (dto.ServiceProvider.Equals(UserServiceProvider.ADMIN) && !User.IsInRole("Admin"))
            return Unauthorized("You are not authorized to create an admin user");

        var service = _userFactory.Create(dto.ServiceProvider);
        var result = await service.CreateUserAsync(dto);
        if (result.Success) return CreatedAtAction(nameof(Register), new { dto.UserName }, null);

        return BadRequest(result.Errors);
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var service = _userFactory.Create(UserServiceProvider.DEFAULT);
        var result = await service.LoginAsync(request);
        if (result is IdentityServiceResult identityServiceResult) return Ok(new { identityServiceResult.Token });

        return Unauthorized();
    }

    //TODO: Add more roles
    [HttpPost("add-role")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddRole(ProvideAdminFunctionalityRequest request)
    {
        var service = _userFactory.Create(UserServiceProvider.ADMIN);
        var result = await service.ProvideAdminFunctionality(request);
        if (result.Success) return Ok();

        return NotFound(result.Errors);
    }
}