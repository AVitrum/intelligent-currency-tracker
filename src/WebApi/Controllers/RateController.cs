using Application.Common.Interfaces.Services;
using Application.Rates.Results;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Shared.Payload.Requests;
using Shared.Payload.Responses.Rate;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RateController : ControllerBase
{
    private readonly IRateService _rateService;

    public RateController(IRateService rateService)
    {
        _rateService = rateService;
    }

    [HttpGet("get-all-currencies")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BaseResponse>> GetAllCurrencies()
    {
        BaseResponse response;
        BaseResult result = await _rateService.GetAllCurrenciesAsync();

        if (result is not GetAllCurrenciesResult getAllCurrenciesResult)
        {
            response = new GetAllCurrenciesResponse(
                false,
                "Failed to retrieve currencies.",
                StatusCodes.Status400BadRequest,
                result.Errors,
                null
            );

            return BadRequest(response);
        }

        response = new GetAllCurrenciesResponse(
            true,
            "Currencies retrieved successfully.",
            StatusCodes.Status200OK,
            new List<string>(),
            getAllCurrenciesResult.Currencies
        );

        return Ok(response);
    }

    [HttpGet("get-range")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BaseResponse>> GetRatesAsync([FromQuery] ExchangeRateRequest request)
    {
        BaseResponse response;
        BaseResult result = await _rateService.GetRatesAsync(request.Start, request.End, request.Currency, request.Page,
            request.PageSize);

        if (result is not GetRatesResult getRatesResult)
        {
            response = new GetRatesResponse(
                false,
                "Failed to retrieve rates.",
                StatusCodes.Status400BadRequest,
                result.Errors,
                null
            );

            return BadRequest(response);
        }

        response = new GetRatesResponse(
            true,
            "Rates retrieved successfully.",
            StatusCodes.Status200OK,
            new List<string>(),
            getRatesResult.Rates
        );

        return Ok(response);
    }

    [HttpDelete("delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BaseResponse>> DeleteRatesAsync([FromBody] DeleteRatesRequest request)
    {
        BaseResponse response;
        BaseResult result = await _rateService.DeleteRatesAsync(request.Date);

        if (result.Success)
        {
            response = new DeleteRatesResponse(
                true,
                "Rates deleted successfully.",
                StatusCodes.Status200OK,
                new List<string>()
            );

            return Ok(response);
        }

        response = new DeleteRatesResponse(
            false,
            "Failed to delete rates.",
            StatusCodes.Status400BadRequest,
            result.Errors
        );

        return BadRequest(response);
    }
}