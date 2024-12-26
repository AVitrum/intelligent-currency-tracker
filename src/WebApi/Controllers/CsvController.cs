using Application.ExchangeRates.Results;
using Domain.Common;

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

    [HttpPost("uploadCsv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadCsvAsync([FromForm] CsvFileUploadDto dto)
    {
        BaseResult result = await _csvExchangeRateService.GetExchangeRatesFromCsvAsync(dto.File);
        return result.Success ? Ok() : BadRequest(result.Errors);
    }
    
    [HttpGet("exportCsv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportCsvAsync([FromQuery] ExchangeRatesRangeDto dto)
    {
        BaseResult result = await _csvExchangeRateService.ExportExchangeRatesToCsvAsync(dto);
        
        if (result is ExportExchangeRatesToCsvResult exportResult)
            return File(exportResult.FileContent, "text/csv", exportResult.FileName);
        
        return BadRequest(result.Errors);
    }
}