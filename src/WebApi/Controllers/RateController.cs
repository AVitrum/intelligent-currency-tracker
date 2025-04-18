using Application.Common.Interfaces.Services;
using Application.ExchangeRates.Results;
using Application.Rates.Results;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Shared.Payload.Requests;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RateController : ControllerBase
{
    private readonly ICsvService _csvService;
    private readonly IRateService _rateService;

    public RateController(IRateService rateService, ICsvService csvService)
    {
        _rateService = rateService;
        _csvService = csvService;
    }

    [Authorize]
    [HttpGet("get-range")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRatesAsync([FromQuery] ExchangeRateRequest request)
    {
        BaseResult result = await _rateService.GetRatesAsync(request);

        if (result is not GetRatesResult getRatesResult)
        {
            return BadRequest(result);
        }

        return Ok(getRatesResult.Rates);
    }

    // [Authorize]
    // [HttpGet("export-rates")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // public async Task<IActionResult> ExportRatesToCsv([FromQuery] ExchangeRateRequest request)
    // {
    //     BaseResult result = await _csvService.ExportExchangeRatesToCsvAsync(
    //         request.Start, request.End, request.Currency, request.Page, request.PageSize);
    //
    //     if (result is not ExportExchangeRatesToCsvResult toCsvResult)
    //     {
    //         return BadRequest(result);
    //     }
    //
    //     return File(toCsvResult.FileContent, "text/csv", toCsvResult.FileName);
    // }
}