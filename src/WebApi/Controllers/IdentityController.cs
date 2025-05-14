using Application.Common.Interfaces.Services;
using Domain.Common;
using Infrastructure.Identity.Results;
using Infrastructure.Utils.Extensions;
using Microsoft.AspNetCore.Authorization;
using Shared.Dtos;
using Shared.Payload.Requests;
using Shared.Payload.Responses;
using Shared.Payload.Responses.Identity;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IdentityController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserService _userService;

    public IdentityController(
        IUserService userService,
        IAdminService adminService,
        IHttpContextAccessor httpContextAccessor)
    {
        _userService = userService;
        _adminService = adminService;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BaseResponse>> Register(CreateUserDto dto)
    {
        BaseResult result = await _userService.CreateAsync(dto);

        if (result.Success)
        {
            return Ok(new RegistrationResponse(
                true,
                "User created successfully",
                StatusCodes.Status201Created,
                new List<string>()));
        }

        return Conflict(new RegistrationResponse(
            false,
            "User creation failed",
            StatusCodes.Status409Conflict,
            result.Errors));
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BaseResponse>> Login(LoginRequest request)
    {
        BaseResult result = await _userService.LoginAsync(request);

        if (result is not UserServiceResult extendedResult)
        {
            return UnauthorizedResponse("Login failed");
        }

        return Ok(new LoginResponse(
            true,
            "Login successful",
            StatusCodes.Status200OK,
            new List<string>(),
            extendedResult.Token,
            extendedResult.RefreshToken));
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BaseResponse>> RefreshToken(RefreshTokenRequest request)
    {
        BaseResult result = await _userService.LoginWithRefreshTokenAsync(request);

        if (result is not UserServiceResult extendedResult)
        {
            return UnauthorizedResponse("Token refresh failed");
        }

        return Ok(new LoginResponse(
            true,
            "Token refreshed successfully",
            StatusCodes.Status200OK,
            new List<string>(),
            extendedResult.Token,
            extendedResult.RefreshToken));
    }

    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BaseResponse>> GetProfile()
    {
        string? userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return UnauthorizedResponse("User not found");
        }

        BaseResult result = await _userService.GetProfileAsync(userId);

        if (result is ProfileResult profileResult)
        {
            return Ok(new ProfileResponse(
                true,
                "Profile retrieved successfully",
                StatusCodes.Status200OK,
                new List<string>(),
                profileResult.UserId,
                profileResult.UserName,
                profileResult.Email,
                profileResult.PhoneNumber,
                profileResult.Photo));
        }

        return BadRequest(new ProfileResponse(
            false,
            "Profile retrieval failed",
            StatusCodes.Status400BadRequest,
            result.Errors,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            []));
    }

    [HttpPut("upload-photo")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BaseResponse>> UploadPhoto(IFormFile? file)
    {
        string? userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return UnauthorizedResponse("User not found");
        }

        if (file is null || file.Length == 0)
        {
            return BadRequestResponse("File is empty");
        }

        string filePath = Path.GetTempFileName();
        await using (FileStream stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        string fileExtension = Path.GetExtension(file.FileName);
        BaseResult result = await _userService.UploadPhotoAsync(filePath, fileExtension, userId);

        if (!result.Success)
        {
            return BadRequestResponse("Photo upload failed", result.Errors);
        }

        return Ok(new UploadPhotoResponse(
            true,
            "Photo uploaded successfully",
            StatusCodes.Status200OK,
            new List<string>()));
    }

    [HttpPost("save-settings")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BaseResponse>> SaveSettings(SettingsDto dto)
    {
        string? userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return UnauthorizedResponse("User not found");
        }

        BaseResult result = await _userService.SaveSettingsAsync(dto, userId);
        if (result.Success)
        {
            return Ok(new SaveSettingsResponse(
                true,
                "Settings saved successfully",
                StatusCodes.Status200OK,
                new List<string>()));
        }

        return BadRequest(new SaveSettingsResponse(
            false,
            "Settings save failed",
            StatusCodes.Status400BadRequest,
            result.Errors));
    }

    [HttpGet("get-settings")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BaseResponse>?> GetSettings()
    {
        string? userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return UnauthorizedResponse("User not found");
        }

        BaseResult result = await _userService.GetSettingsAsync(userId);
        if (result is not GetSettingsResult extendedResult)
        {
            return NotFound(new GetSettingsResponse(
                false,
                "Settings retrieval failed",
                StatusCodes.Status404NotFound,
                result.Errors,
                null));
        }

        return Ok(new GetSettingsResponse(
            true,
            "Settings retrieved successfully",
            StatusCodes.Status200OK,
            new List<string>(),
            extendedResult.Settings));
    }

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BaseResponse>> ChangePassword(ChangePasswordRequest request)
    {
        string? userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return UnauthorizedResponse("User not found");
        }

        BaseResult result = await _userService.ChangePasswordAsync(
            request.OldPassword, request.ConfirmPassword, request.NewPassword, userId);

        if (result.Success)
        {
            return Ok(new ChangePasswordResponse(
                true,
                "Password changed successfully",
                StatusCodes.Status200OK,
                new List<string>()));
        }

        return BadRequest(new ChangePasswordResponse(
            false,
            "Password change failed",
            StatusCodes.Status400BadRequest,
            result.Errors));
    }


    // [HttpPost("create-admin")]
    // [Authorize(Roles = "ADMIN")]
    // [ProducesResponseType(StatusCodes.Status201Created)]
    // [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(StatusCodes.Status409Conflict)]
    // public async Task<IActionResult> CreateAdmin(CreateUserDto dto)
    // {
    //     BaseResult result = await _adminService.CreateAsync(dto);
    //
    //     if (result.Success)
    //     {
    //         return Created(string.Empty, null);
    //     }
    //
    //     return Conflict(result.Errors);
    // }
    //
    // [HttpPost("change-role")]
    // [Authorize(Roles = "ADMIN")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // public async Task<IActionResult> AddRole(ChangeRoleRequest request)
    // {
    //     BaseResult result = await _adminService.ChangeRoleAsync(request);
    //
    //     if (result.Success)
    //     {
    //         return Ok();
    //     }
    //
    //     return NotFound(result.Errors);
    // }
    //
    // [HttpGet("get-all-users")]
    // [Authorize(Roles = "ADMIN")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    // {
    //     BaseResult result = await _adminService.GetAllAsync(page, pageSize);
    //
    //     if (result is GetAllUsersResult extendedResult)
    //     {
    //         return Ok(extendedResult.Data);
    //     }
    //
    //     return NotFound(result.Errors);
    // }
    //
    // [HttpGet("search-emails")]
    // [Authorize(Roles = "ADMIN")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // public async Task<IActionResult> SearchEmails([FromQuery] string query)
    // {
    //     BaseResult result = await _adminService.SearchEmailsAsync(query);
    //
    //     if (result is SearchEmailsResult searchEmailsResult)
    //     {
    //         return Ok(searchEmailsResult.Data);
    //     }
    //
    //     return NotFound(result.Errors);
    // }
    //
    // [HttpGet("get-user-by-id")]
    // [Authorize(Roles = "ADMIN")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // public async Task<IActionResult> GetUserById([FromQuery] string id)
    // {
    //     BaseResult result = await _adminService.GetByIdAsync(id);
    //
    //     if (result is GetUserResult getUserResult)
    //     {
    //         return Ok(getUserResult.Data);
    //     }
    //
    //     return NotFound(result.Errors);
    // }

    private string? GetUserId()
    {
        return _httpContextAccessor.HttpContext?.User.GetUserId();
    }

    private UnauthorizedObjectResult UnauthorizedResponse(string message)
    {
        return Unauthorized(new DefaultResponse(false, message, StatusCodes.Status401Unauthorized,
            new List<string> { message }));
    }

    private BadRequestObjectResult BadRequestResponse(string message, IEnumerable<string>? errors = null)
    {
        return BadRequest(new DefaultResponse(false, message, StatusCodes.Status400BadRequest,
            errors ?? new List<string> { message }));
    }
}