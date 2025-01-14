using Application.Common.Payload.Requests;
using Application.Rates.Results;
using Microsoft.AspNetCore.Authorization;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RateController : ControllerBase
{
    private readonly IRateService _rateService;

    public RateController(IRateService rateService)
    {
        _rateService = rateService;
    }

    [Authorize]
    [HttpGet("get-range")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRatesAsync([FromQuery] ExchangeRateRequest request)
    {
        var result = await _rateService.GetRatesAsync(request);
        if (result is not GetRatesResult getRatesResult) return BadRequest(result);

        return Ok(getRatesResult.Rates);
    }
}