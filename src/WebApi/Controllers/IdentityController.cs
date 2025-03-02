using Application.Common.Interfaces.Services;
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
    private readonly IAdminService _adminService;
    private readonly IUserService _userService;

    public IdentityController(IUserService userService, IAdminService adminService)
    {
        _userService = userService;
        _adminService = adminService;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(CreateUserDto dto)
    {
        var result = await _userService.CreateAsync(dto);

        if (result.Success)
        {
            return Created(string.Empty, null);
        }

        return Conflict(result.Errors);
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _userService.LoginAsync(request);

        if (result is UserServiceResult extendedResult)
        {
            return Ok(new LoginResponse(extendedResult.Token, extendedResult.RefreshToken));
        }

        return Unauthorized();
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
    {
        var result = await _userService.LoginWithRefreshTokenAsync(request);

        if (result is UserServiceResult extendedResult)
        {
            return Ok(new LoginResponse(extendedResult.Token, extendedResult.RefreshToken));
        }

        return Unauthorized();
    }

    [HttpPost("create-admin")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAdmin(CreateUserDto dto)
    {
        var result = await _adminService.CreateAsync(dto);

        if (result.Success)
        {
            return Created(string.Empty, null);
        }

        return Conflict(result.Errors);
    }

    [HttpPost("change-role")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddRole(ChangeRoleRequest request)
    {
        var result = await _adminService.ChangeRoleAsync(request);

        if (result.Success)
        {
            return Ok();
        }

        return NotFound(result.Errors);
    }

    [HttpGet("get-all-users")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var result = await _adminService.GetAllAsync(page, pageSize);

        if (result is GetAllUsersResult extendedResult)
        {
            return Ok(extendedResult.Data);
        }

        return NotFound(result.Errors);
    }

    [HttpGet("search-emails")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SearchEmails([FromQuery] string query)
    {
        var result = await _adminService.SearchEmailsAsync(query);

        if (result is SearchEmailsResult searchEmailsResult)
        {
            return Ok(searchEmailsResult.Data);
        }

        return NotFound(result.Errors);
    }

    [HttpGet("get-user-by-id")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById([FromQuery] string id)
    {
        var result = await _adminService.GetByIdAsync(id);

        if (result is GetUserResult getUserResult)
        {
            return Ok(getUserResult.Data);
        }

        return NotFound(result.Errors);
    }
}