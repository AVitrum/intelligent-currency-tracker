using Domain.Enums;
using Infrastructure.Identity.Results;
using Microsoft.AspNetCore.Authorization;
using Shared.Dtos;
using Shared.Payload.Requests;
using Shared.Payload.Responses;

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
        var result = await service.CreateAsync(dto);

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

        if (result is UserServiceResult identityServiceResult)
            return Ok(new LoginResponse(identityServiceResult.Token));

        return Unauthorized();
    }

    [HttpPost("change-role")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddRole(ChangeRoleRequest request)
    {
        var service = _userFactory.Create(UserServiceProvider.ADMIN);
        var result = await service.ChangeRoleAsync(request);

        if (result.Success) return Ok();

        return NotFound(result.Errors);
    }

    [HttpGet("get-all-users")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var service = _userFactory.Create(UserServiceProvider.ADMIN);
        var result = await service.GetAllAsync(page, pageSize);

        if (result is GetUserResult getUserResult) return Ok(getUserResult.Data);

        return NotFound(result.Errors);
    }
}