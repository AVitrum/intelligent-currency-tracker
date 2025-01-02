using Application.Common.Payload.Dtos;
using Application.Common.Payload.Requests;
using Application.ExchangeRates.Results;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CsvController : ControllerBase
{
    private readonly ICsvExchangeRateService _csvExchangeRateService;

    public CsvController(ICsvExchangeRateService csvExchangeRateService)
    {
        _csvExchangeRateService = csvExchangeRateService;
    }

    [HttpPost("upload")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UploadAsync([FromForm] CsvFileUploadDto dto)
    {
        BaseResult result = await _csvExchangeRateService.GetExchangeRatesFromCsvAsync(dto.File);
        return result.Success ? Ok() : BadRequest(result.Errors);
    }

    [HttpGet("export")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExportAsync([FromQuery] ExchangeRateRequest request)
    {
        BaseResult result = await _csvExchangeRateService.ExportExchangeRatesToCsvAsync(request);
        if (result is ExportExchangeRatesToCsvResult exportResult)
            return File(exportResult.FileContent, "text/csv", exportResult.FileName);

        return BadRequest(result.Errors);
    }
}