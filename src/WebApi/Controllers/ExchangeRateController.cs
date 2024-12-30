using Application.ExchangeRates.Results;
using Domain.Common;

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
    
    [HttpGet("fetch")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FetchExchangeRatesAsync([FromQuery] ExchangeRatesRangeDto dto)
    {
        BaseResult result = await _exchangeRateService.FetchExchangeRatesAsync(dto);
        return result.Success ? Ok() : BadRequest(result.Errors);
    }
    
    [HttpGet("getRange")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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