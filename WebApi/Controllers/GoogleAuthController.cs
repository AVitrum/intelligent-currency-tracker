using Infrastructure.Identity.Results;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GoogleAuthController : ControllerBase
{
    private readonly IGoogleAuthService _googleAuthService;
    
    public GoogleAuthController(IGoogleAuthService googleAuthService)
    {
        _googleAuthService = googleAuthService;
    }

    [HttpGet("login")]
    public IActionResult Index()
    {
        return new ChallengeResult(
            GoogleDefaults.AuthenticationScheme,
            new AuthenticationProperties { RedirectUri = "/api/GoogleAuth/response" });
    }
    
    [HttpGet("response")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GoogleResponse()
    {
        var authResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        var result = await _googleAuthService.HandleGoogleResponse(authResult);
        
        if (result is not GoogleAuthResult googleAuthResult) return Unauthorized();
        
        Response.Cookies.Append("jwt", googleAuthResult.Token, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Secure = true
        });
        
        return Ok($"User authenticated successfully, {googleAuthResult.Token}");
    }
}