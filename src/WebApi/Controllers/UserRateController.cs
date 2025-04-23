using Application.Common.Interfaces.Services;
using Domain.Common;
using Infrastructure.Identity.Traceable.Results;
using Infrastructure.Utils;
using Microsoft.AspNetCore.Authorization;
using Shared.Payload.Requests;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserRateController : ControllerBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITraceableCurrencyService _traceableCurrencyService;

    public UserRateController(
        ITraceableCurrencyService traceableCurrencyService,
        IHttpContextAccessor httpContextAccessor)
    {
        _traceableCurrencyService = traceableCurrencyService;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpPost("track-currency")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> TrackCurrency([FromBody] TrackCurrencyRequest request)
    {
        string userId = _httpContextAccessor.HttpContext?.User.GetUserId()!;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        BaseResult serviceResult = await _traceableCurrencyService.TrackCurrencyAsync(userId, request.R030);

        if (serviceResult.Success)
        {
            return Ok("Currency tracked successfully.");
        }

        return BadRequest(serviceResult.Errors);
    }

    [HttpGet("tracked-currencies")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTrackedCurrencies()
    {
        string userId = _httpContextAccessor.HttpContext?.User.GetUserId()!;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        BaseResult serviceResult = await _traceableCurrencyService.GetTrackedCurrenciesAsync(userId);

        if (serviceResult.Success == false)
        {
            return NotFound(serviceResult.Errors);
        }

        if (serviceResult is GetAllResult getAllResult)
        {
            return Ok(getAllResult.Data);
        }

        return BadRequest("Something went wrong.");
    }
}