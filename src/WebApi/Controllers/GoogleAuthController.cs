using Domain.Common;
using Infrastructure.ExternalApis.GoogleAuth.Results;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GoogleAuthController : ControllerBase
{
    private readonly IGoogleAuthService _service;
    
    public GoogleAuthController(IGoogleAuthService service)
    {
        _service = service;
    }

    [HttpGet("login")]
    public IActionResult Index()
    {
        return new ChallengeResult(
            GoogleDefaults.AuthenticationScheme,
            new AuthenticationProperties
            {
                RedirectUri = "/api/GoogleAuth/response"
            });
    }

    [HttpGet("response")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GoogleResponse()
    {
        
        AuthenticateResult authResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        BaseResult result = await _service.HandleGoogleResponse(authResult);
        if (result is not GoogleAuthResult googleAuthResult)
        {
            return Unauthorized();
        }

        Response.Cookies.Append("jwt", googleAuthResult.Token, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Secure = true
        });

        return Ok(googleAuthResult.Token);
    }
}