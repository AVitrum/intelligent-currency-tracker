using Application.Common.Interfaces.Services;
using Domain.Common;
using Infrastructure.Identity.Traceable.Results;
using Infrastructure.Utils.Extensions;
using Microsoft.AspNetCore.Authorization;
using Shared.Payload.Requests;
using Shared.Payload.Responses.UserRate;

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
    public async Task<ActionResult<BaseResponse>> TrackCurrency([FromBody] TrackCurrencyRequest request)
    {
        BaseResponse response;
        string userId = _httpContextAccessor.HttpContext?.User.GetUserId()!;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        BaseResult serviceResult = await _traceableCurrencyService.TrackCurrencyAsync(userId, request.Currency);

        if (serviceResult.Success)
        {
            response = new TrackCurrencyResponse(
                true,
                $"Currency {request.Currency} tracked successfully.",
                StatusCodes.Status200OK,
                new List<string>());

            return Ok(response);
        }

        response = new TrackCurrencyResponse(
            false,
            $"Failed to track currency {request.Currency}.",
            StatusCodes.Status400BadRequest,
            serviceResult.Errors);

        return BadRequest(response);
    }

    [HttpGet("tracked-currencies")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BaseResponse>> GetTrackedCurrencies()
    {
        BaseResponse response;
        string userId = _httpContextAccessor.HttpContext?.User.GetUserId()!;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        BaseResult serviceResult = await _traceableCurrencyService.GetTrackedCurrenciesAsync(userId);

        if (serviceResult.Success == false)
        {
            response = new GetTrackedCurrenciesResponse(
                false,
                "Failed to get tracked currencies.",
                StatusCodes.Status400BadRequest,
                serviceResult.Errors,
                null);

            return NotFound(response);
        }

        if (serviceResult is not GetAllResult getAllResult)
        {
            response = new GetTrackedCurrenciesResponse(
                false,
                "Failed to get tracked currencies.",
                StatusCodes.Status400BadRequest,
                serviceResult.Errors,
                null);

            return BadRequest(response);
        }

        response = new GetTrackedCurrenciesResponse(
            true,
            "Tracked currencies retrieved successfully.",
            StatusCodes.Status200OK,
            new List<string>(),
            getAllResult.Data);

        return Ok(response);
    }

    [HttpDelete("remove-tracked-currency")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BaseResponse>> RemoveTrackedCurrencies(
        [FromBody] RemoveTrackedCurrencyRequest request)
    {
        BaseResponse response;
        string userId = _httpContextAccessor.HttpContext?.User.GetUserId()!;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        BaseResult serviceResult = await _traceableCurrencyService.RemoveTrackedCurrency(userId, request.CurrencyCode);

        if (serviceResult.Success == false)
        {
            response = new RemoveTrackedCurrencyResponse(
                false,
                "Failed to remove tracked currencies.",
                StatusCodes.Status400BadRequest,
                serviceResult.Errors);

            return NotFound(response);
        }

        response = new RemoveTrackedCurrencyResponse(
            true,
            "Tracked currency removed successfully.",
            StatusCodes.Status200OK,
            new List<string>());

        return Ok(response);
    }
}