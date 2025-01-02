using Application.Common.Payload.Requests;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class IdentityAdminController : ControllerBase
{
    private readonly IIdentityAdminService _identityAdminService;

    public IdentityAdminController(IIdentityAdminService identityAdminService)
    {
        _identityAdminService = identityAdminService;
    }

    [HttpPost("provide-admin-functionality")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ProvideAdminFunctionality(ProvideAdminFunctionalityRequest request)
    {
        BaseResult result = await _identityAdminService.ProvideAdminFunctionality(request);
        if (result.Success) return Ok("Admin functionality provided");

        return BadRequest(result.Errors);
    }
}