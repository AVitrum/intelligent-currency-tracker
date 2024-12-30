using Application.ExchangeRates.Results;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExchangeRateController : ControllerBase
{
    private readonly IExchangeRateService _exchangeRateService;

    public ExchangeRateController(IExchangeRateService exchangeRateService)
    {
        _exchangeRateService = exchangeRateService;
    }
    
    [HttpPost("fetch")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> FetchExchangeRatesAsync(ExchangeRatesRangeDto dto)
    {
        BaseResult result = await _exchangeRateService.FetchExchangeRatesAsync(dto);
        return result.Success ? Ok() : BadRequest(result.Errors);
    }
    
    [HttpGet("getRange")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRangeAsync([FromQuery] ExchangeRatesRangeDto dto)
    {
        BaseResult result = await _exchangeRateService.GetRangeAsync(dto);
        return result switch
        {
            GetExchangeRateRangeResult { Success: true } rangeResult => Ok(rangeResult.Data),
            GetExchangeRateRangeResult rangeResult when rangeResult.Equals(GetExchangeRateRangeResult.FailureNotFoundResult()) => NotFound(rangeResult.Errors),
            _ => BadRequest(result.Errors)
        };
    }
}