using Application.ExchangeRates.Results;

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
        var result = await _exchangeRateService.FetchExchangeRatesAsync(dto);
        return result.Success ? Ok() : BadRequest(result.Errors);
    }
    
    
    [HttpGet("getRange")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRangeAsync([FromQuery] ExchangeRatesRangeDto dto)
    {
        var result = await _exchangeRateService.GetRangeAsync(dto);

        if (result is not GetExchangeRateRangeResult rangeResult) return BadRequest(result.Errors);

        return Ok(rangeResult.Data);
    }
}