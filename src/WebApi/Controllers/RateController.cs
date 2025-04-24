using Application.Common.Interfaces.Services;
using Application.Rates.Results;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Shared.Payload.Requests;

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
    
    [HttpDelete("delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteRatesAsync([FromBody] DeleteRatesRequest request)
    {
        BaseResult result = await _rateService.DeleteRatesAsync(request.Date);

        if (result.Success)
        {
            return Ok("Rates deleted successfully.");
        }
        
        return BadRequest(result);
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