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
    public async Task<IActionResult> FetchExchangeRatesAsync([FromQuery] ExchangeRatesRangeDto dto)
    {
        if (!dto.TryGetDateRange(out var start, out var end))
            return BadRequest("Invalid date format. Please use dd.MM.yyyy");
        
        var result = await _exchangeRateService.FetchExchangeRatesAsync(start, end);
        return result.Success ? Ok() : BadRequest(result.Errors);
    }
    
    [HttpPost("uploadCsv")]
    public async Task<IActionResult> UploadCsvAsync([FromForm] CsvFileUploadDto dto)
    {
        var result = await _exchangeRateService.GetExchangeRatesFromCsvAsync(dto.File);
        return result.Success ? Ok() : BadRequest(result.Errors);
    }
    
    [HttpGet("exportCsv")]
    public async Task<IActionResult> ExportCsvAsync([FromQuery] ExchangeRatesRangeDto dto)
    {
        if (!dto.TryGetDateRange(out var start, out var end) || dto.Currency is null)
            return BadRequest("Invalid date format. Please use dd.MM.yyyy");
        
        var result = await _exchangeRateService.ExportExchangeRatesToCsvAsync(start, end, dto.Currency.Value);
        
        if (result is ExportExchangeRatesToCsvResult exportResult)
            return File(exportResult.FileContent, "text/csv", exportResult.FileName);
        
        return BadRequest(result.Errors);
    }
    
}