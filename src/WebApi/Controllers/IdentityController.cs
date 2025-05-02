using Application.Common.Interfaces.Services;
using Domain.Common;
using Infrastructure.Identity.Results;
using Infrastructure.Utils.Extensions;
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
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IdentityController(IUserService userService, IAdminService adminService, IHttpContextAccessor httpContextAccessor)
    {
        _userService = userService;
        _adminService = adminService;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(CreateUserDto dto)
    {
        BaseResult result = await _userService.CreateAsync(dto);

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
        BaseResult result = await _userService.LoginAsync(request);

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
        BaseResult result = await _userService.LoginWithRefreshTokenAsync(request);

        if (result is UserServiceResult extendedResult)
        {
            return Ok(new LoginResponse(extendedResult.Token, extendedResult.RefreshToken));
        }

        return Unauthorized();
    }
    
    [HttpGet("profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProfile()
    {
        string userId = _httpContextAccessor.HttpContext?.User.GetUserId()!;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        BaseResult result = await _userService.GetProfileAsync(userId);

        if (result is ProfileResult profileResult)
        {
            return Ok(profileResult.Profile);
        }

        return BadRequest(result.Errors);
    }
    
    [HttpPut("upload-photo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadPhoto(IFormFile? file)
    {
        string userId = _httpContextAccessor.HttpContext?.User.GetUserId()!;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest("File is empty");
        }

        string filePath = Path.GetTempFileName();
        await using (FileStream stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        
        string fileExtension = Path.GetExtension(file.FileName);
        BaseResult result = await _userService.UploadPhotoAsync(filePath, fileExtension, userId);
        
        if (result.Success)
        {
            return Ok("Photo uploaded successfully.");
        }

        return BadRequest(result.Errors);
    }

    [HttpPost("create-admin")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAdmin(CreateUserDto dto)
    {
        BaseResult result = await _adminService.CreateAsync(dto);

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
        BaseResult result = await _adminService.ChangeRoleAsync(request);

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
        BaseResult result = await _adminService.GetAllAsync(page, pageSize);

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
        BaseResult result = await _adminService.SearchEmailsAsync(query);

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
        BaseResult result = await _adminService.GetByIdAsync(id);

        if (result is GetUserResult getUserResult)
        {
            return Ok(getUserResult.Data);
        }

        return NotFound(result.Errors);
    }
}